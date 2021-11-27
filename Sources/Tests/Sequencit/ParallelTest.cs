using System;
using NUnit.Framework;
using UniRx;

namespace Silphid.Sequencit.Test
{
    [TestFixture]
    public class ParallelTest : SequencingTestBase
    {
        private Parallel _parallel;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _parallel = Parallel.Create();
        }

        [Test]
        public void SubscribingToEmptySequenceCompletesImmediately()
        {
            _parallel.Subscribe(() => _value = 1);
            Assert.That(_value, Is.EqualTo(1));
        }

        [Test]
        public void StartsOnlyOnceSubscribedTo()
        {
            _parallel.AddAction(() => _value = 2);
            Assert.That(_value, Is.EqualTo(0));

            _parallel.Subscribe();
            Assert.That(_value, Is.EqualTo(2));
        }

        [Test]
        public void ParallelWithOnlyInstantObservablesIsExecutedInstantly()
        {
            _parallel.Add(Observable.ReturnUnit());
            _parallel.AddAction(() => _value = 2);
            _parallel.Add(Observable.ReturnUnit());
            _parallel.Subscribe(() => _value = 3);

            Assert.That(_value, Is.EqualTo(3));
        }

        [Test]
        public void Start_SubscribesToParallelAndExecutesIt()
        {
            Parallel.Start(
                s =>
                {
                    s.Add(Observable.ReturnUnit());
                    s.AddAction(() => _value += 123);
                    s.Add(Observable.ReturnUnit());
                    s.AddAction(() => _value += 222);
                    s.Add(Observable.ReturnUnit());
                });

            Assert.That(_value, Is.EqualTo(345));
        }

        [Test]
        public void SequenceWaitsForEachObservableToCompleteBeforeExecutingNextAction()
        {
            Sequence.Start(
                s =>
                {
                    s.AddAction(() => _value = 1);
                    s.Add(CreateTimer(10));
                    s.AddAction(() => _value = 2);
                    s.Add(CreateTimer(10));
                    s.AddAction(() => _value = 3);
                });

            Assert.That(_value, Is.EqualTo(1));

            _scheduler.AdvanceTo(10);
            Assert.That(_value, Is.EqualTo(2));

            _scheduler.AdvanceTo(20);
            Assert.That(_value, Is.EqualTo(3));
        }

        [Test]
        public void Dispose_CancelsRemainingActions()
        {
            var disposable = Sequence.Start(
                s =>
                {
                    s.AddAction(() => _value = 1);
                    s.Add(CreateTimer(10));
                    s.AddAction(() => _value = 2);
                    s.Add(CreateTimer(10));
                    s.AddAction(() => _value = 3);
                });

            Assert.That(_value, Is.EqualTo(1));

            _scheduler.AdvanceTo(10);
            Assert.That(_value, Is.EqualTo(2));

            disposable.Dispose();

            _scheduler.AdvanceTo(1000);
            Assert.That(_value, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_AlsoDisposesCurrentObservable()
        {
            var disposable = Sequence.Start(
                s =>
                {
                    s.Add(
                        () =>
                        {
                            _value = 50;
                            return Observable.Interval(TimeSpan.FromTicks(100), _scheduler)
                                             .Take(2)
                                             .Do(x => _value = (int) (100 + x))
                                             .DoOnCancel(() => _value = 777)
                                             .AsUnitObservable();
                        });
                    s.AddAction(() => _value = 200);
                });

            Assert.That(_value, Is.EqualTo(50));

            _scheduler.AdvanceTo(100);
            Assert.That(_value, Is.EqualTo(100));

            disposable.Dispose();
            Assert.That(_value, Is.EqualTo(777));
        }

        [Test]
        public void SequenceDoesNotExecuteIfNotSubscribedTo()
        {
            _parallel.AddAction(() => _value = 123);
            Assert.That(_value, Is.EqualTo(0));
        }
    }
}
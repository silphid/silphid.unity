using System;
using NUnit.Framework;
using UniRx;

namespace Silphid.Sequencit.Test
{
    [TestFixture]
    public class SequenceTest : SequencingTestBase
    {
        private Sequence _sequence;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _sequence = Sequence.Create();
        }

        [Test]
        public void SubscribingToEmptySequenceCompletesImmediately()
        {
            _sequence.Subscribe(() => _value = 1);
            Assert.That(_value, Is.EqualTo(1));
        }

        [Test]
        public void SequenceStartsOnlyOnceSubscribedTo()
        {
            _sequence.AddAction(() => _value = 2);
            Assert.That(_value, Is.EqualTo(0));

            _sequence.Subscribe();
            Assert.That(_value, Is.EqualTo(2));
        }

        [Test]
        public void SequenceWithOnlyInstantObservablesIsExecutedInstantly()
        {
            _sequence.Add(Observable.ReturnUnit());
            _sequence.AddAction(() => _value = 2);
            _sequence.Add(Observable.ReturnUnit());
            _sequence.Subscribe();

            Assert.That(_value, Is.EqualTo(2));
        }

        [Test]
        public void Start_SubscribesToSequenceAndExecutesIt()
        {
            Sequence.Start(s =>
            {
                s.Add(Observable.ReturnUnit());
                s.AddAction(() => _value = 3);
                s.Add(Observable.ReturnUnit());
            });

            Assert.That(_value, Is.EqualTo(3));
        }

        [Test]
        public void SequenceWaitsForEachObservableToCompleteBeforeExecutingNextAction()
        {
            Sequence.Start(s =>
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
            var disposable = Sequence.Start(s =>
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
            var disposable = Sequence.Start(s =>
            {
                s.Add(() =>
                {
                    _value = 50;
                    return Observable
                        .Interval(TimeSpan.FromTicks(100), _scheduler)
                        .Take(2)
                        .Do(x => _value = (int)(100 + x))
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
            _sequence.AddAction(() => _value = 123);
            Assert.That(_value, Is.EqualTo(0));
        }
    }
}
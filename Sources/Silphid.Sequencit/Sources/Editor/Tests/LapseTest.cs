using System;
using NUnit.Framework;
using UniRx;
// ReSharper disable AccessToDisposedClosure

namespace Silphid.Sequencit.Test
{
    [TestFixture]
    public class LapseTest : SequencingTestBase
    {
        [Test]
        public void AddLapse_ReturnedDisposable()
        {
            IDisposable disposable = null;
        
            Sequence.Start(s =>
            {
                s.AddAction(() => _value = 1);
                disposable = s.AddLapse();
                s.AddAction(() => _value = 2);
            });

            Assert.That(_value, Is.EqualTo(1));

            disposable.Dispose();
            Assert.That(_value, Is.EqualTo(2));
        }

        [Test]
        public void AddLapse_LambdaDisposable()
        {
            IDisposable disposable = null;
        
            Sequence.Start(s =>
            {
                s.AddAction(() => _value = 1);
                s.AddLapse(x => disposable = x);
                s.AddAction(() => _value = 2);
            });

            Assert.That(_value, Is.EqualTo(1));

            disposable.Dispose();
            Assert.That(_value, Is.EqualTo(2));
        }

        [Test]
        public void AddLapse_DisposeInLambda_DoesNotWaitAtAll()
        {
            Sequence.Start(s =>
            {
                s.AddAction(() => _value = 1);
                s.AddLapse(x => x.Dispose());
                s.AddAction(() => _value = 2);
            });

            Assert.That(_value, Is.EqualTo(2));
        }

        [Test]
        public void LambdaShouldBeCalledOnlyUponSubscription()
        {
            var lapse = new Lapse(disposable => _value = 1);

            Assert.That(_value, Is.EqualTo(0));
            
            lapse.Subscribe(_ => { });
            Assert.That(_value, Is.EqualTo(1));
        }

        [Test]
        public void AddLapse_LambdaShouldBeCalledOnlyUponSubscription()
        {
            var disposable = new SerialDisposable();
        
            Sequence.Start(s =>
            {
                s.AddAction(() => _value = 1);
                s.Add(CreateDelay(100));
                s.AddLapse(x =>
                {
                    _value = 2;
                    disposable.Disposable = x;
                });
                s.AddAction(() => _value = 3);
            });

            Assert.That(_value, Is.EqualTo(1));

            _scheduler.AdvanceTo(100);
            Assert.That(_value, Is.EqualTo(2));

            disposable.Dispose();
            Assert.That(_value, Is.EqualTo(3));
        }
    }
}
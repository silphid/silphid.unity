using System;
using NUnit.Framework;
using Silphid.Tests;

namespace UniRx.Completables.Tests
{
    public class TimerTest
    {
        private TestScheduler _scheduler;
        private ICompletable _completable;
        private StubCompletableObserver _observer;

        [SetUp]
        public void SetUp()
        {
            _scheduler = new TestScheduler();
            _observer = new StubCompletableObserver();
        }
        
        [Test]
        public void CompletesAfterTimeSpan()
        {
            _completable = Completable.Timer(TimeSpan.FromTicks(10), _scheduler);

            AssertCompletesAfterTenTicks();
        }

        [Test]
        public void CompletesAfterDateTimeOffset()
        {
            var dueTime = _scheduler.Now + TimeSpan.FromTicks(10);
            _completable = Completable.Timer(dueTime, _scheduler);

            AssertCompletesAfterTenTicks();
        }

        private void AssertCompletesAfterTenTicks()
        {
            _completable.Subscribe(_observer);

            _scheduler.AdvanceBy(9);
            _observer.IsCompleted.IsFalse();
            _observer.Error.IsNull();

            _scheduler.AdvanceBy(1);
            _observer.IsCompleted.IsTrue();
            _observer.Error.IsNull();
        }

        [Test]
        public void ThenTimer_IntegrationInASequence()
        {
            var subject1 = new CompletableSubject();

            _completable = subject1.ThenTimer(TimeSpan.FromTicks(10), _scheduler);
            _scheduler.AdvanceBy(1000);

            subject1.HasObservers.IsFalse();

            _completable.Subscribe(_observer);
            _scheduler.AdvanceBy(1000);
            subject1.HasObservers.IsTrue();
            _observer.IsCompleted.IsFalse();
            
            // Complete subject and elapse 9 ticks of timer (should start the timer but not complete it)
            subject1.OnCompleted();
            _scheduler.AdvanceBy(9);
            _observer.IsCompleted.IsFalse();

            // Advancing by one more tick should complete the timer and the observer
            _scheduler.AdvanceBy(1);
            _observer.IsCompleted.IsTrue();
        }
    }
}
using System;
using NUnit.Framework;
using Silphid.Tests;

namespace UniRx.Completables.Tests
{
    public class TimeoutTest
    {
        private CompletableSubject _subject;
        private TestScheduler _scheduler;
        private ICompletable _completable;
        private StubCompletableObserver _observer;

        [SetUp]
        public void SetUp()
        {
            _subject = new CompletableSubject();
            _scheduler = new TestScheduler();
            _observer = new StubCompletableObserver();
        }
        
        [Test]
        public void TimesOutAfterTimeSpan()
        {
            _completable = _subject.Timeout(TimeSpan.FromTicks(10), _scheduler);

            AssertTimesOutAfterTenTicks();
        }

        [Test]
        public void TimesOutAfterDateTimeOffset()
        {
            var dueTime = _scheduler.Now + TimeSpan.FromTicks(10);
            _completable = _subject.Timeout(dueTime, _scheduler);

            AssertTimesOutAfterTenTicks();
        }
        
        [Test]
        public void DoesNotTimeOutIfCompletesBeforeTimeSpan()
        {
            _completable = _subject.Timeout(TimeSpan.FromTicks(10), _scheduler);

            AssertDoesNotTimeOutIfCompletesBeforeTenTicks();
        }

        [Test]
        public void DoesNotTimeOutIfCompletesBeforeDateTimeOffset()
        {
            var dueTime = _scheduler.Now + TimeSpan.FromTicks(10);
            _completable = _subject.Timeout(dueTime, _scheduler);

            AssertDoesNotTimeOutIfCompletesBeforeTenTicks();
        }

        private void AssertTimesOutAfterTenTicks()
        {
            _completable.Subscribe(_observer);

            _scheduler.AdvanceBy(9);
            _observer.IsCompleted.IsFalse();
            _observer.Error.IsNull();

            _scheduler.AdvanceBy(1);
            _observer.IsCompleted.IsFalse();
            _observer.Error.IsInstanceOf<TimeoutException>();

            // Letting more time elapse should not affect anything
            _scheduler.AdvanceBy(1000);
            _observer.IsCompleted.IsFalse();
            _observer.Error.IsInstanceOf<TimeoutException>();
        }

        private void AssertDoesNotTimeOutIfCompletesBeforeTenTicks()
        {
            _completable.Subscribe(_observer);

            _scheduler.AdvanceBy(9);
            _observer.IsCompleted.IsFalse();
            _observer.Error.IsNull();

            _subject.OnCompleted();
            _observer.IsCompleted.IsTrue();
            _observer.Error.IsNull();

            // Letting more time elapse should not affect anything
            _scheduler.AdvanceBy(1000);
            _observer.IsCompleted.IsTrue();
            _observer.Error.IsNull();
        }
    }
}
using NUnit.Framework;
using Silphid.Tests;

namespace UniRx.Completables.Tests
{
    [TestFixture]
    public class RepeatTest
    {
        [Test]
        public void Repeat3_Requires3OnCompletedFromSourceBeforeActuallyCompleting()
        {
            int subscribeCount = 0;
            bool onCompletedCalled = false;
            
            CompletableSubject subject = null;
            Completable
                .Create(observer =>
                {
                    subscribeCount++;
                    subject = new CompletableSubject();
                    return subject.Subscribe(observer);
                })
                .Repeat(3)
                .Subscribe(() => onCompletedCalled = true);
            
            subscribeCount.Is(1);
            onCompletedCalled.IsFalse();

            subject.OnCompleted();
            subscribeCount.Is(2);
            onCompletedCalled.IsFalse();

            subject.OnCompleted();
            subscribeCount.Is(3);
            onCompletedCalled.IsFalse();

            subject.OnCompleted();
            subscribeCount.Is(3);
            onCompletedCalled.IsTrue();
        }

        [Test]
        public void RepeatInfinite_RepeatsUntilSubscriptionDisposed()
        {
            int subscribeCount = 0;
            bool finallyCalled = false;
            
            CompletableSubject subject = null;
            var subscription = Completable
                .Create(observer =>
                {
                    subscribeCount++;
                    subject = new CompletableSubject();
                    return subject.Subscribe(observer);
                })
                .Repeat()
                .Finally(() => finallyCalled = true)
                .Subscribe();
            
            for (int i = 0; i < 100; i++)
            {
                subscribeCount.Is(i + 1);
                subject.OnCompleted();
            }
            finallyCalled.IsFalse();
            
            subscription.Dispose();
            finallyCalled.IsTrue();
        }
    }
}
using System;
using NUnit.Framework;
using Silphid.Tests;

namespace UniRx.Completables.Tests
{
    [TestFixture]
    public class FinallyTest
    {
        [Test]
        public void OnComplete_CallsFinallyAndOnCompleted()
        {
            bool finallyCalled = false;
            bool onCompletedCalled = false;
            
            var subject = new CompletableSubject();
            subject
                .Finally(() => finallyCalled = true)
                .Subscribe(() => onCompletedCalled = true);

            subject.OnCompleted();
            onCompletedCalled.IsTrue();
            finallyCalled.IsTrue();
        }
        
        [Test]
        public void OnError_CallsFinallyAndOnError()
        {
            bool finallyCalled = false;
            Exception receivedException = null;
            var emittedException = new Exception();
            
            var subject = new CompletableSubject();
            subject
                .Finally(() => finallyCalled = true)
                .Subscribe(ex => receivedException = ex);

            subject.OnError(emittedException);
            receivedException.IsSameReferenceAs(emittedException);
            finallyCalled.IsTrue();
        }
        
        [Test]
        public void ExceptionDuringSubscribe_CallsFinallyAndRethrows()
        {
            bool finallyCalled = false;
            Exception catchedException = null;
            var thrownException = new Exception();
            
            try
            {
                Completable
                    .Create(observer => throw thrownException)
                    .Finally(() => finallyCalled = true)
                    .Subscribe();
            }
            catch (Exception ex)
            {
                catchedException = ex;
            }
            
            catchedException.IsSameReferenceAs(thrownException);
            finallyCalled.IsTrue();
        }
    }
}
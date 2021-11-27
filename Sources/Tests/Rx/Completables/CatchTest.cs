using System;
using NUnit.Framework;
using Silphid.Tests;

namespace UniRx.Completables.Tests
{
    [TestFixture]
    public class CatchTest
    {
        [Test]
        public void CatchWithMoreSpecificException_ShouldNotCatchButCallOnError()
        {
            Exception receivedException = null;
            var emittedException = new Exception();
            
            var subject = new CompletableSubject();
            subject
                .Catch<InvalidOperationException>(ex =>
                {
                    Assert.Fail("Should not be called");
                    return Completable.Empty();
                })
                .Subscribe(ex => receivedException = ex);
            
            subject.OnError(emittedException);
            receivedException.IsSameReferenceAs(emittedException);
        }

        [Test]
        public void CatchWithMoreGenericException_ShouldCatchButNotCallOnError()
        {
            Exception receivedException = null;
            var emittedException = new InvalidOperationException();
            
            var subject = new CompletableSubject();
            subject
                .Catch<Exception>(ex =>
                {
                    receivedException = ex;
                    return Completable.Empty();
                })
                .Subscribe(ex => Assert.Fail("Should not be called"));
            
            subject.OnError(emittedException);
            receivedException.IsSameReferenceAs(emittedException);
        }

        [Test]
        public void CatchIgnoreWithHandler_ShouldCatchButNotCallOnError()
        {
            Exception receivedException = null;
            var emittedException = new InvalidOperationException();
            
            var subject = new CompletableSubject();
            subject
                .CatchIgnore<Exception>(ex =>
                {
                    receivedException = ex;
                })
                .Subscribe(ex => Assert.Fail("Should not be called"));
            
            subject.OnError(emittedException);
            receivedException.IsSameReferenceAs(emittedException);
        }

        [Test]
        public void CatchIgnoreWithoutHandler_ShouldNotCallOnError()
        {
            bool onCompletedCalled = false;
            
            var subject = new CompletableSubject();
            subject
                .CatchIgnore<Exception>()
                .Subscribe(ex => Assert.Fail("Should not be called"), () => onCompletedCalled = true);
            
            subject.OnError(new Exception());
            onCompletedCalled.IsTrue();
        }
    }
}
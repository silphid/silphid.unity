using System;
using NSubstitute;
using NUnit.Framework;
using Silphid.Loadzup.Caching;
using UniRx;

namespace Silphid.Loadzup.Test.Caching
{
    [TestFixture]
    public class CachedLoaderTest
    {
        private ILoader _innerLoader;
        private MemoryCacheLoader _fixture;

        private static readonly Uri MockURI = new Uri("http://test.json");
        private static readonly string ExpectedString = "Object to return";

        [SetUp]
        public void SetUp()
        {
            _innerLoader = Substitute.For<ILoader>();
            _fixture = new MemoryCacheLoader(_innerLoader);
        }

        private void SetupReturn<T>(IObservable<T> returnObservable, Uri uri = null)
        {
            _innerLoader.Load<T>(uri ?? Arg.Any<Uri>(), Arg.Any<Options>())
                .Returns(returnObservable);
        }

        private string LoadString()
        {
            return _fixture.Load<string>(MockURI).Wait();
        }

        [Test]
        public void LoadOnce_ReturnLoadedObject()
        {
            SetupReturn(Observable.Return(ExpectedString));

            var loadedString = LoadString();

            Assert.AreEqual(loadedString, ExpectedString);
        }

        [Test]
        public void LoadTwiceTheSameURI_ReturnCachedObject()
        {
            SetupReturn(Observable.Return(ExpectedString));

            var item1 = LoadString();
            var item2 = LoadString();

            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());

            Assert.AreSame(item1, item2);
        }

        [Test]
        public void BurstLoadWithoutCached_SecondRequestIsReturnFromBurst()
        {
            var sub = new Subject<string>();
            SetupReturn(sub);

            // First request
            string actualString1 = null;
            _fixture.Load<string>(MockURI).Subscribe(x => actualString1 = x);

            Assert.That(actualString1, Is.Null);
            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());

            // Second request
            string actualString2 = null;
            _fixture.Load<string>(MockURI).Subscribe(x => actualString2 = x);

            Assert.That(actualString2, Is.Null);
            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());

            // Make innerloader complete
            sub.OnNext(ExpectedString);
            sub.OnCompleted();

            Assert.That(actualString1, Is.EqualTo(ExpectedString));
            Assert.That(actualString2, Is.EqualTo(ExpectedString));
            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());
        }

        [Test]
        public void BurstLoadWithoutCachedError_SecondRequestIsReturnFromBurstWithError_ReturnInvalidOperation()
        {
            var sub = new Subject<string>();
            SetupReturn(sub);

            // First request
            InvalidOperationException exception1 = null;
            _fixture.Load<string>(MockURI).Subscribe(null, e => exception1 = e as InvalidOperationException);

            Assert.That(exception1, Is.Null);
            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());

            // Second request
            InvalidOperationException exception2 = null;
            _fixture.Load<string>(MockURI).Subscribe(null, e => exception2 = e as InvalidOperationException);

            Assert.That(exception2, Is.Null);
            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());

            // Make innerloader complete
            sub.OnError(new InvalidOperationException());

            Assert.That(exception1, Is.EqualTo(exception2));
            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());
        }

        [Test]
        public void LoadWithDifferentURI_ReturnDifferentObjectAvoidingCache()
        {
            // Create subject for first URI
            var firstExpectedString = ExpectedString;
            var firstURI = MockURI;
            var subFirstURI = new Subject<string>();
            SetupReturn(subFirstURI, firstURI);

            // Create subject for second URI
            var secondExpectedString = ExpectedString + "2";
            var secondURI = new Uri(MockURI.OriginalString + "2");
            var subSecondURI = new Subject<string>();
            SetupReturn(subSecondURI, secondURI);

            // Request with first URI
            string actualString1 = null;
            _fixture.Load<string>(firstURI).Subscribe(x => actualString1 = x);

            Assert.That(actualString1, Is.Null);
            _innerLoader.Received(1).Load<string>(firstURI, Arg.Any<Options>());

            // Make innerloader of request with first URI complete
            subFirstURI.OnNext(firstExpectedString);
            subFirstURI.OnCompleted();

            Assert.That(actualString1, Is.EqualTo(firstExpectedString));
            _innerLoader.Received(1).Load<string>(firstURI, Arg.Any<Options>());

            // Second request
            string actualString2 = null;
            _fixture.Load<string>(secondURI).Subscribe(x => actualString2 = x);

            // Should not return instantly from cache
            Assert.That(actualString2, Is.Null);
            _innerLoader.Received(1).Load<string>(secondURI, Arg.Any<Options>());

            // Make innerload of request with second URI complete
            subSecondURI.OnNext(secondExpectedString);
            subSecondURI.OnCompleted();

            Assert.That(actualString1, Is.EqualTo(firstExpectedString));
            Assert.That(actualString2, Is.EqualTo(secondExpectedString));
            _innerLoader.Received(1).Load<string>(firstURI, Arg.Any<Options>());
            _innerLoader.Received(1).Load<string>(secondURI, Arg.Any<Options>());
            _innerLoader.Received(2).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());
        }

        [Test]
        public void LoadWithDifferentURI_ReturnDifferentObjectAvoidingBurst()
        {
            // Create subject for first URI
            var firstExpectedString = ExpectedString;
            var firstURI = MockURI;
            var subFirstURI = new Subject<string>();
            SetupReturn(subFirstURI, firstURI);

            // Create subject for second URI
            var secondExpectedString = ExpectedString + "2";
            var secondURI = new Uri(MockURI.OriginalString + "2");
            var subSecondURI = new Subject<string>();
            SetupReturn(subSecondURI, secondURI);

            // Request with first URI
            string actualString1 = null;
            _fixture.Load<string>(firstURI).Subscribe(x => actualString1 = x);

            Assert.That(actualString1, Is.Null);
            _innerLoader.Received(1).Load<string>(firstURI, Arg.Any<Options>());

            // Second request
            string actualString2 = null;
            _fixture.Load<string>(secondURI).Subscribe(x => actualString2 = x);

            Assert.That(actualString2, Is.Null);
            _innerLoader.Received(1).Load<string>(secondURI, Arg.Any<Options>());
            _innerLoader.Received(2).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());

            // Make innerloader of request with first URI complete
            subFirstURI.OnNext(firstExpectedString);
            subFirstURI.OnCompleted();

            Assert.That(actualString1, Is.EqualTo(firstExpectedString));
            _innerLoader.Received(1).Load<string>(firstURI, Arg.Any<Options>());

            // Should not return instantly from burst
            Assert.That(actualString2, Is.Null);
            _innerLoader.Received(1).Load<string>(secondURI, Arg.Any<Options>());
            _innerLoader.Received(2).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());

            // Make innerload of request with second URI complete
            subSecondURI.OnNext(secondExpectedString);
            subSecondURI.OnCompleted();

            Assert.That(actualString1, Is.EqualTo(firstExpectedString));
            Assert.That(actualString2, Is.EqualTo(secondExpectedString));
            _innerLoader.Received(1).Load<string>(firstURI, Arg.Any<Options>());
            _innerLoader.Received(1).Load<string>(secondURI, Arg.Any<Options>());
            _innerLoader.Received(2).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());
        }

        [Test]
        public void CompleteFirstRequestBeforeSecondRequest_SecondRequestIsReturnedFromCache()
        {
            var sub = new Subject<string>();
            SetupReturn(sub);

            // First request
            string actualString1 = null;
            _fixture.Load<string>(MockURI).Subscribe(x => actualString1 = x);

            Assert.That(actualString1, Is.Null);
            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());

            // Make innerloader complete
            sub.OnNext(ExpectedString);
            sub.OnCompleted();

            Assert.That(actualString1, Is.EqualTo(ExpectedString));
            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());

            // Second request
            string actualString2 = null;
            _fixture.Load<string>(MockURI).Subscribe(x => actualString2 = x);

            // Should return instantly from cache
            Assert.That(actualString2, Is.EqualTo(ExpectedString));
            _innerLoader.Received(1).Load<string>(Arg.Any<Uri>(), Arg.Any<Options>());
        }
    }
}
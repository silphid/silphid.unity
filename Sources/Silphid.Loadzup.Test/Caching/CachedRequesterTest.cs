using System;
using System.Collections.Generic;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using Silphid.Loadzup.Http;
using Silphid.Loadzup.Http.Caching;
using UniRx;

namespace Silphid.Loadzup.Test.Caching
{
	public class CachedRequesterTest
	{
		private IHttpRequester _innerRequester;
		private IHttpCache _httpCache;
		private HttpCacheRequester _fixture;

		private static readonly Uri AvailableUri = new Uri("http://test.com/data.json");
		private static readonly Uri NotFoundUri = new Uri("http://test.com/not_found");
		private static readonly Uri NotModifiedUri = new Uri("http://test.com/not_modified");
		private static readonly byte[] Bytes1 = {0x12, 0x34};
		private static readonly byte[] Bytes2 = {0x56, 0x78};
		private static readonly ContentType TestContentType = new ContentType("application/json");
		private const string TestETag1 = "0123456789";

		private static IDictionary<string, string> AnyNonNullDictionary =>
			Arg.Is<IDictionary<string, string>>(x => x != null);

		[SetUp]
		public void SetUp()
		{
			_innerRequester = Substitute.For<IHttpRequester>();
			_innerRequester.Request(NotFoundUri, Arg.Any<Options>())
				.Returns(Observable.Throw<Response>(new HttpException(new Uri(""), HttpStatusCode.NotFound)));
			_innerRequester.Request(NotModifiedUri, Arg.Any<Options>())
				.Returns(Observable.Throw<Response>(new HttpException(new Uri(""), HttpStatusCode.NotModified)));

			_httpCache = Substitute.For<IHttpCache>();
			_httpCache.LoadHeaders(Arg.Any<Uri>()).Returns((Dictionary<string, string>) null);

			_fixture = new HttpCacheRequester(_innerRequester, _httpCache);
		}

		private void SetupRequest(Uri uri, byte[] bytes, ContentType contentType, string eTag)
		{
			_innerRequester.Request(uri, Arg.Any<Options>())
				.Returns(Observable.Return(
					new Response(200, bytes,
						new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
						{
							[KnownHttpHeaders.ContentType] = contentType.ToString(),
							[KnownHttpHeaders.ETag] = eTag
						})));
		}

		private void SetupCacheStorage(Uri uri, byte[] bytes, ContentType contentType, string eTag)
		{
			_httpCache
				.LoadHeaders(uri)
				.Returns(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					[KnownHttpHeaders.ContentType] = contentType.ToString(),
					[KnownHttpHeaders.ETag] = eTag
				});

			_httpCache
				.Load(uri)
				.Returns(Observable.Return(bytes));
		}

		private void AssertResponse(Response response, byte[] bytes, ContentType contentType)
		{
			Assert.That(response.Bytes, Is.EqualTo(bytes));
			Assert.That(response.ContentType.MediaType, Is.EqualTo(contentType.MediaType));
		}

		private IObservable<Response> Request(Uri uri, HttpCachePolicy policy) =>
			_fixture.Request(uri, new Options {HttpCachePolicy = policy});

		[Test]
		public void OriginOnly_OriginAvailable_ReturnsOrigin()
		{
			SetupRequest(AvailableUri, Bytes1, TestContentType, TestETag1);

			var response = Request(AvailableUri, HttpCachePolicy.OriginOnly).Wait();

			AssertResponse(response, Bytes1, TestContentType);
			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
			_httpCache.DidNotReceive().Load(Arg.Any<Uri>());
		}

		[Test]
		public void OriginOnly_OriginNotAvailable_ThrowsException()
		{
			Assert.Throws<HttpException>(() =>
				Request(NotFoundUri, HttpCachePolicy.OriginOnly).Wait());

			_innerRequester.Received(1).Request(NotFoundUri, Arg.Any<Options>());
			_httpCache.DidNotReceive().LoadHeaders(Arg.Any<Uri>());
			_httpCache.DidNotReceive().Load(Arg.Any<Uri>());
			_httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
		}

		[Test]
		public void CacheOnly_CacheAvailable_ReturnsCache()
		{
			SetupCacheStorage(AvailableUri, Bytes1, TestContentType, TestETag1);

			var response = Request(AvailableUri, HttpCachePolicy.CacheOnly).Wait();

			AssertResponse(response, Bytes1, TestContentType);
			_innerRequester.DidNotReceive().Request(Arg.Any<Uri>(), Arg.Any<Options>());
			_httpCache.Received(1).LoadHeaders(AvailableUri);
			_httpCache.Received(1).Load(AvailableUri);
			_httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
		}

		[Test]
		public void CacheOnly_CacheNotAvailable_ThrowsException()
		{
			Assert.Throws<InvalidOperationException>(() =>
				Request(AvailableUri, HttpCachePolicy.CacheOnly).Wait());
		}

		[Test]
		public void CacheOtherwiseOrigin_CacheNotAvailable_ReturnsOrigin()
		{
			SetupRequest(AvailableUri, Bytes1, TestContentType, TestETag1);

			var response = Request(AvailableUri, HttpCachePolicy.CacheOtherwiseOrigin).Wait();

			AssertResponse(response, Bytes1, TestContentType);
			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
			_httpCache.Received(1).LoadHeaders(AvailableUri);
			_httpCache.DidNotReceive().Load(Arg.Any<Uri>());
			_httpCache.Received(1).Save(AvailableUri, Bytes1, AnyNonNullDictionary);
		}

		[Test]
		public void CacheOtherwiseOrigin_CacheAvailable_ReturnsCache()
		{
			SetupCacheStorage(AvailableUri, Bytes1, TestContentType, TestETag1);

			var response = Request(AvailableUri, HttpCachePolicy.CacheOtherwiseOrigin).Wait();

			AssertResponse(response, Bytes1, TestContentType);
			_innerRequester.DidNotReceive().Request(Arg.Any<Uri>(), Arg.Any<Options>());
			_httpCache.Received(1).LoadHeaders(AvailableUri);
			_httpCache.Received(1).Load(AvailableUri);
		}

		[Test]
		public void OriginOtherwiseCache_OriginNotAvailable_ReturnsCache()
		{
			SetupCacheStorage(NotFoundUri, Bytes1, TestContentType, TestETag1);

			var response = Request(NotFoundUri, HttpCachePolicy.OriginOtherwiseCache).Wait();

			AssertResponse(response, Bytes1, TestContentType);
			_innerRequester.Received(1).Request(NotFoundUri, Arg.Any<Options>());
			_httpCache.Received(1).LoadHeaders(NotFoundUri);
			_httpCache.Received(1).Load(NotFoundUri);
			_httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
		}

		[Test]
		public void OriginOtherwiseCache_OriginAvailable_ReturnsOrigin()
		{
			SetupRequest(AvailableUri, Bytes1, TestContentType, TestETag1);

			var response = Request(AvailableUri, HttpCachePolicy.OriginOtherwiseCache).Wait();

			AssertResponse(response, Bytes1, TestContentType);
			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
			_httpCache.Received(1).LoadHeaders(AvailableUri);
			_httpCache.DidNotReceive().Load(Arg.Any<Uri>());
			_httpCache.Received(1).Save(AvailableUri, Bytes1, AnyNonNullDictionary);
		}

		[Test]
		public void CacheThenOrigin_BothCacheAndOriginAvailable_ReturnsCacheThenOrigin()
		{
			SetupRequest(AvailableUri, Bytes1, TestContentType, TestETag1);
			SetupCacheStorage(AvailableUri, Bytes2, TestContentType, TestETag1);

			var responses = Request(AvailableUri, HttpCachePolicy.CacheThenOrigin).ToList().Wait();

			Assert.That(responses.Count, Is.EqualTo(2));
			AssertResponse(responses[0], Bytes2, TestContentType);
			AssertResponse(responses[1], Bytes1, TestContentType);
			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
			_httpCache.Received(1).LoadHeaders(AvailableUri);
			_httpCache.Received(1).Load(AvailableUri);
			_httpCache.Received(1).Save(AvailableUri, Bytes1, AnyNonNullDictionary);
		}

		[Test]
		public void CacheThenOrigin_CacheAvailable_OriginNotFound_ReturnsCacheThenError()
		{
			SetupCacheStorage(NotFoundUri, Bytes1, TestContentType, TestETag1);

			var responses = new List<Response>();
			Assert.Throws<HttpException>(() =>
				Request(NotFoundUri, HttpCachePolicy.CacheThenOrigin)
					.Do(x => responses.Add(x)).Wait());

			Assert.That(responses.Count, Is.EqualTo(1));
			AssertResponse(responses[0], Bytes1, TestContentType);
			_innerRequester.Received(1).Request(NotFoundUri, Arg.Any<Options>());
			_httpCache.Received(1).LoadHeaders(NotFoundUri);
			_httpCache.Received(1).Load(NotFoundUri);
			_httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
		}

		[Test]
		public void CacheThenOrigin_CacheAvailable_OriginNotModified_ReturnsCacheThenNothingMore()
		{
			SetupCacheStorage(NotModifiedUri, Bytes1, TestContentType, TestETag1);

			var responses = new List<Response>();
			Request(NotModifiedUri, HttpCachePolicy.CacheThenOrigin)
				.Do(x => responses.Add(x)).Wait();

			Assert.That(responses.Count, Is.EqualTo(1));
			AssertResponse(responses[0], Bytes1, TestContentType);
			_innerRequester.Received(1).Request(NotModifiedUri, Arg.Any<Options>());
			_httpCache.Received(1).LoadHeaders(NotModifiedUri);
			_httpCache.Received(1).Load(NotModifiedUri);
			_httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
		}

		[Test]
		public void CacheThenOrigin_NeitherCacheNorOriginAvailable_ThrowsException()
		{
			Assert.Throws<HttpException>(() =>
				Request(NotModifiedUri, HttpCachePolicy.CacheThenOrigin).Wait());
		}

		[Test]
		public void CacheThenOriginIfETag_CacheAvailable_OriginAvailableAndModified_ReturnsCacheThenOrigin()
		{
			SetupRequest(AvailableUri, Bytes1, TestContentType, TestETag1);
			SetupCacheStorage(AvailableUri, Bytes2, TestContentType, TestETag1);

			var responses = Request(AvailableUri, HttpCachePolicy.CacheThenOriginIfETag).ToList().Wait();

			Assert.That(responses.Count, Is.EqualTo(2));
			AssertResponse(responses[0], Bytes2, TestContentType);
			AssertResponse(responses[1], Bytes1, TestContentType);
			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
			_httpCache.Received(1).LoadHeaders(AvailableUri);
			_httpCache.Received(1).Load(AvailableUri);
			_httpCache.Received(1).Save(AvailableUri, Bytes1, AnyNonNullDictionary);
		}

//    [Test]
//    public void CacheThenOriginIfETag_CacheAvailable_OriginNotFound_ReturnsCacheThenError()
//    {
//        SetupCacheStorage(NotFoundUri, Bytes1, TestContentType, TestETag1);
//
//        var responses = new List<Response>();
//        Assert.Throws<HttpException>(() =>
//            Request(NotFoundUri, HttpCachePolicy.CacheThenOriginIfETag)
//                .Do(x => responses.Add(x)).Wait());
//
//        Assert.That(responses.Count, Is.EqualTo(1));
//        AssertResponse(responses[0], Bytes1, TestContentType);
//        _innerRequester.Received(1).Request(NotFoundUri, Arg.Any<Options>());
//        _httpCache.Received(1).LoadHeaders(NotFoundUri);
//        _httpCache.Received(1).Load(NotFoundUri);
//        _httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
//    }
//
//    [Test]
//    public void CacheThenOriginIfETag_CacheAvailable_OriginNotModified_ReturnsCacheThenNothingMore()
//    {
//        SetupCacheStorage(NotModifiedUri, Bytes1, TestContentType, TestETag1);
//
//        var responses = new List<Response>();
//        Request(NotModifiedUri, HttpCachePolicy.CacheThenOriginIfETag)
//            .Do(x => responses.Add(x)).Wait();
//
//        Assert.That(responses.Count, Is.EqualTo(1));
//        AssertResponse(responses[0], Bytes1, TestContentType);
//        _innerRequester.Received(1).Request(NotModifiedUri, Arg.Any<Options>());
//        _httpCache.Received(1).LoadHeaders(NotModifiedUri);
//        _httpCache.Received(1).Load(NotModifiedUri);
//        _httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
//    }

		[Test]
		public void CacheThenOriginIfETag_NeitherCacheNorOriginAvailable_ThrowsException()
		{
			Assert.Throws<HttpException>(() =>
				Request(NotModifiedUri, HttpCachePolicy.CacheThenOriginIfETag).Wait());
		}
	}
}
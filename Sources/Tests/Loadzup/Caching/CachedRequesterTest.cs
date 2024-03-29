﻿//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http.Headers;
//using NSubstitute;
//using NUnit.Framework;
//using Silphid.Loadzup.Http;
//using Silphid.Loadzup.Http.Caching;
//using UniRx;
//
//namespace Silphid.Loadzup.Test.Caching
//{
//	public class CachedRequesterTest
//	{
//		private IHttpRequester _innerRequester;
//		private IHttpCache _httpCache;
//		private HttpCacheRequester _fixture;
//
//		private static readonly Uri AvailableUri = new Uri("http://test.com/data.json");
//		private static readonly Uri NotFoundUri = new Uri("http://test.com/not_found");
//		private static readonly Uri NotModifiedUri = new Uri("http://test.com/not_modified");
//		private static readonly byte[] Bytes1 = {0x12, 0x34};
//		private static readonly byte[] Bytes2 = {0x56, 0x78};
//		private static readonly MediaTypeHeaderValue TestMediaTypeHeaderValue = new MediaTypeHeaderValue("application/json");
//		private const string TestETag1 = "0123456789";
//
//		private static Headers AnyNonHeadersObject =>
//			Arg.Is<Headers>(x => x != null);
//
//		[SetUp]
//		public void SetUp()
//		{
//			_innerRequester = Substitute.For<IHttpRequester>();
//			_innerRequester.Request(NotFoundUri, Arg.Any<Options>())
//				.Returns(Observable.Throw<Response>(new HttpException(new Uri(""), HttpStatusCode.NotFound)));
//			_innerRequester.Request(NotModifiedUri, Arg.Any<Options>())
//				.Returns(Observable.Throw<Response>(new HttpException(new Uri(""), HttpStatusCode.NotModified)));
//
//			_httpCache = Substitute.For<IHttpCache>();
//			_httpCache.LoadHeaders(Arg.Any<Uri>()).Returns((Headers) null);
//
//			_fixture = new HttpCacheRequester(_innerRequester, _httpCache);
//		}
//
//		private void SetupRequest(Uri uri, byte[] bytes, MediaTypeHeaderValue mediaType, string eTag)
//		{
//			_innerRequester.Request(uri, Arg.Any<Options>())
//				.Returns(Observable.Return(
//					new Response(200, () => bytes,
//						new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
//						{
//							[KnownHeaders.ContentType] = mediaType.ToString(),
//							[KnownHeaders.ETag] = eTag
//						})));
//		}
//
//		private void SetupCacheStorage(Uri uri, byte[] bytes, MediaTypeHeaderValue mediaType, string eTag)
//		{
//			_httpCache
//				.LoadHeaders(uri)
//				.Returns(new Headers()
//				{
//					[KnownHeaders.ContentType] = mediaType.ToString(),
//					[KnownHeaders.ETag] = eTag
//				});
//
//			_httpCache
//				.Load(uri)
//				.Returns(Observable.Return(bytes));
//		}
//
//		private void AssertResponse(Response response, byte[] bytes, MediaTypeHeaderValue mediaType)
//		{
//			Assert.That(response.Bytes, Is.EqualTo(bytes));
//			Assert.That(response.Headers.ContentType.MediaType, Is.EqualTo(mediaType.MediaType));
//		}
//
//		private IObservable<Response> Request(Uri uri, CachePolicy policy) =>
//			_fixture.Request(uri, Options.Empty.With(policy));
//
//		[Test]
//		public void OriginOnly_OriginAvailable_ReturnsOrigin()
//		{
//			SetupRequest(AvailableUri, Bytes1, TestMediaTypeHeaderValue, TestETag1);
//
//			var response = Request(AvailableUri, CachePolicy.OriginOnly).Wait();
//
//			AssertResponse(response, Bytes1, TestMediaTypeHeaderValue);
//			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
//			_httpCache.DidNotReceive().Load(Arg.Any<Uri>());
//		}
//
//		[Test]
//		public void OriginOnly_OriginNotAvailable_ThrowsException()
//		{
//			Assert.Throws<HttpException>(() =>
//				Request(NotFoundUri, CachePolicy.OriginOnly).Wait());
//
//			_innerRequester.Received(1).Request(NotFoundUri, Arg.Any<Options>());
//			_httpCache.DidNotReceive().LoadHeaders(Arg.Any<Uri>());
//			_httpCache.DidNotReceive().Load(Arg.Any<Uri>());
//			_httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
//		}
//
//		[Test]
//		public void CacheOtherwiseOrigin_CacheNotAvailable_ReturnsOrigin()
//		{
//			SetupRequest(AvailableUri, Bytes1, TestMediaTypeHeaderValue, TestETag1);
//
//			var response = Request(AvailableUri, CachePolicy.CacheOtherwiseOrigin).Wait();
//
//			AssertResponse(response, Bytes1, TestMediaTypeHeaderValue);
//			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
//			_httpCache.Received(1).LoadHeaders(AvailableUri);
//			_httpCache.DidNotReceive().Load(Arg.Any<Uri>());
//			_httpCache.Received(1).Save(AvailableUri, Bytes1, AnyNonHeadersObject);
//		}
//
//		[Test]
//		public void CacheOtherwiseOrigin_CacheAvailable_ReturnsCache()
//		{
//			SetupCacheStorage(AvailableUri, Bytes1, TestMediaTypeHeaderValue, TestETag1);
//
//			var response = Request(AvailableUri, CachePolicy.CacheOtherwiseOrigin).Wait();
//
//			AssertResponse(response, Bytes1, TestMediaTypeHeaderValue);
//			_innerRequester.DidNotReceive().Request(Arg.Any<Uri>(), Arg.Any<Options>());
//			_httpCache.Received(1).LoadHeaders(AvailableUri);
//			_httpCache.Received(1).Load(AvailableUri);
//		}
//
//		[Test]
//		public void OriginOtherwiseCache_OriginNotAvailable_ReturnsCache()
//		{
//			SetupCacheStorage(NotFoundUri, Bytes1, TestMediaTypeHeaderValue, TestETag1);
//
//			var response = Request(NotFoundUri, CachePolicy.OriginOtherwiseCache).Wait();
//
//			AssertResponse(response, Bytes1, TestMediaTypeHeaderValue);
//			_innerRequester.Received(1).Request(NotFoundUri, Arg.Any<Options>());
//			_httpCache.Received(1).LoadHeaders(NotFoundUri);
//			_httpCache.Received(1).Load(NotFoundUri);
//			_httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
//		}
//
//		[Test]
//		public void OriginOtherwiseCache_OriginAvailable_ReturnsOrigin()
//		{
//			SetupRequest(AvailableUri, Bytes1, TestMediaTypeHeaderValue, TestETag1);
//
//			var response = Request(AvailableUri, CachePolicy.OriginOtherwiseCache).Wait();
//
//			AssertResponse(response, Bytes1, TestMediaTypeHeaderValue);
//			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
//			_httpCache.Received(1).LoadHeaders(AvailableUri);
//			_httpCache.DidNotReceive().Load(Arg.Any<Uri>());
//			_httpCache.Received(1).Save(AvailableUri, Bytes1, AnyNonHeadersObject);
//		}
//
//		[Test]
//		public void CacheThenOrigin_BothCacheAndOriginAvailable_ReturnsCacheThenOrigin()
//		{
//			SetupRequest(AvailableUri, Bytes1, TestMediaTypeHeaderValue, TestETag1);
//			SetupCacheStorage(AvailableUri, Bytes2, TestMediaTypeHeaderValue, TestETag1);
//
//			var responses = Request(AvailableUri, CachePolicy.CacheThenOrigin).ToList().Wait();
//
//			Assert.That(responses.Count, Is.EqualTo(2));
//			AssertResponse(responses[0], Bytes2, TestMediaTypeHeaderValue);
//			AssertResponse(responses[1], Bytes1, TestMediaTypeHeaderValue);
//			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
//			_httpCache.Received(1).LoadHeaders(AvailableUri);
//			_httpCache.Received(1).Load(AvailableUri);
//			_httpCache.Received(1).Save(AvailableUri, Bytes1, AnyNonHeadersObject);
//		}
//
//		[Test]
//		public void CacheThenOrigin_CacheAvailable_OriginNotFound_ReturnsCacheThenError()
//		{
//			SetupCacheStorage(NotFoundUri, Bytes1, TestMediaTypeHeaderValue, TestETag1);
//
//			var responses = new List<Response>();
//			Assert.Throws<HttpException>(() =>
//				Request(NotFoundUri, CachePolicy.CacheThenOrigin)
//					.Do(x => responses.Add(x)).Wait());
//
//			Assert.That(responses.Count, Is.EqualTo(1));
//			AssertResponse(responses[0], Bytes1, TestMediaTypeHeaderValue);
//			_innerRequester.Received(1).Request(NotFoundUri, Arg.Any<Options>());
//			_httpCache.Received(1).LoadHeaders(NotFoundUri);
//			_httpCache.Received(1).Load(NotFoundUri);
//			_httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
//		}
//
//		[Test]
//		public void CacheThenOrigin_CacheAvailable_OriginNotModified_ReturnsCacheThenNothingMore()
//		{
//			SetupCacheStorage(NotModifiedUri, Bytes1, TestMediaTypeHeaderValue, TestETag1);
//
//			var responses = new List<Response>();
//			Request(NotModifiedUri, CachePolicy.CacheThenOrigin)
//				.Do(x => responses.Add(x)).Wait();
//
//			Assert.That(responses.Count, Is.EqualTo(1));
//			AssertResponse(responses[0], Bytes1, TestMediaTypeHeaderValue);
//			_innerRequester.Received(1).Request(NotModifiedUri, Arg.Any<Options>());
//			_httpCache.Received(1).LoadHeaders(NotModifiedUri);
//			_httpCache.Received(1).Load(NotModifiedUri);
//			_httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
//		}
//
//		[Test]
//		public void CacheThenOrigin_NeitherCacheNorOriginAvailable_ThrowsException()
//		{
//			Assert.Throws<HttpException>(() =>
//				Request(NotModifiedUri, CachePolicy.CacheThenOrigin).Wait());
//		}
//
//		[Test]
//		public void CacheThenOriginIfETag_CacheAvailable_OriginAvailableAndModified_ReturnsCacheThenOrigin()
//		{
//			SetupRequest(AvailableUri, Bytes1, TestMediaTypeHeaderValue, TestETag1);
//			SetupCacheStorage(AvailableUri, Bytes2, TestMediaTypeHeaderValue, TestETag1);
//
//			var responses = Request(AvailableUri, CachePolicy.CacheThenOriginIfETag).ToList().Wait();
//
//			Assert.That(responses.Count, Is.EqualTo(2));
//			AssertResponse(responses[0], Bytes2, TestMediaTypeHeaderValue);
//			AssertResponse(responses[1], Bytes1, TestMediaTypeHeaderValue);
//			_innerRequester.Received(1).Request(AvailableUri, Arg.Any<Options>());
//			_httpCache.Received(1).LoadHeaders(AvailableUri);
//			_httpCache.Received(1).Load(AvailableUri);
//			_httpCache.Received(1).Save(AvailableUri, Bytes1, AnyNonHeadersObject);
//		}
//
////    [Test]
////    public void CacheThenOriginIfETag_CacheAvailable_OriginNotFound_ReturnsCacheThenError()
////    {
////        SetupCacheStorage(NotFoundUri, Bytes1, TestContentType, TestETag1);
////
////        var responses = new List<Response>();
////        Assert.Throws<HttpException>(() =>
////            Request(NotFoundUri, HttpCachePolicy.CacheThenOriginIfETag)
////                .Do(x => responses.Add(x)).Wait());
////
////        Assert.That(responses.Count, Is.EqualTo(1));
////        AssertResponse(responses[0], Bytes1, TestContentType);
////        _innerRequester.Received(1).Request(NotFoundUri, Arg.Any<Options>());
////        _httpCache.Received(1).LoadHeaders(NotFoundUri);
////        _httpCache.Received(1).Load(NotFoundUri);
////        _httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
////    }
////
////    [Test]
////    public void CacheThenOriginIfETag_CacheAvailable_OriginNotModified_ReturnsCacheThenNothingMore()
////    {
////        SetupCacheStorage(NotModifiedUri, Bytes1, TestContentType, TestETag1);
////
////        var responses = new List<Response>();
////        Request(NotModifiedUri, HttpCachePolicy.CacheThenOriginIfETag)
////            .Do(x => responses.Add(x)).Wait();
////
////        Assert.That(responses.Count, Is.EqualTo(1));
////        AssertResponse(responses[0], Bytes1, TestContentType);
////        _innerRequester.Received(1).Request(NotModifiedUri, Arg.Any<Options>());
////        _httpCache.Received(1).LoadHeaders(NotModifiedUri);
////        _httpCache.Received(1).Load(NotModifiedUri);
////        _httpCache.DidNotReceiveWithAnyArgs().Save(null, null, null);
////    }
//
//		[Test]
//		public void CacheThenOriginIfETag_NeitherCacheNorOriginAvailable_ThrowsException()
//		{
//			Assert.Throws<HttpException>(() =>
//				Request(NotModifiedUri, CachePolicy.CacheThenOriginIfETag).Wait());
//		}
//	}
//}


using System;
using NUnit.Framework;

namespace Silphid.Extensions.Tests
{
    public class UriExtensionsTests
    {
        [Test]
        public void ParseUriQueryParams_ReturnsParams()
        {
            var uri = new Uri("https://www.google.com?key1=expected1&key2=expected2&keys3");
            var queryParams = uri.ParseQueryParams();

            Assert.AreEqual("expected1", queryParams["key1"]);
            Assert.AreEqual("expected2", queryParams["key2"]);
            Assert.AreEqual(null, queryParams["keys3"]);
        }

        [Test]
        public void ParseUriQueryParamsWithoutParams_ReturnsEmptyCollection()
        {
            var uri = new Uri("https://www.google.com");
            var queryParams = uri.ParseQueryParams();

            Assert.AreEqual(0, queryParams.Count);
        }

        [Test]
        public void ParseUriQueryParamsWithDuplicateParams_ReturnsParams()
        {
            var uri = new Uri("https://www.google.com?test=123&test=321");
            var queryParams = uri.ParseQueryParams();

            Assert.AreEqual("123,321", queryParams["test"]);
        }
    }
}
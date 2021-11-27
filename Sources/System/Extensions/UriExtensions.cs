using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;

namespace Silphid.Extensions
{
    public static class UriExtensions
    {
        public static Uri WithQueryParam(this Uri This, string key, string value)
        {
            if (key.IsNullOrWhiteSpace() || value == null)
                return This;

            var query = $"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}";
            var builder = new UriBuilder(This)
            {
                Query = string.IsNullOrEmpty(This.Query)
                            ? query
                            : $"{This.Query.Substring(1)}&{query}"
            };

            return builder.Uri;
        }

        public static Uri WithQueryParams(this Uri This, IDictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return This;
            
            var concatenatedParameters = parameters.Select(
                pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}");

            var query = string.Join("&", concatenatedParameters);
            var builder = new UriBuilder(This)
            {
                Query = string.IsNullOrEmpty(This.Query)
                            ? query
                            : $"{This.Query.Substring(1)}&{query}"
            };

            return builder.Uri;
        }

        public static string GetParentUriString(this Uri This)
        {
            return This.AbsoluteUri.Remove(
                This.AbsoluteUri.Length - This.Segments.Last()
                                            .Length);
        }

        public static NameValueCollection ParseQueryParams(this Uri This)
        {
            var queryParams = new NameValueCollection();

            if (This.Query.IsNullOrEmpty())
                return queryParams;

            var @params = This.Query.RemoveLeft(1)
                             .Split('&')
                             .Select(param => param.Split('='));

            foreach (var param in @params)
            {
                queryParams.Add(param[0], param.ElementAtOrDefault(1));
            }

            return queryParams;
        }
    }
}
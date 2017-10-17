using System.Collections.Generic;
using Silphid.Loadzup.Caching;
using Silphid.Loadzup.Http.Caching;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class Options
    {
        public ContentType ContentType;
        public HttpCachePolicy? HttpCachePolicy;
        public MemoryCachePolicy? MemoryCachePolicy;
        public Dictionary<string, string> Headers;
        public bool IsAdditiveSceneLoading = true;
        public HttpMethod Method = HttpMethod.Get;
        public WWWForm PostForm;
        public string PutBody;

        public void SetHeader(string key, string value)
        {
            if (Headers == null)
                Headers = new Dictionary<string, string>();

            Headers[key] = value;
        }

        public static implicit operator Options(HttpCachePolicy httpCachePolicy) =>
            new Options { HttpCachePolicy = httpCachePolicy };

        public static implicit operator Options(ContentType contentType) =>
            new Options { ContentType = contentType };

        public static implicit operator Options(Dictionary<string, string> headers) =>
            new Options { Headers = headers };
    }
}
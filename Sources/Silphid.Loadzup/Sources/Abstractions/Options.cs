using System.Collections.Generic;
using Silphid.Loadzup.Caching;

namespace Silphid.Loadzup
{
    public class Options
    {
        public ContentType ContentType;
        public CachePolicy? CachePolicy;
        public Dictionary<string, string> RequestHeaders;
        public bool IsSceneLoadAdditive = true;

        public void SetRequestHeader(string key, string value)
        {
            if (RequestHeaders == null)
                RequestHeaders = new Dictionary<string, string>();

            RequestHeaders[key] = value;
        }

        public static implicit operator Options(CachePolicy cachePolicy) =>
            new Options { CachePolicy = cachePolicy };

        public static implicit operator Options(ContentType contentType) =>
            new Options { ContentType = contentType };

        public static implicit operator Options(Dictionary<string, string> requestHeaders) =>
            new Options { RequestHeaders = requestHeaders };
    }
}
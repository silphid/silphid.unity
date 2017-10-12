using System.Collections.Generic;
using Silphid.Loadzup.Caching;

namespace Silphid.Loadzup
{
    public class Options
    {
        public ContentType ContentType;
        public CachePolicy? CachePolicy;
        public Dictionary<string, string> Headers;
        public bool IsAdditiveSceneLoading = true;

        public void SetHeader(string key, string value)
        {
            if (Headers == null)
                Headers = new Dictionary<string, string>();

            Headers[key] = value;
        }

        public static implicit operator Options(CachePolicy cachePolicy) =>
            new Options { CachePolicy = cachePolicy };

        public static implicit operator Options(ContentType contentType) =>
            new Options { ContentType = contentType };

        public static implicit operator Options(Dictionary<string, string> headers) =>
            new Options { Headers = headers };
    }
}
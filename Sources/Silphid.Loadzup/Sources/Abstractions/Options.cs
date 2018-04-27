using System;
using System.Collections.Generic;
using System.Threading;
using Silphid.Extensions;
using Silphid.Loadzup.Caching;
using Silphid.Loadzup.Http;
using Silphid.Loadzup.Http.Caching;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class Options
    {
        public ContentType ContentType;
        public HttpCachePolicy? HttpCachePolicy;
        public MemoryCachePolicy? MemoryCachePolicy;
        public IDictionary<string, string> Headers;
        public IDictionary<object, object> CustomValues;
        public bool IsAdditiveSceneLoading = true;
        public HttpMethod Method = HttpMethod.Get;
        public WWWForm PostForm;
        public string PutBody;
        public TimeSpan? Timeout;
        public ICancelable CancellationToken;

        public void SetHeader(string key, string value)
        {
            if (Headers == null)
                Headers = new Dictionary<string, string>();

            Headers[key] = value;
        }

        public void SetCustomValue(object key, object value)
        {
            if (CustomValues == null)
                CustomValues = new Dictionary<object, object>();

            CustomValues[key] = value;
        }

        public static Options Clone(Options other) =>
            new Options
            {
                ContentType = other?.ContentType,
                HttpCachePolicy = other?.HttpCachePolicy,
                MemoryCachePolicy = other?.MemoryCachePolicy,
                Headers = other?.Headers?.Clone(),
                CustomValues = other?.CustomValues?.Clone(),
                IsAdditiveSceneLoading = other?.IsAdditiveSceneLoading ?? false,
                Method = other?.Method ?? HttpMethod.Get,
                PostForm = other?.PostForm,
                PutBody = other?.PutBody
            };
    }
}
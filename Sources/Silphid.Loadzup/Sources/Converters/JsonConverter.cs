using System;
using System.Text;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class JsonConverter : IConverter
    {
        public bool Supports<T>(byte[] bytes, ContentType contentType) =>
            contentType.MediaType == KnownMediaType.ApplicationJson;

        public IObservable<T> Convert<T>(byte[] bytes, ContentType contentType, Encoding encoding)
        {
            return Observable.Return(JsonUtility.FromJson<T>(encoding.GetString(bytes)));
        }
    }
}
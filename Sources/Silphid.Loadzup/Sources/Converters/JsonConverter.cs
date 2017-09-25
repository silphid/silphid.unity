using System;
using System.Text;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class JsonConverter : IConverter
    {
        public bool Supports<T>(object input, ContentType contentType) =>
            input is byte[] && contentType.MediaType == KnownMediaType.ApplicationJson;

        public IObservable<T> Convert<T>(object input, ContentType contentType, Encoding encoding)
        {
            return Observable.Return(JsonUtility.FromJson<T>(encoding.GetString((byte[]) input)));
        }
    }
}
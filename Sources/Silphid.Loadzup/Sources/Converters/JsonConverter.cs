using System;
using System.Text;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class JsonConverter : ConverterBase<byte[]>
    {
        public JsonConverter() : base(KnownMediaType.ApplicationJson)
        {
        }

        protected override IObservable<T> ConvertInternal<T>(byte[] input, ContentType contentType, Encoding encoding)
        {
            return Observable.Return(JsonUtility.FromJson<T>(encoding.GetString(input)));
        }
    }
}
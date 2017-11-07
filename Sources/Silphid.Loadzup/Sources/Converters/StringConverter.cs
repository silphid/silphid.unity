using System;
using System.Text;
using UniRx;

namespace Silphid.Loadzup
{
    public class StringConverter : ConverterBase<byte[]>
    {
        protected override IObservable<T> ConvertInternal<T>(byte[] input, ContentType contentType, Encoding encoding) =>
            Observable.Return((T) (object) encoding.GetString(input));
    }
}
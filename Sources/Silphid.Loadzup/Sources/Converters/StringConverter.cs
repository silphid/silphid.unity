using System;
using System.Text;
using UniRx;

namespace Silphid.Loadzup
{
    public class StringConverter : ConverterBase<string>
    {
        public StringConverter() : base("text/html")
        {
        }

        protected override IObservable<T> ConvertInternal<T>(string input, ContentType contentType, Encoding encoding) =>
            Observable.Return((T) (object) input);
    }
}
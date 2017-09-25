using System;
using System.Text;

namespace Silphid.Loadzup
{
    public interface IConverter
    {
        bool Supports<T>(object input, ContentType contentType);
        IObservable<T> Convert<T>(object input, ContentType contentType, Encoding encoding);
    }
}

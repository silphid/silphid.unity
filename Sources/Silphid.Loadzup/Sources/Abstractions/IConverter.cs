using System;
using System.Text;

namespace Silphid.Loadzup
{
    public interface IConverter
    {
        bool Supports<T>(byte[] bytes, ContentType contentType);
        IObservable<T> Convert<T>(byte[] bytes, ContentType contentType, Encoding encoding);
    }
}

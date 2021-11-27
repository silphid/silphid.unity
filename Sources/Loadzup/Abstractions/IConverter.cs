using System;
using System.Text;

namespace Silphid.Loadzup
{
    public interface IConverter
    {
        bool Supports<T>(object input, string mediaType);
        IObservable<T> Convert<T>(object input, string mediaType, Encoding encoding);
    }
}
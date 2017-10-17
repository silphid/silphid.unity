using System;
using System.Collections.Generic;

namespace Silphid.Loadzup.Http.Caching
{
    public interface IHttpCache
    {
        void Clear();
        void ClearExpired();
        bool Contains(Uri uri);
        IObservable<byte[]> Load(Uri uri);
        Dictionary<string, string> LoadHeaders(Uri uri);
        void Save(Uri uri, byte[] bytes, IDictionary<string, string> headers);
    }
}
using System;
using System.Collections.Generic;

namespace Silphid.Loadzup.Caching
{
    public interface ICacheStorage
    {
        void Clear();
        bool Contains(Uri uri);
        byte[] Load(Uri uri);
        Dictionary<string, string> LoadHeaders(Uri uri);
        void Save(Uri uri, byte[] bytes, IDictionary<string, string> headers);
    }
}
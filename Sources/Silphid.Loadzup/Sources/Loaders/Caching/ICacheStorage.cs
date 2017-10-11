using System;
using System.Collections.Generic;
using UniRx;

namespace Silphid.Loadzup.Caching
{
    public interface ICacheStorage
    {
        IObservable<Unit> DeleteExpired(DateTime now, TimeSpan defaultExpirySpan);
        void DeleteAll();
        bool Contains(Uri uri);
        byte[] Load(Uri uri);
        Dictionary<string, string> LoadHeaders(Uri uri);
        void Save(Uri uri, byte[] bytes, IDictionary<string, string> headers);
    }
}
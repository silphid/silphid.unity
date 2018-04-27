using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class QueuedLoader : ILoader
    {
        private class QueuedItem<T>
        {
            private readonly Subject<T> _subject = new Subject<T>();

            public readonly Uri Uri;
            public readonly Options Options;
            
            public IObservable<T> Loader => _subject;
            public bool IsCancelled => Options.CancellationToken.IsDisposed;

            public QueuedItem(Uri uri, Options options)
            {
                Uri = uri;
                Options = options;
            }
        }
        
        private readonly ILoader _loader;
        private readonly int _simulatenousLoading;

        private readonly Type[] _supportedTypes = 
        {
            typeof(Texture2D),
            typeof(Sprite),
            typeof(DisposableSprite)
        };

        private bool _isSupportedType<T>() =>
            _supportedTypes.Contains(typeof(T));

        public bool Supports<T>(Uri uri) =>
            _loader.Supports<T>(uri);

        public QueuedLoader(ILoader loader, int simulatenousLoading)
        {
            _loader = loader;
            _simulatenousLoading = simulatenousLoading;
        }

        public IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            if (!_isSupportedType<T>())
                return _loader.Load<T>(uri, options);
            
            var queuedItem = new QueuedItem<T>(uri, options);

            return queuedItem.Loader;
        }
    }
}
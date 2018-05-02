using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class QueuingLoader : ILoader
    {
        private interface IQueueItem
        {
            ICompletable Load();
        }

        private class QueueItem<T> : IQueueItem
        {
            private readonly Subject<T> _subject = new Subject<T>();
            private readonly IObservable<T> _loadRequest;
            private readonly ICancelable _cancellation;
            private bool IsCancelled => _cancellation.IsDisposed;

            public IObservable<T> QueuedLoad => _subject;

            public QueueItem(IObservable<T> loadRequest, ICancelable cancellation)
            {
                if(cancellation == null)
                    throw new InvalidOperationException("QueuedLoader requires cancellation token.");
                
                _loadRequest = loadRequest;
                _cancellation = cancellation;
            }

            public ICompletable Load()
            {
                if (IsCancelled)
                {
                    _subject.Dispose();
                    return Completable.Empty();
                }

                return _loadRequest
                    .Do(_subject.OnNext, _subject.OnError, _subject.OnCompleted)
                    .CatchIgnore()
                    .AsCompletable();
            }
        }

        private readonly ILoader _loader;
        private int _concurrencyLimit;
        private int _currentLoadingItems;
        private readonly LinkedList<IQueueItem> _queue = new LinkedList<IQueueItem>();

        private readonly Type[] _supportedTypes =
        {
            typeof(Texture2D),
            typeof(Sprite),
            typeof(DisposableSprite)
        };

        private bool IsSupportedType<T>() =>
            _supportedTypes.Contains(typeof(T));

        public bool Supports<T>(Uri uri) =>
            _loader.Supports<T>(uri);

        public QueuingLoader(ILoader loader, int concurrencyLimit)
        {
            _loader = loader;
            _concurrencyLimit = concurrencyLimit;
        }

        public void SetConcurrencyLimit(int limit)
        {
            lock (this)
            {
                _concurrencyLimit = limit;
                UpdateLoading();
            }
        }

        public IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            if (_concurrencyLimit == 0 || !IsSupportedType<T>())
                return _loader.Load<T>(uri, options);

            var queuedItem = new QueueItem<T>(_loader.Load<T>(uri, options), options?.CancellationToken);

            return queuedItem.QueuedLoad
                .DoOnSubscribe(() => Enqueue(queuedItem, options?.IsPriority ?? false));
        }

        private void Enqueue(IQueueItem queueItem, bool isPriority)
        {
            lock (this)
            {
                if (isPriority)
                    _queue.AddFirst(queueItem);
                else
                    _queue.AddLast(queueItem);

                UpdateLoading();
            }
        }

        private void UpdateLoading()
        {
            while (_currentLoadingItems < _concurrencyLimit && _currentLoadingItems < _queue.Count)
            {
                _currentLoadingItems++;

                var nextItem = _queue.First.Value;
                _queue.RemoveFirst();
                
                nextItem.Load()
                    .Subscribe(QueuedItemCompleted);
            }
        }

        private void QueuedItemCompleted()
        {
            lock (this)
            {
                _currentLoadingItems--;
                UpdateLoading();
            }
        }
    }
}
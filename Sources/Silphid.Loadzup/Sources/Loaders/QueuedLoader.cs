using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class QueuedLoader : ILoader
    {
        private interface IQueuedItem
        {
            ICompletable Load();
        }

        private class QueuedItem<T> : IQueuedItem
        {
            private readonly Subject<T> _subject = new Subject<T>();
            private readonly IObservable<T> _loadRequest;
            private readonly ICancelable _cancellation;
            private bool _isCancelled => _cancellation != null && _cancellation.IsDisposed;

            public IObservable<T> QueuedLoad => _subject;

            public QueuedItem(IObservable<T> loadRequest, ICancelable cancellation)
            {
                _loadRequest = loadRequest;
                _cancellation = cancellation;
            }

            public ICompletable Load()
            {
                if (_isCancelled)
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
        private int _simultaneousLoadingItemsLimit;
        private int _currentLoadingItems;
        private readonly LinkedList<IQueuedItem> _queue = new LinkedList<IQueuedItem>();

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

        public QueuedLoader(ILoader loader, int simultaneousLoadingItemsLimit)
        {
            _loader = loader;
            _simultaneousLoadingItemsLimit = simultaneousLoadingItemsLimit;
        }
        
        public void SetSimultaneousLoadingItemsLimit(int limit)
        {
            lock (this)
            {
                _simultaneousLoadingItemsLimit = limit;
                UpdateLoading();
            }
        }

        public IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            if (_simultaneousLoadingItemsLimit == 0 || !_isSupportedType<T>())
                return _loader.Load<T>(uri, options);

            var queuedItem = new QueuedItem<T>(_loader.Load<T>(uri, options), options?.CancellationToken);

            return queuedItem.QueuedLoad
                .DoOnSubscribe(() => AddToQueue(queuedItem, options?.IsPriority ?? false));
        }

        private void AddToQueue(IQueuedItem queuedItem, bool isPriority)
        {
            lock (this)
            {
                if (isPriority)
                    _queue.AddFirst(queuedItem);
                else
                    _queue.AddLast(queuedItem);

                UpdateLoading();
            }
        }

        private void UpdateLoading()
        {
            if (_queue.Count == 0)
                return;

            lock (this)
            {
                while (_currentLoadingItems < _simultaneousLoadingItemsLimit && _currentLoadingItems < _queue.Count)
                {
                    _currentLoadingItems++;
                    _queue.First.Value
                        .Load()
                        .Subscribe(QueuedItemCompleted);
                    _queue.RemoveFirst();
                }
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
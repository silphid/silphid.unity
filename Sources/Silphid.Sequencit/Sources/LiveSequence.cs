using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit
{
    public class LiveSequence : SequencerBase, ISequencer, IDisposable
    {
        #region Static methods

        public static LiveSequence Create(Action<ISequencer> action)
        {
            var sequence = new LiveSequence();
            action(sequence);
            return sequence;
        }

        public static LiveSequence Start()
        {
            var liveSequence = new LiveSequence();
            liveSequence.SubscribeAndForget();
            return liveSequence;
        }

        public static IDisposable Start(Action<ISequencer> action) =>
            Create(action).Subscribe();

        #endregion

        #region Private fields

        private Queue<object> _items = new Queue<object>();
        private IDisposable _currentExecution;
        private bool _isStarted;
        private bool _isExecuting;
        private Lapse _subscriptionLapse;
        
        #endregion
        
        #region IObservable<Unit> members

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            if (!_isStarted)
            {
                _subscriptionLapse = new Lapse(); 
                _isStarted = true;
                StartNext();
            }

            return new CompositeDisposable(
                Disposable.Create(Complete),
                _subscriptionLapse.Subscribe(observer));
        }

        #endregion

        #region ISequencer members

        public object Add(IObservable<Unit> observable) =>
            AddInternal(observable);

        public object Add(Func<IObservable<Unit>> selector) =>
            AddInternal(selector);

        private object AddInternal(object item)
        {
            _items.Enqueue(item);
            StartNext();
            return item;
        }

        #endregion

        #region IDisposable members

        public void Dispose()
        {
            Complete();
        }

        #endregion
        
        #region Public methods

        public void Complete()
        {
            if (!_isStarted)
                return;

            _subscriptionLapse.Dispose();
            _subscriptionLapse = null;
            _isStarted = false;
            _currentExecution?.Dispose();
        }
        
        public void Clear()
        {
            _items.Clear();
            _isExecuting = false;
            _currentExecution?.Dispose();
        }

        /// <summary>
        /// Removes observables from sequence, starting with given and up to the end.
        /// Currently executing item (if any) is not affected by this operation because
        /// it is now longer in the queue.
        /// </summary>
        /// <returns>Whether the given item was found and the truncation occurred.</returns>
        public bool TruncateBefore(object item) =>
            Truncate(item, isInclusive: true);

        /// <summary>
        /// Removes observables from sequence, starting after given item and up to the end.
        /// Currently executing item (if any) is not affected by this operation because
        /// it is now longer in the queue.
        /// </summary>
        /// <returns>Whether the given item was found and the truncation occurred.</returns>
        public bool TruncateAfter(object item) =>
            Truncate(item, isInclusive: false);

        /// <summary>
        /// Removes observables from sequence, starting with first one and up to given item exclusively.
        /// If the sequence was running, currently executing item (if any) is cancelled/disposed and the
        /// next item in queue (if any) is executed.
        /// </summary>
        /// <returns>Whether the given item was found and the truncation occurred.</returns>
        public bool SkipBefore(object item) =>
            Skip(item, isInclusive: false);

        /// <summary>
        /// Removes observables from sequence, starting with first one and up to given item inclusively.
        /// If the sequence was running, currently executing item (if any) is cancelled/disposed and the
        /// next item in queue (if any) is executed.
        /// </summary>
        /// <returns>Whether the given item was found and the truncation occurred.</returns>
        public bool SkipAfter(object item) =>
            Skip(item, isInclusive: true);

        #endregion

        #region Private methods

        private bool Skip(object item, bool isInclusive)
        {
            if (!_items.Contains(item))
                return false;

            _isExecuting = false;
            _currentExecution?.Dispose();
            
            while (_items.Peek() != item)
                _items.Dequeue();
            
            if (isInclusive)
                _items.Dequeue();

            if (_isStarted)
                StartNext();

            return true;
        }

        private bool Truncate(object item, bool isInclusive)
        {
            var index = _items.IndexOf(item);
            if (index == null)
                return false;

            var count = index.Value + (isInclusive ? 0 : 1);
            _items = new Queue<object>(_items.Take(count));
            return true;
        }

        private void StartNext()
        {
            if (!_isStarted || _isExecuting || _items.Count == 0)
                return;

            var item = _items.Dequeue();
            var observable = GetObservableFromItem(item);
            _isExecuting = true;
            _currentExecution = observable
                .Finally(() =>
                {
                    _isExecuting = false;
                    StartNext();
                })
                .Subscribe();
        }

        #endregion
    }
}
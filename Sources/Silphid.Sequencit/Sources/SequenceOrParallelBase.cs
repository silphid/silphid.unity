using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Silphid.Sequencit
{
    public abstract class SequenceOrParallelBase : SequencerBase, ISequencer
    {
        private class Collector : ISequencer
        {
            public readonly List<object> Items = new List<object>();

            public IDisposable Subscribe(IObserver<Unit> observer)
            {
                throw new NotSupportedException();
            }

            public object Add(IObservable<Unit> observable)
            {
                Items.Add(observable);
                return observable;
            }

            public object Add(Func<IObservable<Unit>> selector)
            {
                Items.Add(selector);
                return selector;
            }
        }
        
        #region Private fields

        private readonly Action<ISequencer> _action;
        private readonly List<object> _items = new List<object>();

        #endregion

        #region Constructors

        public SequenceOrParallelBase(Action<ISequencer> action = null)
        {
            _action = action;
        }
        
        #endregion

        #region ISequencer members

        public object Add(IObservable<Unit> observable)
        {
            _items.Add(observable);
            return observable;
        }

        public object Add(Func<IObservable<Unit>> selector)
        {
            _items.Add(selector);
            return selector;
        }

        #endregion

        #region IObservable<Unit> members

        public abstract IDisposable Subscribe(IObserver<Unit> observer);

        protected IEnumerable<IObservable<Unit>> GetObservables()
        {
            var observables = _items.Select(GetObservableFromItem);
            if (_action == null)
                return observables;

            var collector = new Collector();
            _action(collector);
            
            return collector.Items
                .Select(GetObservableFromItem)
                .Concat(observables);
        }

        #endregion
    }
}
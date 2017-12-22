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

            public IDisposable Subscribe(ICompletableObserver observer)
            {
                throw new NotSupportedException();
            }

            public object Add(ICompletable observable)
            {
                Items.Add(observable);
                return observable;
            }

            public object Add(Func<ICompletable> selector)
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

        public object Add(ICompletable observable)
        {
            _items.Add(observable);
            return observable;
        }

        public object Add(Func<ICompletable> selector)
        {
            _items.Add(selector);
            return selector;
        }

        #endregion

        #region ICompletable members

        public abstract IDisposable Subscribe(ICompletableObserver completableObserver);

        protected IEnumerable<ICompletable> GetCompletables()
        {
            var observables = _items.Select(GetCompletableFromItem);
            if (_action == null)
                return observables;

            var collector = new Collector();
            _action(collector);
            
            return collector.Items
                .Select(GetCompletableFromItem)
                .Concat(observables);
        }

        #endregion
    }
}
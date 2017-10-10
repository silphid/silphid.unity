using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace Silphid.Showzup
{
    public abstract class ViewModel<T> : IViewModel<T>, IDisposable, ICollection<IDisposable>
    {
        public T Model { get; }
        object IViewModel.Model => Model;
        protected readonly CompositeDisposable Disposables = new CompositeDisposable();

        protected ViewModel(T model)
        {
            Model = model;
        }

        public virtual void Dispose()
        {
            Disposables.Dispose();
        }
        
        #region ICollection<IDisposable>

        IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator() => Disposables.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Disposables.GetEnumerator();
        void ICollection<IDisposable>.Add(IDisposable item) => Disposables.Add(item);
        void ICollection<IDisposable>.Clear() => Disposables.Clear();
        bool ICollection<IDisposable>.Contains(IDisposable item) => Disposables.Contains(item);
        void ICollection<IDisposable>.CopyTo(IDisposable[] array, int arrayIndex) => Disposables.CopyTo(array, arrayIndex);
        bool ICollection<IDisposable>.Remove(IDisposable item) => Disposables.Remove(item);
        int ICollection<IDisposable>.Count => Disposables.Count;
        bool ICollection<IDisposable>.IsReadOnly => Disposables.IsReadOnly;

        #endregion
    }
}
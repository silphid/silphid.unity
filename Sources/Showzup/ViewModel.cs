using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace Silphid.Showzup
{
    public abstract class ViewModel<T> : IViewModel<T>, IDisposable, ICollection<IDisposable>
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        protected bool IsDisposed { get; private set; }

        protected ViewModel(T model)
        {
            Model = model;
        }

        #region IViewModel members

        public T Model { get; }
        object IViewModel.Model => Model;

        #endregion

        #region IDisposable members

        public void Dispose()
        {
            if (IsDisposed)
                return;

            OnDispose();

            IsDisposed = true;
        }

        protected virtual void OnDispose()
        {
            _disposables.Dispose();
        }

        #endregion

        #region ICollection<IDisposable> members

        IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator() => _disposables.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _disposables.GetEnumerator();
        void ICollection<IDisposable>.Add(IDisposable item) => _disposables.Add(item);
        void ICollection<IDisposable>.Clear() => _disposables.Clear();
        bool ICollection<IDisposable>.Contains(IDisposable item) => _disposables.Contains(item);

        void ICollection<IDisposable>.CopyTo(IDisposable[] array, int arrayIndex) =>
            _disposables.CopyTo(array, arrayIndex);

        bool ICollection<IDisposable>.Remove(IDisposable item) => _disposables.Remove(item);
        int ICollection<IDisposable>.Count => _disposables.Count;
        bool ICollection<IDisposable>.IsReadOnly => _disposables.IsReadOnly;

        #endregion
    }
}
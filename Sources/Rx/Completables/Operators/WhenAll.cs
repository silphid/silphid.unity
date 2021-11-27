using System;
using System.Collections.Generic;

namespace UniRx.Completables.Operators
{
    internal class WhenAllCompletable : OperatorCompletableBase
    {
        private readonly ICompletable[] sources;
        private readonly IEnumerable<ICompletable> sourcesEnumerable;

        public WhenAllCompletable(ICompletable[] sources)
            : base(false)
        {
            this.sources = sources;
        }

        public WhenAllCompletable(IEnumerable<ICompletable> sources)
            : base(false)
        {
            sourcesEnumerable = sources;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            if (sources != null)
                return new ArrayOuterObserver(sources, observer, cancel).Run();

            var list = sourcesEnumerable as IList<ICompletable> ?? new List<ICompletable>(sourcesEnumerable);

            return new ListOuterObserver(list, observer, cancel).Run();
        }

        #region ArrayOuterObserver

        private class ArrayOuterObserver : OperatorCompletableObserverBase
        {
            private readonly ICompletable[] sources;
            private readonly object gate = new object();
            private int completedCount;
            private int length;

            public ArrayOuterObserver(ICompletable[] sources, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.sources = sources;
            }

            public IDisposable Run()
            {
                length = sources.Length;

                if (length == 0)
                {
                    try
                    {
                        observer.OnCompleted();
                    }
                    finally
                    {
                        Dispose();
                    }

                    return Disposable.Empty;
                }

                completedCount = 0;

                var subscriptions = new IDisposable[length];
                for (int index = 0; index < length; index++)
                {
                    var source = sources[index];
                    var innerObserver = new ArrayInnerObserver(this);
                    subscriptions[index] = source.Subscribe(innerObserver);
                }

                return StableCompositeDisposable.CreateUnsafe(subscriptions);
            }

            public override void OnError(Exception error)
            {
                try
                {
                    observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnCompleted()
            {
                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }

            #region ArrayInnerObserver

            private class ArrayInnerObserver : ICompletableObserver
            {
                private readonly ArrayOuterObserver parent;
                private bool isCompleted;

                public ArrayInnerObserver(ArrayOuterObserver parent)
                {
                    this.parent = parent;
                }

                public void OnError(Exception error)
                {
                    lock (parent.gate)
                    {
                        if (!isCompleted)
                        {
                            parent.OnError(error);
                        }
                    }
                }

                public void OnCompleted()
                {
                    lock (parent.gate)
                    {
                        if (!isCompleted)
                        {
                            isCompleted = true;
                            parent.completedCount++;
                            if (parent.completedCount == parent.length)
                                parent.OnCompleted();
                        }
                    }
                }

                #endregion
            }
        }

        #endregion

        #region ListOuterObserver

        private class ListOuterObserver : OperatorCompletableObserverBase
        {
            private readonly IList<ICompletable> sources;
            private readonly object gate = new object();
            private int completedCount;
            private int length;

            public ListOuterObserver(IList<ICompletable> sources, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.sources = sources;
            }

            public IDisposable Run()
            {
                length = sources.Count;

                if (length == 0)
                {
                    try
                    {
                        observer.OnCompleted();
                    }
                    finally
                    {
                        Dispose();
                    }

                    return Disposable.Empty;
                }

                completedCount = 0;

                var subscriptions = new IDisposable[length];
                for (int index = 0; index < length; index++)
                {
                    var source = sources[index];
                    var innerObserver = new ListInnerObserver(this);
                    subscriptions[index] = source.Subscribe(innerObserver);
                }

                return StableCompositeDisposable.CreateUnsafe(subscriptions);
            }

            public override void OnError(Exception error)
            {
                try
                {
                    observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnCompleted()
            {
                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }

            #region ListInnerObserver

            private class ListInnerObserver : ICompletableObserver
            {
                private readonly ListOuterObserver parent;
                private bool isCompleted;

                public ListInnerObserver(ListOuterObserver parent)
                {
                    this.parent = parent;
                }

                public void OnError(Exception error)
                {
                    lock (parent.gate)
                    {
                        if (!isCompleted)
                        {
                            parent.OnError(error);
                        }
                    }
                }

                public void OnCompleted()
                {
                    lock (parent.gate)
                    {
                        if (!isCompleted)
                        {
                            isCompleted = true;
                            parent.completedCount++;
                            if (parent.completedCount == parent.length)
                                parent.OnCompleted();
                        }
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}
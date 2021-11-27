using System;

namespace UniRx.Completables.Operators
{
    // DoOnError, DoOnCompleted, DoOnTerminate, DoOnSubscribe, DoOnCancel

    internal class DoOnErrorCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly Action<Exception> onError;

        public DoOnErrorCompletable(ICompletable source, Action<Exception> onError)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onError = onError;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnErrorObserver(this, observer, cancel).Run();
        }

        private class DoOnErrorObserver : OperatorCompletableObserverBase
        {
            private readonly DoOnErrorCompletable parent;

            public DoOnErrorObserver(DoOnErrorCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                return parent.source.Subscribe(this);
            }

            public override void OnError(Exception error)
            {
                try
                {
                    parent.onError(error);
                }
                catch (Exception ex)
                {
                    try
                    {
                        observer.OnError(ex);
                    }
                    finally
                    {
                        Dispose();
                    }

                    return;
                }

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
        }
    }

    internal class DoOnCompletedCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        public readonly Action onCompleted;

        public DoOnCompletedCompletable(ICompletable source, Action onCompleted)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onCompleted = onCompleted;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnCompleted(this, observer, cancel).Run();
        }

        private class DoOnCompleted : OperatorCompletableObserverBase
        {
            private readonly DoOnCompletedCompletable parent;

            public DoOnCompleted(DoOnCompletedCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                return parent.source.Subscribe(this);
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
                    parent.onCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    Dispose();
                    return;
                }

                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }
        }
    }

    internal class DoOnTerminateCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        public readonly Action onTerminate;

        public DoOnTerminateCompletable(ICompletable source, Action onTerminate)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onTerminate = onTerminate;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnTerminate(this, observer, cancel).Run();
        }

        class DoOnTerminate : OperatorCompletableObserverBase
        {
            private readonly DoOnTerminateCompletable parent;

            public DoOnTerminate(DoOnTerminateCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                return parent.source.Subscribe(this);
            }

            public override void OnError(Exception error)
            {
                try
                {
                    parent.onTerminate();
                }
                catch (Exception ex)
                {
                    try
                    {
                        observer.OnError(ex);
                    }
                    finally
                    {
                        Dispose();
                    }

                    return;
                }

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
                    parent.onTerminate();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    Dispose();
                    return;
                }

                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }
        }
    }

    internal class DoOnSubscribeCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly Action onSubscribe;

        public DoOnSubscribeCompletable(ICompletable source, Action onSubscribe)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onSubscribe = onSubscribe;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnSubscribe(this, observer, cancel).Run();
        }

        private class DoOnSubscribe : OperatorCompletableObserverBase
        {
            private readonly DoOnSubscribeCompletable parent;

            public DoOnSubscribe(DoOnSubscribeCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                try
                {
                    parent.onSubscribe();
                }
                catch (Exception ex)
                {
                    try
                    {
                        observer.OnError(ex);
                    }
                    finally
                    {
                        Dispose();
                    }

                    return Disposable.Empty;
                }

                return parent.source.Subscribe(this);
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
        }
    }

    internal class DoOnCancelCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        public readonly Action onCancel;

        public DoOnCancelCompletable(ICompletable source, Action onCancel)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onCancel = onCancel;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnCancel(this, observer, cancel).Run();
        }

        private class DoOnCancel : OperatorCompletableObserverBase
        {
            private readonly DoOnCancelCompletable parent;
            private bool isCompletedCall;

            public DoOnCancel(DoOnCancelCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                return StableCompositeDisposable.Create(
                    parent.source.Subscribe(this),
                    Disposable.Create(
                        () =>
                        {
                            if (!isCompletedCall)
                            {
                                parent.onCancel();
                            }
                        }));
            }

            public override void OnError(Exception error)
            {
                isCompletedCall = true;
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
                isCompletedCall = true;
                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }
        }
    }
}
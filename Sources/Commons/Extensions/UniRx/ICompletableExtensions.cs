using System;
using JetBrains.Annotations;
using UniRx;
using UniRx.Completables.Operators;

namespace Silphid.Extensions
{
    public static class ICompletableExtensions
    {
        #region AutoDetach

        internal class AutoDetachCompletable : OperatorCompletableBase
        {
            private readonly ICompletable _source;

            public AutoDetachCompletable(ICompletable source)
                : base(source.IsRequiredSubscribeOnCurrentThread())
            {
                _source = source;
            }

            protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel) =>
                _source.Subscribe(CompletableObserver.CreateAutoDetachObserver(observer, cancel));
        }

        [Pure]
        public static ICompletable AutoDetach(this ICompletable This) =>
            new AutoDetachCompletable(This);

        #endregion

        #region SubscribeAndForget

        public static IDisposable SubscribeAndForget(this ICompletable This) =>
            AutoDetach(This)
               .Subscribe();

        public static IDisposable SubscribeAndForget(this ICompletable This, ICompletableObserver observer) =>
            AutoDetach(This)
               .Subscribe(observer);

        public static IDisposable SubscribeAndForget(this ICompletable This, Action<Exception> onError) =>
            AutoDetach(This)
               .Subscribe(onError);

        public static IDisposable SubscribeAndForget(this ICompletable This, Action onCompleted) =>
            AutoDetach(This)
               .Subscribe(onCompleted);

        public static IDisposable SubscribeAndForget(this ICompletable This,
                                                     Action<Exception> onError,
                                                     Action onCompleted) =>
            AutoDetach(This)
               .Subscribe(onError, onCompleted);

        #endregion
    }
}
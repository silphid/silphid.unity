using System;
using Silphid.Requests;

namespace Silphid.Showzup
{
    public static class IViewExtensions
    {
        #region Request helpers

        public static void Send(this IView This, IRequest request) =>
            This.GameObject.Send(request);

        public static void Send(this IView This, Exception exception) =>
            This.GameObject.Send(exception);

        public static void Send<TRequest>(this IView This) where TRequest : IRequest, new() =>
            This.GameObject.Send(new TRequest());

        #endregion
    }
}
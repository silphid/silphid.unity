using System;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Requests
{
    public static class GameObjectExtensions
    {
        public static IRequestSink AsRequestSink(this GameObject This) =>
            new GameObjectRequestSink(This);

        public static void Send(this GameObject This, IRequest request) =>
            This.Ancestors<IRequestHandler>()
                .Select(x => x.Handle(request))

                 // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                .FirstOrDefault(x => x);

        public static void Send<TRequest>(this GameObject This) where TRequest : IRequest, new() =>
            This.Send(new TRequest());

        public static void Send(this Component This, IRequest request) =>
            This.gameObject.Send(request);

        public static void Send<TRequest>(this Component This) where TRequest : IRequest, new() =>
            This.Send(new TRequest());

        public static void Send(this GameObject This, Exception exception) =>
            This.Send(exception.AsRequest());

        public static void Send(this Component This, Exception exception) =>
            This.gameObject.Send(exception);
    }
}
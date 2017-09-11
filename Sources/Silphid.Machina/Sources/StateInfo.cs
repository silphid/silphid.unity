using System;
using System.Collections.Generic;
using Silphid.Requests;

namespace Silphid.Machina
{
    public class StateInfo : IStateConfig
    {
        public readonly Dictionary<Type, Func<object, IRequest>> Handlers = new Dictionary<Type, Func<object, IRequest>>();
        
        /// <summary>
        /// Adds an handler for given trigger type T, which should return whether it handled the trigger or not.
        /// </summary>
        public void Handle<T>(Func<T, IRequest> handler)
        {
            Handlers[typeof(T)] = x => handler((T) x);
        }

        /// <summary>
        /// Adds an handler for given trigger type T, which is assumed to always handle the trigger.
        /// </summary>
        public void Handle<T>(Action<T> handler)
        {
            Handlers[typeof(T)] = x =>
            {
                handler((T) x);
                return null;
            };
        }
    }
}
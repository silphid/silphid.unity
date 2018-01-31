using System;

namespace Silphid.Showzup.Flows
{
    public class FlowFactory : IFlowFactory
    {
        protected readonly Func<Type, object[], IFlow> _func;

        public FlowFactory(Func<Type, object[], IFlow> func)
        {
            _func = func;
        }

        public IFlow Create(Type type, object[] parameters)
        {
            return _func(type, parameters);
        }
    }
}
using System;

namespace Silphid.Showzup.Flows
{
    public class FlowFactory : IFlowFactory
    {
        protected readonly Func<Type, IFlowOptions, IFlow> _func;

        public FlowFactory(Func<Type, IFlowOptions, IFlow> func)
        {
            _func = func;
        }

        public IFlow Create(Type type, IFlowOptions options)
        {
            return _func(type, options);
        }
    }
}
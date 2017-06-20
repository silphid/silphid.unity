using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly Func<Type, IEnumerable<object>, IViewModel> _func;

        public ViewModelFactory(Func<Type, IEnumerable<object>, IViewModel> func)
        {
            _func = func;
        }

        public IViewModel Create(Type type, IEnumerable<object> parameters)
        {
            return _func(type, parameters);
        }
    }
}
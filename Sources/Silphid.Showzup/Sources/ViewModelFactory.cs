using System;

namespace Silphid.Showzup
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly Func<object, Type, IViewModel> func;

        public ViewModelFactory(Func<object, Type, IViewModel> func)
        {
            this.func = func;
        }

        public IViewModel Create(object model, Type type)
        {
            return func(model, type);
        }
    }
}
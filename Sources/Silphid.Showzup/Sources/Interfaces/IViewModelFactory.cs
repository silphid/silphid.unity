using System;

namespace Silphid.Showzup
{
    public interface IViewModelFactory
    {
        IViewModel Create(object model, Type type);
    }
}
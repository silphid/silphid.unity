using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    public interface IViewModelFactory
    {
        IViewModel Create(Type viewModelType, IEnumerable<object> parameters);
    }
}
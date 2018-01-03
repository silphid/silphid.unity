using UnityEngine;

namespace Silphid.Showzup
{
    public interface IView
    {
        IViewModel ViewModel { get; set; }
        GameObject GameObject { get; }
    }
    
    public interface IView<out TViewModel> : IView where TViewModel : IViewModel
    {
        new TViewModel ViewModel { get; }
    }
}
using UnityEngine;

namespace Silphid.Showzup
{
    public interface IView
    {
        bool IsActive { get; set; }
        object ViewModel { get; set; }
        GameObject GameObject { get; }
    }
    
    public interface IView<out TViewModel> : IView where TViewModel : IViewModel
    {
        TViewModel ViewModel { get; }
    }
}
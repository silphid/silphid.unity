using UnityEngine;

namespace Silphid.Showzup
{
    public interface IView
    {
        bool IsActive { get; set; }
        object ViewModel { get; set; }
        GameObject GameObject { get; }
    }
}
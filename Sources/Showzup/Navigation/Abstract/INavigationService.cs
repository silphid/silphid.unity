using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public interface INavigationService
    {
        void SetSelection(GameObject gameObject, bool forceNotify = false);
        IReadOnlyReactiveProperty<GameObject> Selection { get; }
        IReactiveProperty<GameObject[]> SelectionAndAncestors { get; }
    }
}
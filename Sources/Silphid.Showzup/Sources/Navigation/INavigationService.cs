using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public interface INavigationService
    {
        void SetSelection(GameObject gameObject);
        ReadOnlyReactiveProperty<GameObject> Selection { get; }
        ReactiveProperty<GameObject[]> SelectionAndAncestors { get; }
    }
}
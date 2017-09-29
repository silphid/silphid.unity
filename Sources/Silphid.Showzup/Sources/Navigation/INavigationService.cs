using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public interface INavigationService
    {
        ReactiveProperty<GameObject> Selection { get; }
        ReactiveProperty<GameObject> Focus { get; }
        ReactiveProperty<GameObject[]> SelectionAndAncestors { get; }
        ReactiveProperty<GameObject[]> FocusAndAncestors { get; }
    }
}
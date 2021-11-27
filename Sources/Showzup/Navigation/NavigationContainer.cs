using System;
using Silphid.Extensions;
using Silphid.Showzup.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Plugins.Silphid.Showzup.Navigation
{
    public class NavigationContainer : MonoBehaviour, IMoveHandler
    {
        public GameObject[] Items;
        public NavigationOrientation Orientation;

        private readonly NavigationHandler _navigationHandler = new NavigationHandler();

        private void Start()
        {
            if (Orientation == NavigationOrientation.None)
                throw new InvalidOperationException(
                    $"NavigationContainer is missing orientation value on gameObject {gameObject.ToHierarchyPath()}");

            var items = Items != null && Items.Length > 0
                            ? Items
                            : this.Children();

            var direction = Orientation == NavigationOrientation.Horizontal
                                ? MoveDirection.Right
                                : MoveDirection.Down;

            items.Pairwise()
                 .ForEach(x => _navigationHandler.BindBidirectional(x.Previous, x.Current, direction));
        }

        public void OnMove(AxisEventData eventData)
        {
            _navigationHandler.OnMove(eventData);
        }
    }
}
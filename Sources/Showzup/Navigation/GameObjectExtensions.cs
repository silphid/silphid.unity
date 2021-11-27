using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup.Navigation
{
    public static class GameObjectExtensions
    {
        #region Selection

        public static void Select(this GameObject This, bool forceNotify = false) =>
            NavigationService.Instance.SetSelection(This, forceNotify);

        public static void Select(this Component This, bool forceNotify = false) =>
            This.gameObject.Select(forceNotify);

        public static void Select(this IView This, bool forceNotify = false) =>
            This.GameObject.Select(forceNotify);

        public static bool IsSelected(this GameObject This) =>
            NavigationService.Instance.Selection.Value == This;

        public static bool IsSelected(this Component This) =>
            This.gameObject.IsSelected();

        public static IObservable<bool> IsSelectedObservable(this GameObject This) =>
            NavigationService.Instance.Selection.Select(x => x == This)
                             .StartWith(This.IsSelected());

        public static bool IsDescendantSelected(this GameObject This) =>
            NavigationService.Instance.Selection.Value != This &&
            NavigationService.Instance.SelectionAndAncestors.Value.Contains(This);

        public static bool IsDescendantSelected(this Component This) =>
            This.gameObject.IsDescendantSelected();

        public static bool IsSelfOrDescendantSelected(this GameObject This) =>
            NavigationService.Instance.SelectionAndAncestors.Value.Contains(This);

        public static bool IsSelfOrDescendantSelected(this Component This) =>
            This.gameObject.IsSelfOrDescendantSelected();

        #endregion
    }
}
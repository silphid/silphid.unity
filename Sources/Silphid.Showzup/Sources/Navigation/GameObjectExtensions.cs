using System.Linq;
using UnityEngine;

namespace Silphid.Showzup.Navigation
{
    public static class GameObjectExtensions
    {
        #region Selection

        public static void Select(this GameObject This) =>
            NavigationService.Instance.SetSelection(This);

        public static void Select(this Component This) =>
            This.gameObject.Select();

        public static void Select(this IView This) =>
            This.GameObject.Select();

        public static bool IsSelected(this GameObject This) =>
            NavigationService.Instance.Selection.Value == This;

        public static bool IsSelected(this Component This) =>
            This.gameObject.IsSelected();

        public static bool IsSelected(this IView This) =>
            This.GameObject.IsSelected();

        public static bool IsDescendantSelected(this GameObject This) =>
            NavigationService.Instance.Selection.Value != This &&
            NavigationService.Instance.SelectionAndAncestors.Value.Contains(This);

        public static bool IsDescendantSelected(this Component This) =>
            This.gameObject.IsDescendantSelected();

        public static bool IsDescendantSelected(this IView This) =>
            This.GameObject.IsDescendantSelected();

        public static bool IsSelfOrDescendantSelected(this GameObject This) =>
            NavigationService.Instance.SelectionAndAncestors.Value.Contains(This);
        
        public static bool IsSelfOrDescendantSelected(this Component This) =>
            This.gameObject.IsSelfOrDescendantSelected();
        
        public static bool IsSelfOrDescendantSelected(this IView This) =>
            This.GameObject.IsSelfOrDescendantSelected();

        #endregion
    }
}
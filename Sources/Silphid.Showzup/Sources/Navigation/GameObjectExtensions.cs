using System.Linq;
using UnityEngine;

namespace Silphid.Showzup.Navigation
{
    public static class GameObjectExtensions
    {
        #region Selection

        public static void Select(this GameObject This) =>
            NavigationService.Instance.Selection.Value = This;

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

        #region Focus

        public static void Focus(this GameObject This) =>
            NavigationService.Instance.Focus.Value = This;

        public static void Focus(this Component This) =>
            This.gameObject.Focus();

        public static void Focus(this IView This) =>
            This.GameObject.Focus();

        public static bool IsFocused(this GameObject This) =>
            NavigationService.Instance.Focus.Value == This;

        public static bool IsFocused(this Component This) =>
            This.gameObject.IsFocused();

        public static bool IsFocused(this IView This) =>
            This.GameObject.IsFocused();

        public static bool IsDescendantFocused(this GameObject This) =>
            NavigationService.Instance.Focus.Value != This &&
            NavigationService.Instance.SelectionAndAncestors.Value.Contains(This);

        public static bool IsDescendantFocused(this Component This) =>
            This.gameObject.IsDescendantFocused();

        public static bool IsDescendantFocused(this IView This) =>
            This.GameObject.IsDescendantFocused();

        public static bool IsSelfOrDescendantFocused(this GameObject This) =>
            NavigationService.Instance.SelectionAndAncestors.Value.Contains(This);
        
        public static bool IsSelfOrDescendantFocused(this Component This) =>
            This.gameObject.IsSelfOrDescendantFocused();
        
        public static bool IsSelfOrDescendantFocused(this IView This) =>
            This.GameObject.IsSelfOrDescendantFocused();

        #endregion
    }
}
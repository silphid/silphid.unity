using Silphid.Extensions;

namespace Silphid.Showzup
{
    public static class IViewExtensions
    {
        #region Selection

        public static void Select(this IView This) =>
            This.GameObject.Select();

        public static void SelectDeferred(this IView This) =>
            This.GameObject.SelectDeferred();

        public static bool IsSelected(this IView This) =>
            This.GameObject.IsSelected();

        #endregion
    }
}
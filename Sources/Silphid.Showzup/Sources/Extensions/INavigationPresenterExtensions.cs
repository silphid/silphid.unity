using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Silphid.Showzup
{
    public static class INavigationPresenterExtensions
    {
        public static void DropFromHistory(this INavigationPresenter This, int count)
        {
            Debug.Log($"#Nav# DropFromHistory({count})");
            This.AssertCanAlterHistory();

            if (count > This.History.Value.Count)
                throw new InvalidOperationException($"Cannot remove {count} views from history because it only contains {This.History.Value.Count}");

            This.History.Value = This.History.Value
                .Take(This.History.Value.Count - count)
                .ToList();
        }

        public static void ClearHistory(this INavigationPresenter This)
        {
            Debug.Log("#Nav# ClearHistory()");
            This.AssertCanAlterHistory();

            This.History.Value = new List<IView>();
        }

        private static void AssertCanAlterHistory(this INavigationPresenter This)
        {
            if (This.IsNavigating.Value)
                throw new InvalidOperationException("Cannot alter history during navigation");
        }
    }
}
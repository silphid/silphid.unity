namespace Silphid.Showzup
{
    public static class IViewOptionsExtensions
    {
        #region WithValue/Flag

        public static IViewOptions WithValue(this IViewOptions This, object key, object value) =>
            value != null
                ? new ViewOptions(This, key, value)
                : This;

        public static IViewOptions WithValue<T>(this IViewOptions This, T value) =>
            value != null
                ? new ViewOptions(This, typeof(T), value)
                : This;

        public static IViewOptions WithFlag(this IViewOptions This, object key, bool value = true) =>
            This.WithValue(key, value);

        #endregion
    }
}
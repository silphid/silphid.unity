using Silphid.Extensions;

namespace Silphid.Loadzup
{
    public static class OptionsExtensions
    {
        public static void SetCustomValue<T>(this Options This, T value) =>
            This.SetCustomValue(typeof(T), value);

        public static void SetCustomFlag(this Options This, object key, bool value = true) =>
            This.SetCustomValue(key, value);

        public static object GetCustomValue(this Options This, object key, object defaultValue) =>
            This?.CustomValues?.GetValueOrDefault(key, defaultValue) ?? defaultValue;

        public static T GetCustomValue<T>(this Options This, T defaultValue = default(T)) =>
            (T) (This?.CustomValues?.GetValueOrDefault(typeof(T), defaultValue) ?? defaultValue);

        public static bool GetCustomFlag(this Options This, object key, bool defaultValue = false) =>
            (bool) This.GetCustomValue(key, defaultValue);
    }
}
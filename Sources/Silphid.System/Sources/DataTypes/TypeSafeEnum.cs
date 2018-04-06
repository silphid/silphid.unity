using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Silphid.Extensions;
// ReSharper disable StaticMemberInGenericType

namespace Silphid.DataTypes
{
    [DebuggerDisplay("{Name} ({Value})")]
    public abstract class TypeSafeEnum<T> where T : TypeSafeEnum<T>
    {
        #region Statics

        private static readonly List<T> _items = new List<T>();
        private static int _autoValue;

        protected static TCustom Add<TCustom>(TCustom item) where TCustom : T
        {
            if (item._value == null)
                item._value = _autoValue++;

            _items.Add(item);
            return item;
        }

        public static IReadOnlyList<T> All { get; } = _items.AsReadOnly();

        public static T[] Nothing { get; } = { };

        public static T FromValue(int value) =>
            All.FirstOrDefault(x => x.Value.Equals(value));

        public static T FromName(string name) =>
            All.FirstOrDefault(x => x.Name.CaseInsensitiveEquals(name));

        #endregion

        #region Properties

        private int? _value;
        public int Value => _value.Value;
        
        public string Name { get; }  

        #endregion

        #region Lifetime

        protected TypeSafeEnum(int value, string name)
        {
            _value = value;
            Name = name;
        }

        protected TypeSafeEnum(string name)
        {
            Name = name;
        }

        #endregion

        #region Object overrides

        public override string ToString() => Name;

        #endregion

        #region Equality members

        protected bool Equals(TypeSafeEnum<T> other) =>
            Value == other.Value;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TypeSafeEnum<T>)obj);
        }

        public override int GetHashCode() =>
            GetType().GetHashCode() * 397 ^ Value.GetHashCode();

        #endregion
    }
}
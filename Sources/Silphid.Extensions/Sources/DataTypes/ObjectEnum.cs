using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Silphid.Extensions.DataTypes
{
    [DebuggerDisplay("{Name} ({Id})")]
    public abstract class ObjectEnum<T> where T : ObjectEnum<T>
    {
        #region Statics

        #region Initialization

        private static bool s_isInitialized;
        private static bool? s_isImplicitIds;

        private static void EnsureInitialized()
        {
            if (s_isInitialized)
                return;

            typeof(T)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.FieldType.IsAssignableTo<T>())
                .Select(x => new
                    {
                        Value = (T)x.GetValue(null),
                        x.Name
                    })
                .ForEach((i, x) => InitializeIdAndName(x.Value, i, x.Name));

            s_isInitialized = true;
        }

        private static void InitializeIdAndName(T item, int id, string name)
        {
            if (item._id == null)
            {
                // Items must either have implicit OR explicit ids altogether
                if (!(s_isImplicitIds ?? true))
                    ThrowBothImplicitAndExplicitIds();

                // Implicit id
                item._id = id;
                s_isImplicitIds = true;
            }
            else
            {
                // Items must either have implicit OR explicit ids altogether
                if (s_isImplicitIds ?? false)
                    ThrowBothImplicitAndExplicitIds();

                // Explicit id
                s_isImplicitIds = false;
            }

            item._name = name;
        }

        private static void ThrowBothImplicitAndExplicitIds()
        {
            throw new NotSupportedException("Object enum {0} cannot have both implicit and explicit ids".Formatted(typeof(T).Name));
        }

        #endregion

        #region All

        private static List<T> _all;

        public static List<T> All
        {
            get
            {
                return _all ?? (_all = typeof(T)
                    .GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(x => x.FieldType.IsAssignableTo<T>())
                    .Select(x => (T)x.GetValue(null))
                    .ToList());
            }
        }

        #endregion

        #region Nothing

        private static T[] _nothing;

        public static T[] Nothing
        {
            get { return _nothing ?? (_nothing = new T[] { }); }
        }

        #endregion

        #region Static methods

        public static T FromId(int id)
        {
            return All.FirstOrDefault(x => x.Id.Equals(id));
        }

        public static T FromName(string name)
        {
            return All.FirstOrDefault(x => x.Name.CaseInsensitiveEquals(name));
        }

        #endregion

        #endregion

        #region Id

        private int? _id;

        public int Id
        {
            get
            {
                EnsureInitialized();
                return _id.Value;
            }
        }

        #endregion

        #region Name

        private string _name;

        public string Name  
        {
            get
            {
                EnsureInitialized();
                return _name;
            }
        }

        #endregion

        #region Lifetime

        protected ObjectEnum()
        {
        }

        protected ObjectEnum(int id)
        {
            _id = id;
        }

        #endregion

        #region Object overrides

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Equality members

        protected bool Equals(ObjectEnum<T> other)
        {
            return Name.CaseInsensitiveEquals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ObjectEnum<T>)obj);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() * 397 ^ Name.GetHashCode();
        }

        #endregion
    }
}
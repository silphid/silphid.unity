using System;
using System.Reflection;
using log4net;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class SerializableVariant
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SerializableVariant));

        [SerializeField] private ClassTypeReference _typeRef;
        [SerializeField] private string _name;

        public SerializableVariant(IVariant variant)
        {
            _typeRef = variant.GetType();
            _name = variant.Name;
        }

        public IVariant Variant
        {
            get
            {
                var field = _typeRef.Type.GetField(_name, BindingFlags.Static | BindingFlags.Public);

                if (field == null)
                    Log.Warn($"Unrecognized variant {_name}");

                return (IVariant) field?.GetValue(null);
            }
        }
    }
}
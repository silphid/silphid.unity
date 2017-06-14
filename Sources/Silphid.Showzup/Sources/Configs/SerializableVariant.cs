using System;
using System.Reflection;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class SerializableVariant
    {
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
                Debug.Log($"Deserializing variant: {_name}");
                var field = _typeRef.Type.GetField(_name, BindingFlags.Static | BindingFlags.Public);
                var variant = (IVariant) field.GetValue(null);
                Debug.Log($"Deserialized variant: {variant.Name}");
                return variant;
            }
        }
    }
}
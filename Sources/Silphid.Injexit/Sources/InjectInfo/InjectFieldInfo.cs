using System;
using System.Reflection;

namespace Silphid.Injexit
{
    public class InjectFieldInfo : InjectFieldOrPropertyInfo
    {
        private readonly FieldInfo _fieldInfo;

        public InjectFieldInfo(FieldInfo fieldInfo, Type type, bool isOptional, string id) : base(type, isOptional, id)
        {
            _fieldInfo = fieldInfo;
        }

        public override void SetValue(object obj, object value)
        {
            _fieldInfo.SetValue(obj, value);
        }
    }
}
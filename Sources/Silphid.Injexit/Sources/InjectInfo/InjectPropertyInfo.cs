using System;
using System.Reflection;

namespace Silphid.Injexit
{
    public class InjectPropertyInfo : InjectFieldOrPropertyInfo
    {
        private readonly PropertyInfo _propertyInfo;

        public InjectPropertyInfo(PropertyInfo propertyInfo, Type type, bool isOptional, string id) : base(propertyInfo.Name, type, isOptional, id)
        {
            _propertyInfo = propertyInfo;
        }

        public override void SetValue(object obj, object value)
        {
            _propertyInfo.SetValue(obj, value);
        }
    }
}
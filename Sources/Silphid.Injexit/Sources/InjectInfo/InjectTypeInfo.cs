using System;

namespace Silphid.Injexit
{
    public class InjectTypeInfo
    {
        public Type Type { get; }
        public InjectConstructorInfo Constructor { get; }
        public InjectMethodInfo[] Methods { get; }
        public InjectFieldOrPropertyInfo[] FieldsAndProperties { get; }

        public InjectTypeInfo(Type type, InjectConstructorInfo constructor, InjectMethodInfo[] methods, InjectFieldOrPropertyInfo[] fieldsAndProperties)
        {
            Type = type;
            Constructor = constructor;
            Methods = methods;
            FieldsAndProperties = fieldsAndProperties;
        }
    }
}
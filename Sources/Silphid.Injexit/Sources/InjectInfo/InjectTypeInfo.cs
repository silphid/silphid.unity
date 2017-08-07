namespace Silphid.Injexit
{
    public class InjectTypeInfo
    {
        public InjectConstructorInfo Constructor { get; }
        public InjectMethodInfo[] Methods { get; }
        public InjectFieldOrPropertyInfo[] FieldsAndProperties { get; }

        public InjectTypeInfo(InjectConstructorInfo constructor, InjectMethodInfo[] methods, InjectFieldOrPropertyInfo[] fieldsAndProperties)
        {
            Constructor = constructor;
            Methods = methods;
            FieldsAndProperties = fieldsAndProperties;
        }
    }
}
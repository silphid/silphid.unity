using System;

namespace Silphid.Injexit
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ExtraBindAttribute : Attribute
    {
        public Type Type { get; }

        public ExtraBindAttribute(Type type)
        {
            Type = type;
        }
    }
}
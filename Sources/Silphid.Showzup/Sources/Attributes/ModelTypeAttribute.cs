using System;

namespace Silphid.Showzup
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModelTypeAttribute : Attribute
    {
        public Type Type { get; }

        public ModelTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
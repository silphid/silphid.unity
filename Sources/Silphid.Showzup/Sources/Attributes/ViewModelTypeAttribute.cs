using System;

namespace Silphid.Showzup
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ViewModelTypeAttribute : Attribute
    {
        public Type Type { get; }

        public ViewModelTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
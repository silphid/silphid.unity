using System;

namespace Silphid.Injexit
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Field |
        AttributeTargets.Property |
        AttributeTargets.Method |
        AttributeTargets.Constructor)]
    public class InjectAttribute : Attribute
    {
        public bool IsOptional;
    }
}
using System;

namespace Silphid.Injexit
{
    [AttributeUsage(
        AttributeTargets.Field |
        AttributeTargets.Property |
        AttributeTargets.Method |
        AttributeTargets.Parameter |
        AttributeTargets.Constructor)]
    public class InjectAttribute : Attribute
    {
        public bool IsOptional;
        public string Id;
    }
}
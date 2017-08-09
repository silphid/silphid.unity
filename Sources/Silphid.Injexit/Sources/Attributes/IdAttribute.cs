using System;

namespace Silphid.Injexit
{
    [AttributeUsage(
        AttributeTargets.Field |
        AttributeTargets.Property |
        AttributeTargets.Parameter)]
    public class IdAttribute : Attribute
    {
        public readonly string Id;

        public IdAttribute(string id)
        {
            Id = id;
        }
    }
}
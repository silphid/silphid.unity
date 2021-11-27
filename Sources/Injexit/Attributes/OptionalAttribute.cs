using System;

namespace Silphid.Injexit
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
    public class OptionalAttribute : Attribute {}
}
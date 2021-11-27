using System;

namespace Silphid.Injexit
{
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor)]
    public class InjectAttribute : Attribute {}
}
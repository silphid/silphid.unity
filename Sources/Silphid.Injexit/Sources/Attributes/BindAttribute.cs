using System;

namespace Silphid.Injexit
{
    /// <summary>
    /// Specifies an extra binding to add when binding an instance with Container.BindInstance(),
    /// in addition to the instance's type, in order to support polymorphism.
    /// </summary>
    [Obsolete("Use System.Runtime.Serialization.KnownTypeAttribute instead.")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BindAttribute : Attribute
    {
        public readonly Type Type;

        public BindAttribute(Type type)
        {
            Type = type;
        }
    }
}
using System;

namespace Silphid.Showzup
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class IgnoreInManifestAttribute : Attribute {}
}
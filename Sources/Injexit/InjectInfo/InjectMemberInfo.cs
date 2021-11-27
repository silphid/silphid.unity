using System;

namespace Silphid.Injexit
{
    public class InjectMemberInfo
    {
        public string Name { get; }
        public Type Type { get; }
        public bool IsOptional { get; }
        public string CanonicalName { get; }

        public InjectMemberInfo(string name, Type type, bool isOptional)
        {
            Name = name;
            Type = type;
            IsOptional = isOptional;
            CanonicalName = Reflector.GetCanonicalName(name);
        }
    }
}
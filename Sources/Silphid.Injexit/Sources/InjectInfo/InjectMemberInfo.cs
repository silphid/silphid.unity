using System;

namespace Silphid.Injexit
{
    public class InjectMemberInfo
    {
        public string Name { get; }
        public Type Type { get; }
        public bool IsOptional { get; }
        public string Id { get; }

        public InjectMemberInfo(string name, Type type, bool isOptional, string id)
        {
            Name = name;
            Type = type;
            IsOptional = isOptional;
            Id = id;
        }
    }
}
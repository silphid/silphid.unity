using System;

namespace Silphid.Injexit
{
    public class InjectMemberInfo
    {
        public Type Type { get; }
        public bool IsOptional { get; }
        public string Id { get; }

        public InjectMemberInfo(Type type, bool isOptional, string id)
        {
            Type = type;
            IsOptional = isOptional;
            Id = id;
        }
    }
}
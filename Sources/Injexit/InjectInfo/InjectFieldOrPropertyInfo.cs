using System;

namespace Silphid.Injexit
{
    public abstract class InjectFieldOrPropertyInfo : InjectMemberInfo
    {
        public abstract void SetValue(object obj, object value);

        protected InjectFieldOrPropertyInfo(string name, Type type, bool isOptional)
            : base(name, type, isOptional) {}
    }
}
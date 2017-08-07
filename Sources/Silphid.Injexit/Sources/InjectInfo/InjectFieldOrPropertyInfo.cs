using System;

namespace Silphid.Injexit
{
    public abstract class InjectFieldOrPropertyInfo : InjectMemberInfo
    {
        public abstract void SetValue(object obj, object value);

        protected InjectFieldOrPropertyInfo(Type type, bool isOptional, string id) : base(type, isOptional, id)
        {
        }
    }
}
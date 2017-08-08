using System;

namespace Silphid.Injexit
{
    public abstract class InjectFieldOrPropertyInfo : InjectMemberInfo
    {
        public abstract void SetValue(object obj, object value);

        protected InjectFieldOrPropertyInfo(string name, Type type, bool isOptional, string id) : base(name, type, isOptional, id)
        {
        }
    }
}
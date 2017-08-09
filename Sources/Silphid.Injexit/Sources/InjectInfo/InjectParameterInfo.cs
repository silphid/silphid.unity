using System;
using System.Reflection;

namespace Silphid.Injexit
{
    public class InjectParameterInfo : InjectMemberInfo
    {
        public object DefaultValue { get; }
        
        public InjectParameterInfo(ParameterInfo parameterInfo, bool isOptional, string id) :
            base(parameterInfo.Name, parameterInfo.ParameterType, isOptional, id)
        {
            if (isOptional)
                DefaultValue = parameterInfo.HasDefaultValue
                    ? parameterInfo.DefaultValue
                    : parameterInfo.ParameterType.IsValueType
                        ? Activator.CreateInstance(parameterInfo.ParameterType)
                        : null;
        }
    }
}
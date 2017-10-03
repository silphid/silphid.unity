using System;
using System.Reflection;

namespace Silphid.Injexit
{
    public class InjectParameterInfo : InjectMemberInfo
    {
        public bool IsOptional { get; }
        public object DefaultValue { get; }
        
        public InjectParameterInfo(ParameterInfo parameterInfo, bool isOptional) :
            base(parameterInfo.Name, parameterInfo.ParameterType, isOptional)
        {
            IsOptional = isOptional;
            
            if (isOptional)
                DefaultValue = parameterInfo.HasDefaultValue
                    ? parameterInfo.DefaultValue
                    : parameterInfo.ParameterType.IsValueType
                        ? Activator.CreateInstance(parameterInfo.ParameterType)
                        : null;
        }
    }
}
using System.Reflection;

namespace Silphid.Injexit
{
    public class InjectParameterInfo : InjectMemberInfo
    {
        public InjectParameterInfo(ParameterInfo parameterInfo, bool isOptional, string id) :
            base(parameterInfo.Name, parameterInfo.ParameterType, isOptional, id)
        {
        }
    }
}
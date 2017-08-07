using System.Reflection;

namespace Silphid.Injexit
{
    public class InjectMethodInfo
    {
        public MethodInfo Method { get; }
        public InjectMemberInfo[] ParameterInfos { get; }

        public InjectMethodInfo(MethodInfo method, InjectMemberInfo[] parameterInfos)
        {
            Method = method;
            ParameterInfos = parameterInfos;
        }
    }
}
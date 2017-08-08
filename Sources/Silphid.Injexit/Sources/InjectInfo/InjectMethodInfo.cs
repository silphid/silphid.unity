using System.Reflection;

namespace Silphid.Injexit
{
    public class InjectMethodInfo
    {
        public string Name { get; }
        public MethodInfo Method { get; }
        public InjectParameterInfo[] Parameters { get; }

        public InjectMethodInfo(MethodInfo method, InjectParameterInfo[] parameters)
        {
            Name = method.Name;
            Method = method;
            Parameters = parameters;
        }
    }
}
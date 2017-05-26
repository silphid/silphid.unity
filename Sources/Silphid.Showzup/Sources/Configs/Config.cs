using System.Collections.Generic;
using System.Reflection;

namespace Silphid.Showzup
{
    public class Config
    {
        public List<Assembly> Assemblies { get; } = new List<Assembly>();
        public List<string> Namespaces { get; } = new List<string>();
        public List<Transition> Transitions { get; } = new List<Transition>();
        public List<Segue> Segues { get; } = new List<Segue>();
        public List<Mapping> Mappings { get; } = new List<Mapping>();
    }
}

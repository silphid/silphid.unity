using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public class CircularDependencyException : Exception
    {
        public Type[] Types { get; }

        public CircularDependencyException(Type parentType, CircularDependencyException exception) :
            this(exception.Types.Prepend(parentType).ToArray())
        {
        }
        
        public CircularDependencyException(Type[] types)
        {
            Types = types;
        }
        
        public CircularDependencyException(Type type)
        {
            Types = new[] { type };
        }

        public override string Message
        {
            get
            {
                // Keep only periodic part of dependency chain
                var types = Types;
                for (int i = 0; i < types.Length - 1; i++)
                {
                    var type1 = types[i];
                    for (int j = i + 1; j < types.Length; j++)
                    {
                        var type2 = types[j];
                        if (type1 == type2)
                        {
                            types = types.Skip(i).Take(j - i + 1).ToArray();
                            break;
                        }
                    }
                }
                
                var formattedTypes = types.Select(x => x.Name).ToDelimitedString(" > ");
                return $"Circular dependency detected: {formattedTypes}";
            }
        }
    }
}
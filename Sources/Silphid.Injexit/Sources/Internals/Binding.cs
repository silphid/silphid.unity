using System;

namespace Silphid.Injexit
{
    internal class Binding : IBinding
    {
        public static readonly IBinding Null = new NullBinding();
        
        public IContainer Container { get; }
        public Type AbstractionType { get; }
        public Type ConcretionType { get; }
        public Lifetime Lifetime { get; set; }
        public IResolver OverrideResolver { get; private set; }
        public object Instance { get; set; }
        public bool IsList { get; private set; }

        public Binding(IContainer container, Type abstractionType, Type concretionType)
        {
            Container = container;
            AbstractionType = abstractionType;
            ConcretionType = concretionType;
        }

        public IBinding IntoList()
        {
            if (IsList)
                throw new InvalidOperationException("Already a list binding.");
                
            IsList = true;
            return this;
        }

        public IBinding AsSingle()
        {
            if (Lifetime == Lifetime.Single)
                throw new InvalidOperationException("Already a single binding.");
                
            Lifetime = Lifetime.Single;
            return this;
        }

        public IBinding With(IResolver resolver)
        {
            if (OverrideResolver != null)
                throw new InvalidOperationException("Already specified binding overrides.");
                    
            OverrideResolver = resolver;
            return this;
        }

        public override string ToString()
        {
            var overrides = OverrideResolver != null ? " with overrides" : "";
            var list = IsList ? " into list" : "";
            var instance = Instance != null ? $" using instance {Instance}" : "";
            
            return $"{AbstractionType} => {ConcretionType} {Lifetime}{overrides}{list}{instance}";
        }
    }
}
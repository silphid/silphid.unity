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
        public bool InList { get; private set; }
        public string Name { get; private set; }
        public string Alias { get; private set; }
        public string Reference { get; set; }

        public Binding(IContainer container, Type abstractionType, Type concretionType)
        {
            Container = container;
            AbstractionType = abstractionType;
            ConcretionType = concretionType;
        }

        public Binding(IContainer container, Type abstractionType, string reference)
        {
            Container = container;
            AbstractionType = abstractionType;
            Reference = reference;
        }

        IBinding IBinding.InList()
        {
            if (InList)
                throw new InvalidOperationException("Already a list binding.");
                
            InList = true;
            return this;
        }

        public IBinding AsSingle()
        {
            if (Lifetime != Lifetime.Transient)
                throw new InvalidOperationException($"Lifetime already set to: {Lifetime}");
                
            Lifetime = Lifetime.Single;
            return this;
        }

        public IBinding AsEagerSingle()
        {
            if (Lifetime != Lifetime.Transient)
                throw new InvalidOperationException($"Lifetime already set to: {Lifetime}");

            Lifetime = Lifetime.EagerSingle;
            return this;
        }

        IBinding IBinding.Alias(string alias)
        {
            if (Alias != null)
                throw new InvalidOperationException("Already marked binding as Alias.");
                    
            Alias = alias;
            return this;
        }

        public IBinding Using(IResolver resolver)
        {
            if (OverrideResolver != null)
                throw new InvalidOperationException("Already specified binding overrides.");
                    
            OverrideResolver = resolver;
            return this;
        }

        IBinding IBinding.Named(string name)
        {
            if (Name != null)
                throw new InvalidOperationException("Already specified binding name.");
                    
            Name = Reflector.GetCanonicalName(name);
            return this;
        }

        public override string ToString()
        {
            var overrides = OverrideResolver != null ? " with overrides" : "";
            var alias = Alias != null ? $" alias {Alias}" : "";
            var list = InList ? " in list" : "";
            var instance = Instance != null ? $" using instance {Instance}" : "";
            
            return $"{AbstractionType} => {ConcretionType} {Lifetime}{overrides}{alias}{list}{instance}";
        }
    }
}
using System;

namespace Silphid.Injexit
{
    internal class NullBinding : IBinding
    {
        public IContainer Container => Injexit.Container.Null;
        public IBinding InList() => Binding.Null;
        public IBinding AsSingle() => Binding.Null;
        public IBinding AsEagerSingle() => Binding.Null;
        public IBinding Using(IResolver resolver) => Binding.Null;
        public IBinding UsingRecursively(IResolver resolver) => Binding.Null;
        public IBinding Named(string name) => Binding.Null;
        public IBinding Alias(Type aliasAbstractionType) => Binding.Null;

        public IBinding Id(BindingId id)
        {
            throw new NotSupportedException("Cannot associate an Id with NullBinding");
        }

        public BindingId Id()
        {
            throw new NotSupportedException("Cannot associate an Id with NullBinding");
        }
    }
}
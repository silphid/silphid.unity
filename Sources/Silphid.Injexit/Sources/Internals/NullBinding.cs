namespace Silphid.Injexit
{
    internal class NullBinding : IBinding
    {
        public IContainer Container => Injexit.Container.Null;
        public IBinding AsList() => Binding.Null;
        public IBinding AsSingle() => Binding.Null;
        public IBinding AsEagerSingle() => Binding.Null;
        public IBinding Using(IResolver resolver) => Binding.Null;
        public IBinding WithId(string id) => Binding.Null;
    }
}
namespace Silphid.Injexit
{
    internal class NullBinding : IBinding
    {
        public IContainer Container => Injexit.Container.Null;
        public IBinding IntoList() => Binding.Null;
        public IBinding AsSingle() => Binding.Null;
        public IBinding With(IResolver resolver) => Binding.Null;
    }
}
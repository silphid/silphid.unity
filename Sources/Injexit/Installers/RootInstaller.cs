namespace Silphid.Injexit
{
    public abstract class RootInstaller : Installer
    {
        protected override IContainer CreateContainer() =>
            new Container(new Reflector(), ShouldInject);

        protected virtual bool ShouldInject(object obj)
        {
            var ns = obj.GetType()
                        .Namespace;
            return ns == null || !ns.StartsWith("Unity") && !ns.StartsWith("TMPro");
        }
    }
}
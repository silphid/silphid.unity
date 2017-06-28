namespace Silphid.Injexit
{
    public interface IContainer : IBinder, IResolver, IInjector
    {
        IContainer CreateChild();
    }
}
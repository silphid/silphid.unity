namespace Silphid.Showzup.Injection
{
    public interface IContainer : IBinder, IResolver, IInjector
    {
        IContainer CreateChild();
    }
}
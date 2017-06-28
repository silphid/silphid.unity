namespace Silphid.Injexit
{
    public interface IInjector
    {
        void Inject(object obj, IResolver overrideResolver = null);
    }
}
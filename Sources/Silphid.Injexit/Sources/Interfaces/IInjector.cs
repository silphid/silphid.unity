namespace Silphid.Showzup.Injection
{
    public interface IInjector
    {
        void Inject(object obj, IResolver overrideResolver = null);
    }
}
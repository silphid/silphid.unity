namespace Silphid.Showzup.Injection
{
    public interface IBinding
    {
        IContainer Container { get; }
        IBinding InList();
        IBinding AsSingle();
        IBinding With(IResolver resolver);
    }
}
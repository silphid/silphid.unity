namespace Silphid.Showzup.Injection
{
    public interface IBinding
    {
        IContainer Container { get; }
        IBinding IntoList();
        IBinding AsSingle();
        IBinding With(IResolver resolver);
    }
}
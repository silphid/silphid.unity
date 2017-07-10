namespace Silphid.Injexit
{
    public interface IBinding
    {
        IContainer Container { get; }
        IBinding IntoList();
        IBinding AsSingle();
        IBinding Using(IResolver resolver);
        IBinding WithId(string id);
    }
}
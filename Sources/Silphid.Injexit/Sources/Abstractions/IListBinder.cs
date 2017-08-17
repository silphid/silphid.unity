namespace Silphid.Injexit
{
    public interface IListBinder<in TAbstraction>
    {
        IBinding Add<TConcretion>() where TConcretion : TAbstraction;
        IBinding AddInstance<TConcretion>(TConcretion instance) where TConcretion : TAbstraction;
    }
}
namespace Silphid.Injexit
{
    public interface IListBinder<TAbstraction>
    {
        IBinding<TAbstraction> Add<TConcretion>() where TConcretion : TAbstraction;
        IBinding<TAbstraction> AddInstance<TConcretion>(TConcretion instance) where TConcretion : TAbstraction;
        IBinding<TAbstraction> AddReference(BindingId id);
    }
}
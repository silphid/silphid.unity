namespace Silphid.Injexit
{
    public interface IListBinder<in TAbstraction>
    {
        IBinding Bind<TConcretion>() where TConcretion : TAbstraction;
        IBinding BindInstance<TConcretion>(TConcretion instance) where TConcretion : TAbstraction;
    }
}
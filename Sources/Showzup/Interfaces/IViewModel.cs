namespace Silphid.Showzup
{
    public interface IViewModel
    {
        object Model { get; }
    }

    public interface IViewModel<out TModel> : IViewModel
    {
        new TModel Model { get; }
    }
}
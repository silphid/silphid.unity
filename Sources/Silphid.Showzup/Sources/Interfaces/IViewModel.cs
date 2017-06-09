namespace Silphid.Showzup
{
    public interface IViewModel
    {
        object Model { get; }
    }

    public interface IViewModel<out T> : IViewModel
    {
        T Model { get; }
    }
}
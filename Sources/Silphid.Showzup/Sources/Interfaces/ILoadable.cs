using UniRx;

namespace Silphid.Showzup
{
    /// <summary>
    /// Allows views to initialize themselves after having been loaded and their view model set
    /// and right before being shown.
    /// </summary>
    public interface ILoadable
    {
        IObservable<Unit> Load();
    }
}
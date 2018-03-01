using UniRx;

namespace Silphid.Showzup
{
    public interface IConstructable
    {
        ICompletable Construct(Options options);
    }
}
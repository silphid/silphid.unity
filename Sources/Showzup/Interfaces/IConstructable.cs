using UniRx;

namespace Silphid.Showzup
{
    public interface IConstructable
    {
        ICompletable Construct(IOptions options);
    }
}
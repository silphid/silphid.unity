using UniRx;

namespace Silphid.Showzup
{
    public interface IDeconstructable
    {
        ICompletable Deconstruct(Options options);
    }
}
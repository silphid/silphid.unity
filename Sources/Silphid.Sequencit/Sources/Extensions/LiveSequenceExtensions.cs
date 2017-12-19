using UniRx;

namespace Silphid.Sequencit
{
    public static class LiveSequenceExtensions
    {
        public static void AddComplete(this LiveSequence This) =>
            This.AddAction(This.Complete);

        /// <summary>
        /// Tries to add given completable at the end of sequence, but if completable was already
        /// present in sequence, truncate everything after it instead.
        /// </summary>
        /// <returns>True if completable was added at the end of sequence, or False if completable
        /// was already present in sequence and everything after it was truncated instead.</returns>
        public static bool AddOrTruncateAfter(this LiveSequence This, ICompletable completable)
        {
            if (This.TruncateAfter(completable))
                return false;
            
            This.Add(completable);
            return true;
        }
    }
}
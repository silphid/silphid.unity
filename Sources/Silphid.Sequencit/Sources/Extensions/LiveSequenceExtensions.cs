using System;
using UniRx;

namespace Silphid.Sequencit
{
    public static class LiveSequenceExtensions
    {
        public static void AddComplete(this LiveSequence This) =>
            This.AddAction(This.Complete);

        /// <summary>
        /// Tries to add given observable at the end of sequence, but if observable was already
        /// present in sequence, truncate everything after it instead.
        /// </summary>
        /// <returns>True if observable was added at the end of sequence, or False if observable
        /// was already present in sequence and everything after it was truncated instead.</returns>
        public static bool AddOrTruncateAfter(this LiveSequence This, IObservable<Unit> observable)
        {
            if (This.TruncateAfter(observable))
                return false;
            
            This.Add(observable);
            return true;
        }
    }
}
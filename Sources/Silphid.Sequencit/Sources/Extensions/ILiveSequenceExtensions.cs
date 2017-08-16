namespace Silphid.Sequencit
{
    public static class ILiveSequenceExtensions
    {
        public static void AddComplete(this LiveSequence This) =>
            This.AddAction(This.Complete);

        /// <summary>
        /// Tries to add given marker at the end of sequence, but if marker was already
        /// present in sequence, truncate everything after it instead.
        /// </summary>
        /// <returns>True if marker was added at the end of sequence, or False if marker
        /// was already present in sequence and everything after it was truncated instead.</returns>
        public static bool AddMarkerOrTruncate(this LiveSequence This, Marker marker)
        {
            if (This.TruncateAfter(marker))
                return false;
            
            This.Add(marker);
            return true;
        }
    }
}
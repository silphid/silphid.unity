namespace Silphid.Showzup
{
    public enum PushMode
    {
        /// <summary>
        /// Add current view to history.
        /// </summary>
        Child,

        /// <summary>
        /// Leave history unchanged (replaces current view with new view)
        /// </summary>
        Sibling,

        /// <summary>
        /// Clear all history (new view becomes root)
        /// </summary>
        Root,

        /// <summary>
        /// Default mode: Child
        /// </summary>
        Default = Child
    }
}
namespace Silphid.Injexit
{
    internal enum Lifetime
    {
        /// <summary>
        /// A new object is created for each individual usage.
        /// </summary>
        Transient,

        /// <summary>
        /// A single instance is shared for all usages (and it is lazily created, if it was not specified at bind-time).
        /// </summary>
        Single
    }
}
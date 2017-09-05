namespace Silphid.Injexit
{
    internal enum Lifetime
    {
        /// <summary>
        /// A new object is created for each individual usage.
        /// </summary>
        Transient,

        /// <summary>
        /// A single instance is shared among all dependent objects, and that is lazily created, if it was not specified at bind-time.
        /// </summary>
        Single,

        /// <summary>
        /// A single instance is shared among all dependent objects, and that is eagerly created.
        /// </summary>
        EagerSingle
    }
}
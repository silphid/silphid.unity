namespace Silphid.Machina
{
	public class Change<TState>
    {
        public TState Source { get; }
        public TState Destination { get; }

        public Change(TState source, TState destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
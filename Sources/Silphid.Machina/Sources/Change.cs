namespace Silphid.Machina
{
	public class Change<TState> where TState : IState
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
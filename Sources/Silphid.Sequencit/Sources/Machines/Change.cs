namespace Silphid.Sequencit.Machines
{
	public class Change<TState> where TState : IState
    {
        public TState Source { get; private set; }
        public TState Destination { get; private set; }

        public Change(TState source, TState destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
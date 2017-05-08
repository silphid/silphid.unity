namespace Silphid.Sequencit.Machines
{
	public interface IState
	{
		IState BaseState { get; }
		string Name { get; }
	}
}
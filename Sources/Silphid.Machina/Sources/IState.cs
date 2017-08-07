namespace Silphid.Machina
{
	public interface IState
	{
		IState BaseState { get; }
		string Name { get; }
	}
}
namespace Silphid.Showzup
{
    public class ViewModel<T>
    {
        public T Model { get; }

        public ViewModel(T model)
        {
            Model = model;
        }
    }
}
namespace Silphid.Showzup
{
    public class ViewModel<T> : IViewModel<T>
    {
        public T Model { get; }
        object IViewModel.Model => Model;

        public ViewModel(T model)
        {
            Model = model;
        }
    }
}
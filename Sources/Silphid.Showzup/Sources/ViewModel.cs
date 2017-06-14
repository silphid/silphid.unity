namespace Silphid.Showzup
{
    public abstract class ViewModel<T> : IViewModel<T>
    {
        public T Model { get; }
        object IViewModel.Model => Model;

        protected ViewModel(T model)
        {
            Model = model;
        }
    }
}
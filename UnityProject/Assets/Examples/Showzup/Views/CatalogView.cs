using Silphid.Showzup;
using UniRx;

public class CatalogView : View<CatalogViewModel>, ILoadable
{
    public SelectionControl SelectionControl;
    
    public IObservable<Unit> Load() =>
        SelectionControl.Present(ViewModel.Photos).AsSingleUnitObservable();
}
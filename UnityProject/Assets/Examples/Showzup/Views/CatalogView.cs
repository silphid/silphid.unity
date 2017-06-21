using System;
using Silphid.Showzup;
using UniRx;

public class CatalogView : View<CatalogViewModel>, ILoadable
{
    public SelectionControl SelectionControl;

    public IObservable<Unit> Load()
    {
        return SelectionControl.Present(ViewModel.Photos).AsSingleUnitObservable();
    }
}
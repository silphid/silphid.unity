using System;
using Silphid.Extensions;
using Silphid.Showzup;
using UniRx;

public class CatalogView : View<CatalogViewModel>
{
    public SelectionControl SelectionControl;

    public override IObservable<Unit> Load()
    {
        SelectionControl
            .Present(ViewModel.Photos)
            .AsSingleUnitObservable()
            .SubscribeAndForget();

        return null;
    }
}
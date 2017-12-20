using Silphid.Extensions;
using Silphid.Showzup;
using UniRx;

public class CatalogView : View<CatalogViewModel>
{
    public SelectionControl SelectionControl;

    public override ICompletable Load()
    {
        SelectionControl
            .Present(ViewModel.Photos)
            .AsSingleUnitObservable()
            .SubscribeAndForget();

        return null;
    }
}
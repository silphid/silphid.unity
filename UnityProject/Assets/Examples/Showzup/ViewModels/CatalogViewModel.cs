using System.Collections.Generic;
using System.Linq;
using Silphid.Showzup;

public class CatalogViewModel : ViewModel<Catalog>
{
    public List<PhotoViewModel> Photos { get; }

    public CatalogViewModel(Catalog catalog) : base(catalog)
    {
        Photos = catalog.Photos
            .Select(x => new PhotoViewModel(x))
            .ToList();
    }
}
using System.Collections.Generic;
using Silphid.Showzup;

public class CatalogViewModel : ViewModel<Catalog>
{
    public List<Photo> Photos => Model.Photos;

    public CatalogViewModel(Catalog catalog) : base(catalog)
    {
    }
}
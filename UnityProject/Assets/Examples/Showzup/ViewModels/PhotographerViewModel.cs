using System;
using Silphid.Showzup;

public class PhotographerViewModel : ViewModel<Photographer>
{
    public string Name => Model.Name;
    public string Location => Model.Location;
    public string Website => Model.Website;
    public Uri ImageUri => new Uri($"res://Photographers/{Model.Id}.jpg");

    public PhotographerViewModel(Photographer photographer) : base(photographer)
    {
    }
}
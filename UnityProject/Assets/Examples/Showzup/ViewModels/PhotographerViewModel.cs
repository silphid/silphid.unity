using System;

public class PhotographerViewModel
{
    private readonly Photographer _photographer;

    public string Name => _photographer.Name;
    public string Location => _photographer.Location;
    public string Website => _photographer.Website;
    public Uri ImageUri => new Uri($"res://Photographers/{_photographer.Id}.jpg");

    public PhotographerViewModel(Photographer photographer)
    {
        _photographer = photographer;
    }
}
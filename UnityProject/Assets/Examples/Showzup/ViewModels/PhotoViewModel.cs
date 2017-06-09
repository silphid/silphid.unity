using System;

public class PhotoViewModel
{
    private readonly Photo _photo;
    private PhotographerViewModel _photographer;

    public Uri ImageUri =>
        new Uri($"res://Photos/{_photo.Photographer.Id}-{_photo.Id}.jpg");

    public PhotographerViewModel Photographer =>
        _photographer =
            _photographer ?? new PhotographerViewModel(_photo.Photographer);
    
    public PhotoViewModel(Photo photo)
    {
        _photo = photo;
    }
}
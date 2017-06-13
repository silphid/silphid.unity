using System;
using Silphid.Showzup;

public class PhotoViewModel : ViewModel<Photo>
{
    public Uri ImageUri =>
        new Uri($"res://Photos/{Model.Photographer.Id}-{Model.Id}.jpg");

    private PhotographerViewModel _photographer;
    public PhotographerViewModel Photographer =>
        _photographer = _photographer ?? new PhotographerViewModel(Model.Photographer);
    
    public PhotoViewModel(Photo photo) : base(photo)
    {
    }
}
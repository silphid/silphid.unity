using System;
using Silphid.Showzup;
using UniRx;
using UnityEngine.UI;

public class PhotoView : View<PhotoViewModel>, ILoadable
{
    public Image Image;
    public Text PhotographerName;
    
    public IObservable<Unit> Load()
    {
        Bind(Image, ViewModel.ImageUri);
        Bind(PhotographerName, ViewModel.Photographer.Name);
        return null;
    }
}
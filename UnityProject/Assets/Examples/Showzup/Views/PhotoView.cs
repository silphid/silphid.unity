using Silphid.Showzup;
using UniRx;
using UnityEngine.UI;

public class PhotoView : View<PhotoViewModel>
{
    public Image Image;
    public Text PhotographerName;
    
    public override ICompletable Load()
    {
        Binder.Bind(ViewModel.ImageUri, Image, true, 0.3f);
        Binder.Bind(ViewModel.Photographer.Name, PhotographerName);
        return null;
    }
}
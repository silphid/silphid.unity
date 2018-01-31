using Silphid.Showzup;
using UniRx;
using UnityEngine.UI;

public class PhotoView : View<PhotoViewModel>
{
    public Image Image;
    public Text PhotographerName;
    
    public override ICompletable Load()
    {
        Bind(Image, ViewModel.ImageUri, true, 0.3f);
        Bind(PhotographerName, ViewModel.Photographer.Name);
        return null;
    }
}
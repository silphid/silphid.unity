using Silphid.Showzup;
using UniRx;
using UnityEngine.UI;

public class HomePage : View<HomePageViewModel>, ILoadable
{
    public Text WelcomeMessageText;
    
    public IObservable<Unit> Load()
    {
        WelcomeMessageText.text = ViewModel.WelcomeMessage;
        return null;
    }
}
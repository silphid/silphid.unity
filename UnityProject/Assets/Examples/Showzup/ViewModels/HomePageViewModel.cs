using Silphid.Showzup;

[Variant(nameof(Granularity.Page))]
public class HomePageViewModel
{
    private readonly HomeModel _homeModel;

    public string WelcomeMessage => $"Welcome back, {_homeModel.UserName}!";

    public HomePageViewModel(HomeModel homeModel)
    {
        _homeModel = homeModel;
    }
}
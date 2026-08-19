using PasswordVale.Contracts;

namespace PasswordVale.Services;

public class NavigationService : INavigationService
{
    public event Action<AppPage>? OnNavigated;

    public void NavigateTo(AppPage page)
    {
        CurrentPage = page;
        OnNavigated?.Invoke(page);
    }

    public AppPage CurrentPage { get; private set; }
}


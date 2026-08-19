namespace PasswordVale.Contracts;

public interface INavigationService
{
    public event Action<AppPage>? OnNavigated;
    public void NavigateTo(AppPage page);
    public AppPage CurrentPage { get; }
}


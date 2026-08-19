using Microsoft.Extensions.DependencyInjection;

using PasswordVale.Contracts;

namespace PasswordVale.Views;

public class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _services;
    private readonly LoadingPage _loadingPage;

    public MainWindow(IServiceProvider services, INavigationService navigationService)
    {
        _services = services;
        _navigationService = navigationService;
        _navigationService.OnNavigated += NavigationChanged;

        Title = "Password Manager";
        Width = 800;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _loadingPage = new LoadingPage();

        // Immediately show the loading page.
        Content = _loadingPage;

        // Handle initial data fetch when window opens
        Opened += OnOpened;
    }

    private void NavigationChanged(AppPage page)
    {
        Content = page switch
        {
            AppPage.Setup => _services.GetRequiredService<SetupPage>(),
            AppPage.Unlock => _services.GetRequiredService<UnlockPage>(),
            AppPage.Vault => _services.GetRequiredService<VaultPage>(),
            _ => throw new InvalidOperationException("Invalid page navigation"),
        };
    }

    // Load initial application state from the database.
    private async void OnOpened(object? sender, EventArgs e)
    {
        try
        {
            await Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                // Decide which page to show based on database contents
            });

            // TODO: actually load the database information to determine the current
            // vault status.
            // Pretend the db is configured and just navigate to the unlock page.
            _navigationService.NavigateTo(AppPage.Unlock);
        }
        catch (Exception ex)
        {
            _loadingPage.UpdateStatus($"Startup Failed: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _navigationService.OnNavigated -= NavigationChanged;
    }
}

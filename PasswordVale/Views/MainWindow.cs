using Avalonia.Threading;

using Microsoft.Extensions.DependencyInjection;

using PasswordVale.Contracts;

namespace PasswordVale.Views;

public class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private readonly IDataService _dataService;
    private readonly IServiceProvider _services;
    private readonly LoadingPage _loadingPage;
    private readonly Vault _vault;

    public MainWindow(IServiceProvider services, INavigationService navigationService, IDataService dataService, Vault vault)
    {
        _services = services;
        _navigationService = navigationService;
        _dataService = dataService;
        _vault = vault;

        _vault.OnMasterPasswordChange += MasterPasswordStateChanged;
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
            _ => throw new InvalidOperationException($"App Page '{Enum.GetName(page)}' does not support navigation."),
        };
    }

    private void MasterPasswordStateChanged(MasterPassword? masterPasswordState)
    {
        if (masterPasswordState is null)
        {
            _navigationService.NavigateTo(AppPage.Setup);
            return;
        }

        _navigationService.NavigateTo(AppPage.Unlock);
    }

    // Load initial application state from the database.
    private async void OnOpened(object? sender, EventArgs e)
    {
        try
        {
            await Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                await Dispatcher.UIThread.InvokeAsync(async () => _vault.MasterPasswordRecord = await _dataService.GetMasterPassword());
            });
        }
        catch (Exception ex)
        {
            _loadingPage.UpdateStatus($"Startup Failed: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _navigationService.OnNavigated -= NavigationChanged;
        _vault.OnMasterPasswordChange -= MasterPasswordStateChanged;
    }
}

using Avalonia.Threading;

using Microsoft.Extensions.DependencyInjection;

using PasswordVale.Contracts;
using PasswordVale.Services;

namespace PasswordVale.Views;

public class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _services;
    private readonly LoadingPage _loadingPage;
    private readonly Vault _vault;

    public MainWindow(IServiceProvider services, INavigationService navigationService, Vault vault)
    {
        _services = services;
        _navigationService = navigationService;
        _vault = vault;

        _vault.OnStateChanged += VaultStateChanged;
        _navigationService.OnNavigated += NavigationChanged;

        Title = "Password Vale";
        Width = 800;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _loadingPage = new LoadingPage();

        // Immediately show the loading page.
        Content = _loadingPage;

        // Handle vault initialization when window opens
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

    private void VaultStateChanged(VaultState vaultState)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (vaultState)
            {
                case VaultState.NotConfigured:
                    _navigationService.NavigateTo(AppPage.Setup);
                    return;
                case VaultState.Locked:
                    _navigationService.NavigateTo(AppPage.Unlock);
                    return;
                case VaultState.Unlocked:
                    _navigationService.NavigateTo(AppPage.Vault);
                    return;
                default:
                    throw new InvalidOperationException($"Vault State '{Enum.GetName(vaultState)}' unhandled in Main Window");
            }
        });
    }

    // Initialize the vault.
    private async void OnOpened(object? sender, EventArgs e)
    {
        try
        {
            await Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                await _vault.Initialize();
            });
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(() => _loadingPage.UpdateStatus($"Startup Failed: {ex.Message}"));
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _navigationService.OnNavigated -= NavigationChanged;
        _vault.OnStateChanged -= VaultStateChanged;
    }
}

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

using Microsoft.Extensions.DependencyInjection;

using PasswordVale.Contracts;
using PasswordVale.Services;
using PasswordVale.Views;

namespace PasswordVale;

public class App : Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        // Add the standard Fluent theme directly in code
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 1. Build the DI container
            _serviceProvider = ConfigureServices();

            // 2. Resolve MainWindow directly with all dependencies wired
            desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            // 3. Clean disposal when application shuts down
            desktop.Exit += (s, e) =>
            {
                if (_serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register core services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDataService, FakeDataService>();
        services.AddSingleton<ICryptoService, CryptoService>();
        services.AddSingleton<Vault>();

        // Register Pages
        services.AddTransient<LoadingPage>();
        services.AddTransient<SetupPage>();
        services.AddTransient<UnlockPage>();
        services.AddTransient<VaultPage>();

        // Register MainWindow
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}

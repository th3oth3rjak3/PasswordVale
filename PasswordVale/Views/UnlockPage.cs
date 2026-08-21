using System.Text;

using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

using PasswordVale.Services;

namespace PasswordVale.Views;

public class UnlockPage : UserControl
{
    private readonly Vault _vault;
    private readonly TextBox _passwordInput;
    private readonly TextBlock _messageText;
    private readonly Button _submitButton;

    public UnlockPage(Vault vault)
    {
        _vault = vault;

        var titleText = new TextBlock
        {
            Text = "Unlock Vault",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var subtitleText = new TextBlock
        {
            Text = "Enter your master password to unlock the vault.",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };

        _passwordInput = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            PasswordChar = '•',
            RevealPassword = false,
            PlaceholderText = "Enter Master Password"
        };

        _messageText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center
        };

        _submitButton = new Button
        {
            Content = "Unlock Vault",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            IsEnabled = false
        };

        _passwordInput.TextChanged += (_, _) => ValidatePassword();
        _passwordInput.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter && _submitButton.IsEnabled)
            {
                SubmitButton_Click(this, new RoutedEventArgs());
            }
        };

        _submitButton.Click += SubmitButton_Click;

        var contentBox = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                titleText,
                subtitleText,
                _passwordInput,
                _messageText,
                _submitButton
            }
        };

        var cardContainer = new Border
        {
            Width = 340,
            Padding = new Thickness(24),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = contentBox
        };

        Content = new Grid
        {
            Children = { cardContainer }
        };
    }

    private async void SubmitButton_Click(object? sender, RoutedEventArgs e)
    {
        var masterPassword = _passwordInput.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(masterPassword))
        {
            ValidatePassword();
            return;
        }

        var shouldRevalidate = false;

        try
        {
            SetUiBusy(true, "Unlocking vault...");

            var masterPasswordBytes = Encoding.UTF8.GetBytes(masterPassword);
            await Task.Run(async () => await _vault.Unlock(masterPasswordBytes));
        }
        catch (Exception ex)
        {
            _messageText.Text = $"Login failed: {ex.Message}";
            _messageText.Foreground = Brushes.IndianRed;
            shouldRevalidate = true;
        }
        finally
        {
            SetUiBusy(false);
            if (shouldRevalidate)
            {
                ValidatePassword(clearMessageText: false);
            }
        }
    }

    private void ValidatePassword(bool clearMessageText = true)
    {
        var password = _passwordInput.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            _messageText.Text = "Enter a valid master password";
            _messageText.Foreground = Brushes.Orange;
            _submitButton.IsEnabled = false;
            return;
        }

        if (clearMessageText)
        {
            _messageText.Text = null;
        }

        _submitButton.IsEnabled = true;
    }

    private void SetUiBusy(bool isBusy, string? message = null)
    {
        _passwordInput.IsEnabled = !isBusy;
        _submitButton.IsEnabled = !isBusy;

        if (message is not null)
        {
            _messageText.Text = message;
            _messageText.Foreground = Brushes.Gray;
        }
    }
}

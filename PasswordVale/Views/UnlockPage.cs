using System.Text;

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

        _passwordInput = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = 240,
            PasswordChar = '•',
            RevealPassword = false,
            PlaceholderText = "Enter Master Password"
        };

        _messageText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Gray,
            FontSize = 14
        };

        _submitButton = new Button
        {
            Content = "Submit",
            Width = 240,
            HorizontalAlignment = HorizontalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            IsEnabled = false
        };

        _passwordInput.TextChanged += (_, _) => ValidatePassword();
        _submitButton.Click += SubmitButton_Click;


        var titleText = new TextBlock
        {
            Text = "Unlock Vault",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.White
        };

        var contentBox = new StackPanel
        {
            Spacing = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        contentBox.Children.Add(titleText);
        contentBox.Children.Add(_passwordInput);
        contentBox.Children.Add(_messageText);
        contentBox.Children.Add(_submitButton);

        var grid = new Grid
        {
            Background = Brushes.Black
        };

        grid.Children.Add(contentBox);
        Content = grid;
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
            SetUiBusy(true, "Unlocking Vault...");

            var masterPasswordBytes = Encoding.UTF8.GetBytes(masterPassword);
            await Task.Run(async () => await _vault.Unlock(masterPasswordBytes));

            // clear password fields after success
            _passwordInput.Text = string.Empty;
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
                ValidatePassword(false);
            }
        }
    }

    private void ValidatePassword(bool clearMessageText = true)
    {
        var password = _passwordInput.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            _messageText.Text = "Enter a valid password";
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

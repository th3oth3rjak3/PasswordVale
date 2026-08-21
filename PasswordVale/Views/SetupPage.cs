using System.Text;

using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

using PasswordVale.Services;

namespace PasswordVale.Views;

public class SetupPage : UserControl
{
    private readonly Vault _vault;
    private readonly TextBox _createPassword;
    private readonly TextBox _confirmPassword;
    private readonly TextBlock _messageText;
    private readonly Button _submitButton;

    public SetupPage(Vault vault)
    {
        _vault = vault;

        var titleText = new TextBlock
        {
            Text = "Create Master Password",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var subtitleText = new TextBlock
        {
            Text = "Your master password encrypts the entire vault.",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };

        _createPassword = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            PasswordChar = '•',
            RevealPassword = false,
            PlaceholderText = "New Master Password"
        };

        _confirmPassword = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            PasswordChar = '•',
            RevealPassword = false,
            PlaceholderText = "Confirm Master Password"
        };

        _messageText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Gray,
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center
        };

        _submitButton = new Button
        {
            Content = "Create Vault",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            IsEnabled = false
        };

        _createPassword.TextChanged += (_, _) => ValidatePasswords();
        _confirmPassword.TextChanged += (_, _) => ValidatePasswords();
        _submitButton.Click += SubmitButton_Click;

        var contentBox = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                titleText,
                subtitleText,
                _createPassword,
                _confirmPassword,
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

    private void ValidatePasswords()
    {
        var password1 = _createPassword.Text ?? string.Empty;
        var password2 = _confirmPassword.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(password1) || string.IsNullOrWhiteSpace(password2))
        {
            _messageText.Text = "Enter and confirm your master password.";
            _messageText.Foreground = Brushes.Gray;
            _submitButton.IsEnabled = false;
            return;
        }

        if (password1.Length < 8)
        {
            _messageText.Text = "Password must be at least 8 characters.";
            _messageText.Foreground = Brushes.Orange;
            _submitButton.IsEnabled = false;
            return;
        }

        if (password1 != password2)
        {
            _messageText.Text = "Passwords do not match.";
            _messageText.Foreground = Brushes.IndianRed;
            _submitButton.IsEnabled = false;
            return;
        }

        _messageText.Text = "Passwords match.";
        _messageText.Foreground = Brushes.LightGreen;
        _submitButton.IsEnabled = true;
    }

    private async void SubmitButton_Click(object? sender, RoutedEventArgs e)
    {
        var masterPassword = _createPassword.Text ?? string.Empty;
        var confirmPassword = _confirmPassword.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(masterPassword) || masterPassword != confirmPassword)
        {
            ValidatePasswords();
            return;
        }

        try
        {
            SetUiBusy(true, "Creating vault...");

            var masterPasswordBytes = Encoding.UTF8.GetBytes(masterPassword);
            await Task.Run(async () => await _vault.SetMasterPassword(masterPasswordBytes));
        }
        catch (Exception ex)
        {
            _messageText.Text = $"Setup failed: {ex.Message}";
            _messageText.Foreground = Brushes.IndianRed;
        }
        finally
        {
            SetUiBusy(false);
            ValidatePasswords();
        }
    }

    private void SetUiBusy(bool isBusy, string? message = null)
    {
        _createPassword.IsEnabled = !isBusy;
        _confirmPassword.IsEnabled = !isBusy;
        _submitButton.IsEnabled = !isBusy;

        if (message is not null)
        {
            _messageText.Text = message;
            _messageText.Foreground = Brushes.Gray;
        }
    }
}

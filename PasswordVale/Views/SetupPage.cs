using System.Text;

using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

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

        _createPassword = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = 240,
            PasswordChar = '•',
            RevealPassword = false,
            PlaceholderText = "New Master Password"
        };

        _confirmPassword = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = 240,
            PasswordChar = '•',
            RevealPassword = false,
            PlaceholderText = "Confirm Master Password"
        };

        _messageText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Gray,
            FontSize = 14
        };

        _submitButton = new Button
        {
            Content = "Create Vault",
            Width = 240,
            HorizontalAlignment = HorizontalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            IsEnabled = false
        };

        _createPassword.TextChanged += (_, _) => ValidatePasswords();
        _confirmPassword.TextChanged += (_, _) => ValidatePasswords();
        _submitButton.Click += SubmitButton_Click;

        var titleText = new TextBlock
        {
            Text = "Create Master Password",
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
        contentBox.Children.Add(_createPassword);
        contentBox.Children.Add(_confirmPassword);
        contentBox.Children.Add(_messageText);
        contentBox.Children.Add(_submitButton);

        var grid = new Grid
        {
            Background = Brushes.Black
        };

        grid.Children.Add(contentBox);
        Content = grid;
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
            await Dispatcher.UIThread.InvokeAsync(async () => await _vault.SetMasterPassword(masterPasswordBytes));

            _messageText.Text = "Vault setup complete.";
            _messageText.Foreground = Brushes.LightGreen;

            // clear password fields after success
            _createPassword.Text = string.Empty;
            _confirmPassword.Text = string.Empty;
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
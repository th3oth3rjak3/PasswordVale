using Avalonia.Layout;
using Avalonia.Media;

namespace PasswordVale.Views;

public class LoadingPage : UserControl
{
    private readonly TextBlock _statusLabel;
    private readonly ProgressBar _progressBar;

    public LoadingPage(string initialMessage = "Loading...")
    {
        _progressBar = new ProgressBar
        {
            IsIndeterminate = true,
            Width = 240,
            Height = 4,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        _statusLabel = new TextBlock
        {
            Text = initialMessage,
            FontSize = 14,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var contentBox = new StackPanel
        {
            Spacing = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        contentBox.Children.Add(_progressBar);
        contentBox.Children.Add(_statusLabel);

        Content = new Grid
        {
            Children = { contentBox }
        };
    }

    public void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
    }
}

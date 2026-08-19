using Avalonia.Layout;
using Avalonia.Media;

namespace PasswordVale.Views;

public class LoadingPage : UserControl
{
    private readonly TextBlock _statusLabel;
    private readonly ProgressBar _progressBar;

    public LoadingPage(string initialMessage = "Loading...")
    {
        _statusLabel = new TextBlock
        {
            Text = initialMessage,
            FontSize = 16,
            FontWeight = FontWeight.Medium,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Gray
        };

        // Indeterminate Progress Bar
        _progressBar = new ProgressBar
        {
            IsIndeterminate = true,
            Width = 240,
            Height = 6,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // Inner vertical container
        var contentBox = new StackPanel
        {
            Spacing = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        contentBox.Children.Add(_progressBar);
        contentBox.Children.Add(_statusLabel);

        // Root Grid expands to 100% of parent area
        var rootGrid = new Grid
        {
            Background = Brushes.Black
        };
        rootGrid.Children.Add(contentBox);

        Content = rootGrid;
    }

    // Public method to update status text as loading steps finish
    public void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
    }
}

using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace PasswordVale.Views;

public class VaultPage : UserControl
{
    private readonly TextBox _searchBox;
    private readonly ListBox _entryListBox;
    private List<PasswordEntrySummary> _allEntries = [];

    public event Action<PasswordEntrySummary>? EntrySelected;
    public event Action? NewEntryRequested;
    public event Action? LockRequested;

    public VaultPage()
    {
        // 1. Clearable Search Box
        var clearBtn = new Button
        {
            Content = "✕",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(6, 0),
            Cursor = new Cursor(StandardCursorType.Hand),
            IsVisible = false
        };

        _searchBox = new TextBox
        {
            PlaceholderText = "Search vault entries...",
            InnerRightContent = clearBtn,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        _searchBox.TextChanged += (s, e) =>
        {
            clearBtn.IsVisible = !string.IsNullOrEmpty(_searchBox.Text);
            FilterEntries();
        };

        clearBtn.Click += (s, e) =>
        {
            _searchBox.Text = string.Empty;
            _searchBox.Focus();
        };

        // 2. Action Buttons (New Entry + Lock)
        var newBtn = new Button { Content = "+ New", Margin = new Thickness(6, 0, 0, 0) };
        var lockBtn = new Button { Content = "🔒 Lock", Margin = new Thickness(6, 0, 0, 0) };

        newBtn.Click += (s, e) => NewEntryRequested?.Invoke();
        lockBtn.Click += (s, e) => LockRequested?.Invoke();

        // 3. Top Header Bar (Search takes remaining width, buttons docked right)
        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
            Margin = new Thickness(12, 12, 12, 8)
        };

        Grid.SetColumn(_searchBox, 0);
        Grid.SetColumn(newBtn, 1);
        Grid.SetColumn(lockBtn, 2);

        headerGrid.Children.Add(_searchBox);
        headerGrid.Children.Add(newBtn);
        headerGrid.Children.Add(lockBtn);

        // 4. ListBox with Clean Row Template
        _entryListBox = new ListBox
        {
            Margin = new Thickness(12, 0, 12, 12),
            Background = Brushes.Transparent,
            ItemTemplate = new FuncDataTemplate<PasswordEntrySummary>((item, _) => BuildEntryCard(item))
        };

        _entryListBox.SelectionChanged += (s, e) =>
        {
            if (_entryListBox.SelectedItem is PasswordEntrySummary selected)
            {
                EntrySelected?.Invoke(selected);
            }
        };

        // 5. Root Layout (Header on top, List stretches below)
        var rootGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*")
        };

        Grid.SetRow(headerGrid, 0);
        Grid.SetRow(_entryListBox, 1);

        rootGrid.Children.Add(headerGrid);
        rootGrid.Children.Add(_entryListBox);

        Content = rootGrid;

        LoadMockData();
    }

    private static Control BuildEntryCard(PasswordEntrySummary? item)
    {
        if (item == null)
            return new Panel();

        var nameText = new TextBlock { Text = item.Name, FontWeight = FontWeight.Bold, FontSize = 14 };
        var userText = new TextBlock { Text = item.Username ?? "No username", Foreground = Brushes.Gray, FontSize = 12 };

        var textStack = new StackPanel { Spacing = 2, Children = { nameText, userText } };

        var star = new TextBlock
        {
            Text = item.Favorite ? "★" : "",
            Foreground = Brushes.Gold,
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center
        };

        var rowGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(4, 6)
        };

        Grid.SetColumn(textStack, 0);
        Grid.SetColumn(star, 1);

        rowGrid.Children.Add(textStack);
        rowGrid.Children.Add(star);

        return rowGrid;
    }

    private void FilterEntries()
    {
        var query = _searchBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(query))
        {
            _entryListBox.ItemsSource = _allEntries;
        }
        else
        {
            _entryListBox.ItemsSource = _allEntries
                .Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (x.Username != null && x.Username.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }

    private void LoadMockData()
    {
        _allEntries =
        [
            new() { Id = Guid.NewGuid(), Name = "GitHub", Username = "octocat@github.com", Favorite = true },
            new() { Id = Guid.NewGuid(), Name = "AWS Console", Username = "admin@company.com", Favorite = true },
            new() { Id = Guid.NewGuid(), Name = "Navy Federal", Username = "j_hathaway", Favorite = false },
            new() { Id = Guid.NewGuid(), Name = "ProtonMail", Username = "user.secure@pm.me", Favorite = false }
        ];

        _entryListBox.ItemsSource = _allEntries;
    }
}

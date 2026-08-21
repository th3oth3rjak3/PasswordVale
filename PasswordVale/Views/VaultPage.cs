using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace PasswordVale.Views;

public class VaultPage : UserControl
{
    private readonly ListBox _entryListBox;
    private readonly TextBox _searchBox;
    private List<PasswordEntrySummary> _allEntries = [];

    public event Action<PasswordEntrySummary>? EntrySelected;

    public VaultPage()
    {
        var sidebar = BuildSidebar();

        var clearButton = new Button
        {
            Content = "✕",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(6, 0),
            Cursor = new Cursor(StandardCursorType.Hand),
            IsVisible = false // Hidden initially when text is empty
        };

        _searchBox = new TextBox
        {
            PlaceholderText = "Search vault entries...",
            Margin = new Thickness(12, 12, 12, 6),
            InnerRightContent = clearButton,
        };

        _searchBox.TextChanged += (s, e) =>
        {
            clearButton.IsVisible = !string.IsNullOrEmpty(_searchBox.Text);
            FilterEntries();
        };

        clearButton.Click += (s, e) =>
        {
            _searchBox.Text = string.Empty;
            _searchBox.Focus();
        };

        _entryListBox = new ListBox
        {
            Margin = new Thickness(12, 0, 12, 12),
            Background = Brushes.Transparent,
            ItemTemplate = new FuncDataTemplate<PasswordEntrySummary>((item, _) =>
                BuildEntryCard(item))
        };

        _entryListBox.SelectionChanged += (s, e) =>
        {
            if (_entryListBox.SelectedItem is PasswordEntrySummary selected)
            {
                EntrySelected?.Invoke(selected);
            }
        };

        var mainContentGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*")
        };

        Grid.SetRow(_searchBox, 0);
        Grid.SetRow(_entryListBox, 1);

        mainContentGrid.Children.Add(_searchBox);
        mainContentGrid.Children.Add(_entryListBox);

        var rootGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("200,*")
        };

        Grid.SetColumn(sidebar, 0);
        Grid.SetColumn(mainContentGrid, 1);

        rootGrid.Children.Add(sidebar);
        rootGrid.Children.Add(mainContentGrid);

        Content = rootGrid;

        LoadMockData();
    }

    private Control BuildSidebar()
    {
        var newBtn = new Button
        {
            Content = "+ New Entry",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12)
        };

        var allItemsBtn = new Button { Content = "All Items", HorizontalAlignment = HorizontalAlignment.Stretch };
        var favoritesBtn = new Button { Content = "Favorites", HorizontalAlignment = HorizontalAlignment.Stretch };

        allItemsBtn.Click += (s, e) =>
        {
            FilterEntries("all");
        };

        favoritesBtn.Click += (s, e) =>
        {
            FilterEntries("favorites");
        };

        var sidePanel = new StackPanel
        {
            Spacing = 4,
            Margin = new Thickness(10)
        };

        sidePanel.Children.Add(newBtn);
        sidePanel.Children.Add(allItemsBtn);
        sidePanel.Children.Add(favoritesBtn);

        return new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = sidePanel
        };
    }

    private static Control BuildEntryCard(PasswordEntrySummary? item)
    {
        if (item == null)
            return new Panel();

        var nameText = new TextBlock
        {
            Text = item.Name,
            FontWeight = FontWeight.Bold,
            FontSize = 14
        };

        var usernameText = new TextBlock
        {
            Text = item.Username ?? "No username",
            Foreground = Brushes.Gray,
            FontSize = 12
        };

        var textStack = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center
        };
        textStack.Children.Add(nameText);
        textStack.Children.Add(usernameText);

        var rightStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (item.Favorite)
        {
            rightStack.Children.Add(new TextBlock
            {
                Text = "★",
                Foreground = Brushes.Gold,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        rightStack.Children.Add(BuildTagDisplay(item.Tags, maxVisible: 2));

        var rowGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(4, 6)
        };

        Grid.SetColumn(textStack, 0);
        Grid.SetColumn(rightStack, 1);

        rowGrid.Children.Add(textStack);
        rowGrid.Children.Add(rightStack);

        return rowGrid;
    }

    private static Control BuildTagDisplay(List<string> tags, int maxVisible = 2)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (tags == null || tags.Count == 0)
            return row;

        foreach (var tag in tags.Take(maxVisible))
        {
            row.Children.Add(CreateTagBadge(tag));
        }

        int overflowCount = tags.Count - maxVisible;
        if (overflowCount > 0)
        {
            var overflowTags = tags.Skip(maxVisible).ToList();

            var moreBtn = new Button
            {
                Content = $"+{overflowCount}",
                FontSize = 11,
                Padding = new Thickness(6, 2)
            };

            moreBtn.Flyout = CreateTagFlyout(overflowTags);
            row.Children.Add(moreBtn);
        }

        return row;
    }

    private static Flyout CreateTagFlyout(List<string> tags)
    {
        var wrapPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            ItemWidth = double.NaN,
            Margin = new Thickness(4)
        };

        foreach (var tag in tags)
        {
            var badge = CreateTagBadge(tag);
            badge.Margin = new Thickness(3);
            wrapPanel.Children.Add(badge);
        }

        var scrollViewer = new ScrollViewer
        {
            MaxHeight = 220,
            MaxWidth = 280,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = wrapPanel
        };

        return new Flyout
        {
            Content = scrollViewer,
            Placement = PlacementMode.BottomEdgeAlignedRight,
            ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway,
        };
    }

    private static Border CreateTagBadge(string tag)
    {
        return new Border
        {
            Background = Brushes.SteelBlue,
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 2),
            Child = new TextBlock
            {
                Text = tag,
                FontSize = 11,
                Foreground = Brushes.White
            }
        };
    }

    private void FilterEntries(string? tag = null)
    {
        var query = _searchBox.Text?.Trim() ?? string.Empty;

        var queryable = _allEntries.AsQueryable();

        if (string.IsNullOrEmpty(tag) || tag.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            // No-op, it's already all of the entries.
        }
        else if (tag == "favorites")
        {
            queryable = queryable.Where(entry => entry.Favorite);
        }
        else
        {
            queryable = queryable.Where(entry => entry.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(query))
        {
            queryable = queryable
                .Where(x =>
                    x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || (x.Username != null && x.Username.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        _entryListBox.ItemsSource = queryable.ToList();
    }

    private void LoadMockData()
    {
        _allEntries = new List<PasswordEntrySummary>
        {
            new() { Id = Guid.NewGuid(), Name = "GitHub", Username = "octocat@github.com", Favorite = true, Tags = new() { "Dev", "Git", "Work", "2FA" } },
            new() { Id = Guid.NewGuid(), Name = "AWS Console", Username = "admin@company.com", Favorite = true, Tags = new() { "Cloud", "Infra" } },
            new() { Id = Guid.NewGuid(), Name = "Navy Federal", Username = "j_hathaway", Favorite = false, Tags = new() { "Banking" } },
            new() { Id = Guid.NewGuid(), Name = "ProtonMail", Username = "user.secure@pm.me", Favorite = false, Tags = new() { "Email" } }
        };

        _entryListBox.ItemsSource = _allEntries;
    }
}

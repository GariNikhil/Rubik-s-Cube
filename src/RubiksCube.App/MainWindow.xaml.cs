using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Web.WebView2.Core;
using RubiksCube.Core;
using Windows.Storage;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace RubiksCube.App;

public sealed partial class MainWindow : Window
{
    private CubeState _state = new();
    private readonly List<CubeMove> _history = [];
    private readonly ScrambleGenerator _scrambleGenerator = new();
    private static readonly string ThemeSettingsKey = "AppTheme";
    private static readonly string RendererHtmlPath = global::System.IO.Path.GetFullPath(
        global::System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "src", "RubiksCube.Renderer", "index.html"));
    private bool _webViewReady = false;

    private readonly Grid _rootGrid;
    private TextBlock _statusText = null!;
    private TextBlock _moveHistoryText = null!;
    private WebView2 _cubeWebView = null!;
    private Canvas _patternCanvas = null!;
    private ContentDialog? _settingsDialog;
    private RadioButton? _lightThemeRadio;
    private RadioButton? _darkThemeRadio;
    private Border _leftBorder = null!;
    private Border _rightBorder = null!;
    private ElementTheme _theme = ElementTheme.Default;

    private static readonly Dictionary<string, (string PageBg, string PanelBg, string PanelBorder, string Accent, string MutedText)> Themes = new()
    {
        ["Light"] = ("#F5F7F8", "#E8EAEB", "#B9C0C1", "#009E73", "#5A7072"),
        ["Dark"] = ("#102629", "#183438", "#31575B", "#00F5A0", "#A4C1C2"),
    };

    public MainWindow()
    {
        _rootGrid = BuildRootGrid();
        Content = _rootGrid;

        LoadSavedTheme();
        DrawPatternMap();
        UpdateStatus("Solved state");
        _ = InitializeWebViewAsync();
    }

#region UI Construction

private Grid BuildRootGrid()
{
    var grid = new Grid
    {
        Background = GetBrush("PageBackgroundBrush"),
    };
    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

    var headerStack = new StackPanel
    {
        Margin = new Thickness(36, 30, 36, 22),
    };
    var title1 = new TextBlock
    {
        Text = "MATH THINKING FOR SMART",
        Foreground = GetBrush("AccentBrush"),
        FontSize = 16,
        CharacterSpacing = 180,
    };
    var title2 = new TextBlock
    {
        Text = "Rubik's Cube Solving",
        Foreground = new SolidColorBrush(ColorFromArgb(255, 255, 255, 255)),
        FontSize = 40,
    };
    var subtitle = new TextBlock
    {
        Text = "Explore patterns, permutations, and the logic behind every turn.",
        Foreground = GetBrush("MutedTextBrush"),
        FontSize = 16,
        Margin = new Thickness(0, 8, 0, 0),
    };
    headerStack.Children.Add(title1);
    headerStack.Children.Add(title2);
    headerStack.Children.Add(subtitle);
    Grid.SetRow(headerStack, 0);
    grid.Children.Add(headerStack);

    var mainGrid = new Grid { Margin = new Thickness(36, 0, 36, 24) };
    mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
    mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
    mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

    _leftBorder = new Border
    {
        Background = GetBrush("PanelBackgroundBrush"),
        BorderBrush = GetBrush("PanelBorderBrush"),
        BorderThickness = new Thickness(1, 1, 1, 1),
        CornerRadius = new CornerRadius(20),
        Padding = new Thickness(24, 24, 24, 24),
        Child = BuildPatternMapPanel(),
    };
    Grid.SetColumn(_leftBorder, 0);
    mainGrid.Children.Add(_leftBorder);

    _rightBorder = new Border
    {
        Background = new SolidColorBrush(ColorFromArgb(255, 0x0B, 0x17, 0x19)),
        BorderBrush = GetBrush("PanelBorderBrush"),
        BorderThickness = new Thickness(1, 1, 1, 1),
        CornerRadius = new CornerRadius(20),
        Padding = new Thickness(24, 24, 24, 24),
        Child = BuildCubePlaygroundPanel(),
    };
    Grid.SetColumn(_rightBorder, 2);
    mainGrid.Children.Add(_rightBorder);

    Grid.SetRow(mainGrid, 1);
    grid.Children.Add(mainGrid);

    var controlsBorder = new Border
    {
        Background = new SolidColorBrush(ColorFromArgb(255, 0x0B, 0x17, 0x19)),
        Padding = new Thickness(36, 18, 36, 18),
        Child = BuildControlsPanel(),
    };
    Grid.SetRow(controlsBorder, 2);
    grid.Children.Add(controlsBorder);

    return grid;
}

private Grid BuildPatternMapPanel()
{
    var grid = new Grid();
    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

    var headerStack = new StackPanel();
    var patternTitle = new TextBlock
    {
        Text = "Pattern map",
        Foreground = new SolidColorBrush(ColorFromArgb(255, 255, 255, 255)),
        FontSize = 22,
    };
    var patternSubtitle = new TextBlock
    {
        Text = "A visual language for the cube's six color families.",
        Foreground = GetBrush("MutedTextBrush"),
        FontSize = 16,
        Margin = new Thickness(0, 5, 0, 0),
    };
    headerStack.Children.Add(patternTitle);
    headerStack.Children.Add(patternSubtitle);
    Grid.SetRow(headerStack, 0);
    grid.Children.Add(headerStack);

    _patternCanvas = new Canvas { Margin = new Thickness(0, 24, 0, 0) };
    Grid.SetRow(_patternCanvas, 1);
    grid.Children.Add(_patternCanvas);

    return grid;
#endregion
}

private Grid BuildCubePlaygroundPanel()
{
    var grid = new Grid();
    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

    var headerGrid = new Grid();
    headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

    var titleStack = new StackPanel();
    var cubeTitle = new TextBlock
    {
        Text = "Cube playground",
        Foreground = new SolidColorBrush(ColorFromArgb(255, 255, 255, 255)),
        FontSize = 22,
    };
    _statusText = new TextBlock
    {
        Text = "Solved state",
        Foreground = GetBrush("MutedTextBrush"),
        FontSize = 16,
        Margin = new Thickness(0, 5, 0, 0),
    };
    titleStack.Children.Add(cubeTitle);
    titleStack.Children.Add(_statusText);
    Grid.SetColumn(titleStack, 0);

    var resetButton = new Button
    {
        Content = "\u21BB",
        FontSize = 22,
    };
    resetButton.Click += ResetButton_Click;
    Grid.SetColumn(resetButton, 1);

    headerGrid.Children.Add(titleStack);
    headerGrid.Children.Add(resetButton);
    Grid.SetRow(headerGrid, 0);
    grid.Children.Add(headerGrid);

    var viewbox = new Viewbox { Margin = new Thickness(20) };
    _cubeWebView = new WebView2
    {
        Width = 320,
        Height = 280,
    };
    viewbox.Child = _cubeWebView;
    Grid.SetRow(viewbox, 1);
    grid.Children.Add(viewbox);

    return grid;
}

private Grid BuildControlsPanel()
{
    var grid = new Grid();
    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

    var buttonStack = new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Spacing = 10,
    };

    var scrambleButton = new Button { Content = "Scramble" };
    scrambleButton.Click += ScrambleButton_Click;
    var scrambleAccel = new KeyboardAccelerator { Key = Windows.System.VirtualKey.R, Modifiers = Windows.System.VirtualKeyModifiers.Control };
    scrambleButton.KeyboardAccelerators.Add(scrambleAccel);
    buttonStack.Children.Add(scrambleButton);

    var undoAllButton = new Button { Content = "Undo all" };
    undoAllButton.Click += UndoAllButton_Click;
    var undoAllAccel = new KeyboardAccelerator { Key = Windows.System.VirtualKey.Z, Modifiers = Windows.System.VirtualKeyModifiers.Control | Windows.System.VirtualKeyModifiers.Shift };
    undoAllButton.KeyboardAccelerators.Add(undoAllAccel);
    buttonStack.Children.Add(undoAllButton);

    var undoButton = new Button { Content = "Undo" };
    undoButton.Click += UndoButton_Click;
    var undoAccel = new KeyboardAccelerator { Key = Windows.System.VirtualKey.Z, Modifiers = Windows.System.VirtualKeyModifiers.Control };
    undoButton.KeyboardAccelerators.Add(undoAccel);
    buttonStack.Children.Add(undoButton);

    var settingsButton = new Button { Content = "Settings" };
    settingsButton.Click += SettingsButton_Click;
    buttonStack.Children.Add(settingsButton);

    Grid.SetColumn(buttonStack, 0);
    grid.Children.Add(buttonStack);

    _moveHistoryText = new TextBlock
    {
        Text = "Moves: \u2014",
        Foreground = GetBrush("MutedTextBrush"),
        VerticalAlignment = VerticalAlignment.Center,
    };
    Grid.SetColumn(_moveHistoryText, 1);
    grid.Children.Add(_moveHistoryText);

    return grid;
}

#region Event Handlers

private void ScrambleButton_Click(object sender, RoutedEventArgs e)
{
    var scramble = _scrambleGenerator.Generate();
    _state = _state.Apply(scramble);
    _history.AddRange(scramble);
    PostStateToRenderer();
    foreach (var move in scramble)
        PostMoveToRenderer(move.ToString());
    UpdateStatus($"Scrambled with {scramble.Count} moves");
}

private void UndoAllButton_Click(object sender, RoutedEventArgs e)
{
    var inverse = _history.AsEnumerable().Reverse().Select(move => move.Inverse()).ToArray();
    _state = _state.Apply(inverse);
    _history.Clear();
    PostStateToRenderer();
    foreach (var move in inverse)
        PostMoveToRenderer(move.ToString());
    UpdateStatus(_state.IsSolved() ? "Solved state" : "State updated");
}

private void UndoButton_Click(object sender, RoutedEventArgs e)
{
    if (_history.Count == 0) return;
    var move = _history[^1];
    _state = _state.Apply(move.Inverse());
    _history.RemoveAt(_history.Count - 1);
    UpdateStatus(_state.IsSolved() ? "Solved state" : "Move undone");
    PostMoveToRenderer(move.ToString());
}

private void ResetButton_Click(object sender, RoutedEventArgs e)
{
    _state = new CubeState();
    _history.Clear();
    PostMessageToRenderer(new { type = "reset" });
    PostStateToRenderer();
    UpdateStatus("Solved state");
}

private void UpdateStatus(string status)
{
    _statusText.Text = status;
    _moveHistoryText.Text = _history.Count == 0 ? "Moves: \u2014" : $"Moves: {string.Join(' ', _history)}";
    PostStateToRenderer();
}

private void DrawPatternMap()
{
    var colors = new[] { "#FFFFFF", "#F3D21B", "#18D64B", "#3987FF", "#C72E4A", "#FF8A1E" };
    for (var index = 0; index < colors.Length; index++)
    {
        var angle = Math.PI * 2 * index / colors.Length;
        var x = 180 + Math.Cos(angle) * 115;
        var y = 130 + Math.Sin(angle) * 85;
        var color = ParseColor(colors[index]);
        var ellipse = new Ellipse
        {
            Width = 28,
            Height = 28,
            Fill = new SolidColorBrush(color),
        };
        Canvas.SetLeft(ellipse, x - 14);
        Canvas.SetTop(ellipse, y - 14);
        _patternCanvas.Children.Add(ellipse);
    }
}

#endregion

#region Theme

private void LoadSavedTheme()
{
    var localSettings = ApplicationData.Current.LocalSettings;
    if (localSettings.Values.TryGetValue(ThemeSettingsKey, out var themeValue))
    {
        var themeName = (string?)themeValue;
        if (themeName == "Light")
        {
            _theme = ElementTheme.Light;
            if (_lightThemeRadio != null) _lightThemeRadio.IsChecked = true;
        }
        else
        {
            _theme = ElementTheme.Dark;
            if (_darkThemeRadio != null) _darkThemeRadio.IsChecked = true;
        }
    }
    else
    {
        _theme = ElementTheme.Dark;
        if (_darkThemeRadio != null) _darkThemeRadio.IsChecked = true;
    }
    ApplyThemeColors(_theme);
}

private void SaveTheme(string themeName)
{
    var localSettings = ApplicationData.Current.LocalSettings;
    localSettings.Values[ThemeSettingsKey] = themeName;
}

private void ApplyThemeColors(ElementTheme theme)
{
    var themeName = theme == ElementTheme.Light ? "Light" : "Dark";
    var colors = Themes[themeName];

    _rootGrid.Background = GetBrushFromHex(colors.PageBg);
    _leftBorder.Background = GetBrushFromHex(colors.PanelBg);
    _leftBorder.BorderBrush = GetBrushFromHex(colors.PanelBorder);
    _rightBorder.Background = GetBrushFromHex(colors.PanelBg);
    _rightBorder.BorderBrush = GetBrushFromHex(colors.PanelBorder);
}

private void SettingsButton_Click(object sender, RoutedEventArgs e)
{
    if (_settingsDialog == null)
    {
        _settingsDialog = BuildSettingsDialog();
    }
    _settingsDialog.XamlRoot = this.Content.XamlRoot;
    _ = _settingsDialog.ShowAsync();
}

private ContentDialog BuildSettingsDialog()
{
    var dialog = new ContentDialog
    {
        Title = "Settings",
        CloseButtonText = "Close",
        DefaultButton = ContentDialogButton.Close,
    };

    var stack = new StackPanel { Spacing = 12, Width = 320 };

    var themeLabel = new TextBlock
    {
        Text = "Theme",
        Foreground = GetBrush("MutedTextBrush"),
    };
    stack.Children.Add(themeLabel);

    var radioButtons = new RadioButtons();
    _lightThemeRadio = new RadioButton { Content = "Light" };
    _darkThemeRadio = new RadioButton { Content = "Dark" };
    radioButtons.SelectionChanged += ThemeRadioButtons_SelectionChanged;
    radioButtons.Items.Add(_lightThemeRadio);
    radioButtons.Items.Add(_darkThemeRadio);
    stack.Children.Add(radioButtons);

    dialog.Content = stack;
    return dialog;
}

private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (_lightThemeRadio != null && _lightThemeRadio.IsChecked == true)
    {
        _theme = ElementTheme.Light;
        SaveTheme("Light");
        ApplyThemeColors(ElementTheme.Light);
    }
    else if (_darkThemeRadio != null && _darkThemeRadio.IsChecked == true)
    {
        _theme = ElementTheme.Dark;
        SaveTheme("Dark");
        ApplyThemeColors(ElementTheme.Dark);
    }
}

#endregion

#region WebView2

private async System.Threading.Tasks.Task InitializeWebViewAsync()
{
    await _cubeWebView.EnsureCoreWebView2Async();
    _cubeWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
    _cubeWebView.Source = new Uri(RendererHtmlPath);
    _webViewReady = true;
}

private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
{
    try
    {
        var message = e.TryGetWebMessageAsString();
        System.Diagnostics.Debug.WriteLine($"Renderer message: {message}");
    }
    catch { }
}

private void PostMessageToRenderer(object message)
{
    if (!_webViewReady || _cubeWebView.CoreWebView2 == null) return;
    var json = System.Text.Json.JsonSerializer.Serialize(message);
    _cubeWebView.CoreWebView2.PostWebMessageAsString(json);
}

private void PostStateToRenderer()
{
    var stickers = _state.Stickers.Select(c => c switch
    {
        CubeColor.White => "white",
        CubeColor.Yellow => "yellow",
        CubeColor.Green => "green",
        CubeColor.Blue => "blue",
        CubeColor.Red => "red",
        CubeColor.Orange => "orange",
        _ => "white"
    }).ToList();
    PostMessageToRenderer(new { type = "setState", stickers });
}

private void PostMoveToRenderer(string move)
{
    PostMessageToRenderer(new { type = "animateMove", move });
}

#endregion

#region Helpers

private SolidColorBrush GetBrush(string resourceKey)
{
    return resourceKey switch
    {
        "PageBackgroundBrush" => _theme == ElementTheme.Light
            ? GetBrushFromHex("#F5F7F8")
            : GetBrushFromHex("#102629"),
        "PanelBackgroundBrush" => _theme == ElementTheme.Light
            ? GetBrushFromHex("#E8EAEB")
            : GetBrushFromHex("#183438"),
        "PanelBorderBrush" => _theme == ElementTheme.Light
            ? GetBrushFromHex("#B9C0C1")
            : GetBrushFromHex("#31575B"),
        "AccentBrush" => _theme == ElementTheme.Light
            ? GetBrushFromHex("#009E73")
            : GetBrushFromHex("#00F5A0"),
        "MutedTextBrush" => _theme == ElementTheme.Light
            ? GetBrushFromHex("#5A7072")
            : GetBrushFromHex("#A4C1C2"),
        _ => GetBrushFromHex("#FFFFFF"),
    };
}

private SolidColorBrush GetBrushFromHex(string hex)
{
    hex = hex.TrimStart('#');
    var a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
    var r = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
    var g = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
    var b = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
    return new SolidColorBrush(ColorFromArgb(a, r, g, b));
}

private Windows.UI.Color ParseColor(string hex)
{
    hex = hex.TrimStart('#');
    var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
    var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
    var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
    return ColorFromArgb(255, r, g, b);
}

private static Windows.UI.Color ColorFromArgb(byte a, byte r, byte g, byte b)
{
    var color = default(Windows.UI.Color);
    color.A = a;
    color.R = r;
    color.G = g;
    color.B = b;
    return color;
}

#endregion
}

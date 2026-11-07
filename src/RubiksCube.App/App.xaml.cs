using Microsoft.UI.Xaml;

namespace RubiksCube.App;

public sealed class App : Application
{
    public static Window? MainWindow { get; private set; }

    public App()
    {
        // Code-only UI: no InitializeComponent() (XAML compilation disabled)
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
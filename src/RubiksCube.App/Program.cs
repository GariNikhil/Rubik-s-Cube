using Microsoft.UI.Xaml;

namespace RubiksCube.App;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.Start((p) => new App());
    }
}
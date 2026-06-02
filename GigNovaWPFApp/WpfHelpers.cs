using System.Windows.Media;
using GigNovaWSClient;

namespace GigNovaWPFApp.UserControls
{
    // Small shared helpers used by every user control / dialog in this folder.
    // - BuildClient<T>(path)  -> saves repeating the 4 lines of ApiClient setup everywhere
    // - Brush(hex)            -> saves repeating "new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex))"
    public static class WpfHelpers
    {
        // Builds an ApiClient<T> pointing at our WS (https://localhost:7059) at the given path.
        public static ApiClient<T> BuildClient<T>(string path)
        {
            ApiClient<T> client = new ApiClient<T>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = path;
            return client;
        }

        // Builds a SolidColorBrush from a "#RRGGBB" hex string.
        public static SolidColorBrush Brush(string hex)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hex);
            return new SolidColorBrush(color);
        }
    }
}

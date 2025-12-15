using System.Windows.Input;

namespace System.Windows
{
    public static class ApplicationCommands
    {
        // Provide commonly used RoutedUICommands as placeholders
        public static readonly RoutedUICommand Save = new("Save", "Save", typeof(ApplicationCommands));
        public static readonly RoutedUICommand Open = new("Open", "Open", typeof(ApplicationCommands));
        public static readonly RoutedUICommand Close = new("Close", "Close", typeof(ApplicationCommands));
    }
}

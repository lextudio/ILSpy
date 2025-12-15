namespace System.Windows.Input
{
    public static class NavigationCommands
    {
        public static readonly RoutedCommand BrowseBack = new RoutedCommand();
        public static readonly RoutedCommand BrowseForward = new RoutedCommand();
    }

    // Very small RoutedCommand implementation for shimming purposes
    public class RoutedCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) { }
    }
}

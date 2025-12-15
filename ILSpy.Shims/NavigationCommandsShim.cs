namespace System.Windows.Input
{
    public static class Keyboard
    {
        public static FocusedElement FocusedElement => new FocusedElement();
    }

    public static class Mouse
    {
        public static MouseButtonState RightButton => MouseButtonState.Released; // TODO: implement properly
    }

    public enum MouseButtonState
    {
        Released,
        Pressed
    }

    public class FocusedElement
    {
		internal IDisposable PreserveFocus()
		{
			throw new NotImplementedException();
		}

		internal IDisposable PreserveFocus(bool v)
		{
			throw new NotImplementedException();
		}

	}

    public static class NavigationCommands
    {
        public static readonly RoutedCommand BrowseBack = new RoutedCommand();
        public static readonly RoutedCommand BrowseForward = new RoutedCommand();

        public static readonly RoutedCommand Refresh = new RoutedCommand();
    }

    // Very small RoutedCommand implementation for shimming purposes
    public class RoutedCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) { }
    }

    // Minimal RoutedUICommand shim (WPF provides text/name/owner type overloads)
    public class RoutedUICommand : RoutedCommand
    {
        public RoutedUICommand() { }
        public RoutedUICommand(string text, string name, Type ownerType) { }
    }
}

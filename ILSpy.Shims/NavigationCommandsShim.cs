using Avalonia.Input;

namespace System.Windows.Input
{
    public class QueryCursorEventArgs
    {

    }

    public static class Cursors
    {
        public static Cursor Arrow { get; } = new Cursor(StandardCursorType.Arrow);
    }

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
    public class RoutedCommand : Avalonia.Labs.Input.RoutedCommand
    {
        public RoutedCommand() : base(string.Empty, (KeyGesture)null) { }
        public RoutedCommand(string name) : base(name, (KeyGesture)null) { }
    }

    // Minimal RoutedUICommand shim (WPF provides text/name/owner type overloads)
    public class RoutedUICommand : Avalonia.Labs.Input.RoutedCommand
    {
        public string Text { get; set; }

        public RoutedUICommand() : base(string.Empty, (KeyGesture)null) { }
        public RoutedUICommand(string text, string name, Type ownerType) : base(name, (KeyGesture)null) 
        {
            Text = text;
        }
    }
}

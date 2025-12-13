using System.Windows;
using System.Xml.Linq;

using ICSharpCode.ILSpy.Themes;


namespace ICSharpCode.ILSpy
{
    // Avalonia-specific partial for SessionSettings. Keeps the core class UI-neutral.
    public sealed partial class SessionSettings
    {
        public WindowState WindowState;
        public Rect WindowBounds;
        internal static Rect DefaultWindowBounds = new(10, 10, 750, 550);


		private void LoadTheme(XElement section)
		{
			Theme = FromString((string)section.Element(nameof(Theme)), ThemeManager.Current.DefaultTheme);
            WindowState = FromString((string)section.Element("WindowState"), WindowState.Normal);
		}
    }
}

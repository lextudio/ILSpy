using System;

using Avalonia.Controls;


namespace ICSharpCode.ILSpy.Themes
{
    // Minimal shim for ThemeManager used in ILSpy code. Provides factory methods for controls.
    public class ThemeManager
    {
        public static ThemeManager Current { get; } = new ThemeManager();
		public bool IsDarkTheme { get; internal set; } // TODO:

        public string DefaultTheme => IsDarkTheme ? "Dark" : "Light"; // TODO:

		internal static HighlightingColor GetColorForDarkTheme(HighlightingColor hc) // TODO:
		{
			throw new NotImplementedException();
		}


		public Button CreateButton()
        {
            return new Button();
        }
    }
}

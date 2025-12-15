using Avalonia.Controls;

namespace ICSharpCode.ILSpy.Themes
{
    // Minimal shim for ThemeManager used in ILSpy code. Provides factory methods for controls.
    public class ThemeManager
    {
        public static ThemeManager Current { get; } = new ThemeManager();

        public Button CreateButton()
        {
            return new Button();
        }
    }
}

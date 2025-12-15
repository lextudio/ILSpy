namespace System.Windows
{
    public static class Clipboard
    {
        public static void SetText(string text)
        {
            // no-op shim for non-WPF environments
        }

        public static string GetText()
        {
            return string.Empty;
        }
    }
}

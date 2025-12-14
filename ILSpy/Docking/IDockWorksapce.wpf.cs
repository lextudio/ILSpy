using ICSharpCode.ILSpy.TextView;

namespace ICSharpCode.ILSpy.Docking
{
    /// <summary>
    /// WPF-specific extensions to <see cref="IDockWorkspace"/>.
    /// </summary>
    public partial interface IDockWorkspace
    {
        void ShowText(AvalonEditTextOutput textOutput);
    }
}
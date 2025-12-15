using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;

namespace ICSharpCode.ILSpy.Views;

public partial class DecompilerPane : UserControl
{
    public DecompilerPane()
    {
        InitializeComponent();
    }

    public TextEditor Editor => TextEditor;
}

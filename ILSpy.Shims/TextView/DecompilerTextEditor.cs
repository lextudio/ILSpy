
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;

namespace ICSharpCode.ILSpy.TextView;

public class DecompilerTextEditor : TextEditor
{
	protected override IVisualLineTransformer CreateColorizer(IHighlightingDefinition highlightingDefinition)
	{
		return new ThemeAwareHighlightingColorizer(highlightingDefinition);
	}
}

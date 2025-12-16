using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace ICSharpCode.ILSpy.TextViewControl;

public class DecompilerTextEditor : TextEditor
{
	protected override IVisualLineTransformer CreateColorizer(IHighlightingDefinition highlightingDefinition)
	{
		return new ThemeAwareHighlightingColorizer(highlightingDefinition);
	}
}

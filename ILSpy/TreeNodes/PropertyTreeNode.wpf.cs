using System.Windows.Media;

using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy.TreeNodes
{
    public partial class PropertyTreeNode
    {
		public static ImageSource GetIcon(IProperty property)
		{
			return Images.GetIcon(property.IsIndexer ? MemberIcon.Indexer : MemberIcon.Property,
				Images.GetOverlayIcon(property.Accessibility), property.IsStatic);
		}
    }
}
using Avalonia.Media.Imaging;

using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy.TreeNodes
{
	public partial class PropertyTreeNode
	{
		public static Bitmap GetIcon(IProperty property)
		{
			return null;// TODO
			//Images.GetIcon(property.IsIndexer ? MemberIcon.Indexer : MemberIcon.Property,
			//	Images.GetOverlayIcon(property.Accessibility), property.IsStatic);
		}
	}
}
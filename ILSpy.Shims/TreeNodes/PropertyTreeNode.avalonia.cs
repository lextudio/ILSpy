using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy.TreeNodes
{
	public partial class PropertyTreeNode
	{
		public static object GetIcon(IProperty property)
		{
			return "";// TODO
			//Images.GetIcon(property.IsIndexer ? MemberIcon.Indexer : MemberIcon.Property,
			//	Images.GetOverlayIcon(property.Accessibility), property.IsStatic);
		}
	}
}
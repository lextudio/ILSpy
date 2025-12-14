using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy.TreeNodes
{
	public partial class TypeTreeNode
	{
		public static object GetIcon(ITypeDefinition type)
		{
			return "";// TODO:
			//Images.GetIcon(GetTypeIcon(type, out bool isStatic), GetOverlayIcon(type), isStatic);
		}
	}
}
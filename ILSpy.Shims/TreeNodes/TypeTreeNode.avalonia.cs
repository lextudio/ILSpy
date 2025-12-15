using Avalonia.Media.Imaging;

using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy.TreeNodes
{
	public partial class TypeTreeNode
	{
		public static Bitmap GetIcon(ITypeDefinition type)
		{
			return null;// TODO:
			//Images.GetIcon(GetTypeIcon(type, out bool isStatic), GetOverlayIcon(type), isStatic);
		}
	}
}
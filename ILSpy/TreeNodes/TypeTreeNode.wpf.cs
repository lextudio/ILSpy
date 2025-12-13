using System.Windows.Media;

using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy.TreeNodes
{
    public partial class TypeTreeNode
    {
        public static ImageSource GetIcon(ITypeDefinition type)
		{
			return Images.GetIcon(GetTypeIcon(type, out bool isStatic), GetOverlayIcon(type), isStatic);
		}
    }
}
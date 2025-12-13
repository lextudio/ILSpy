// this file contains the WPF-specific part of MethodTreeNode
using System.Windows.Media;
using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy.TreeNodes
{
    public partial class MethodTreeNode
    {
		public static ImageSource GetIcon(IMethod method)
		{
			if (method.IsOperator)
				return Images.GetIcon(MemberIcon.Operator, Images.GetOverlayIcon(method.Accessibility), false);

			if (method.IsExtensionMethod)
				return Images.GetIcon(MemberIcon.ExtensionMethod, Images.GetOverlayIcon(method.Accessibility), false);

			if (method.IsConstructor)
				return Images.GetIcon(MemberIcon.Constructor, Images.GetOverlayIcon(method.Accessibility), method.IsStatic);

			if (!method.HasBody && method.HasAttribute(KnownAttribute.DllImport))
				return Images.GetIcon(MemberIcon.PInvokeMethod, Images.GetOverlayIcon(method.Accessibility), true);

			return Images.GetIcon(method.IsVirtual ? MemberIcon.VirtualMethod : MemberIcon.Method,
				Images.GetOverlayIcon(method.Accessibility), method.IsStatic);
		}
    }
}

using System.Windows.Media;

using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy.TreeNodes
{
    // This file contains the WPF-specific part of FieldTreeNode
    public partial class FieldTreeNode
    {
		public static ImageSource GetIcon(IField field)
		{
			if (field.DeclaringType.Kind == TypeKind.Enum && field.ReturnType.Kind == TypeKind.Enum)
				return Images.GetIcon(MemberIcon.EnumValue, Images.GetOverlayIcon(field.Accessibility), false);

			if (field.IsConst)
				return Images.GetIcon(MemberIcon.Literal, Images.GetOverlayIcon(field.Accessibility), false);

			if (field.IsReadOnly)
				return Images.GetIcon(MemberIcon.FieldReadOnly, Images.GetOverlayIcon(field.Accessibility), field.IsStatic);

			return Images.GetIcon(MemberIcon.Field, Images.GetOverlayIcon(field.Accessibility), field.IsStatic);
		}
    }
}

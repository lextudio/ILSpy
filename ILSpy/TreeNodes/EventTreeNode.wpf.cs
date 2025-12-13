using System.Windows.Media;

using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy.TreeNodes
{
    public partial class EventTreeNode
    {
		public static ImageSource GetIcon(IEvent @event)
		{
			return Images.GetIcon(MemberIcon.Event, Images.GetOverlayIcon(@event.Accessibility), @event.IsStatic);
		}
    }
}
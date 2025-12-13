using System.Windows.Threading;

using ICSharpCode.ILSpyX.TreeView.PlatformAbstractions;

namespace ICSharpCode.ILSpy.TreeNodes
{
    public partial class ILSpyTreeNode
    {
        public override void ActivateItemSecondary(IPlatformRoutedEventArgs e)
		{
			var assemblyTreeModel = AssemblyTreeModel;

			assemblyTreeModel.SelectNode(this, inNewTabPage: true);

			App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, assemblyTreeModel.RefreshDecompiledView);
		}
    }
}
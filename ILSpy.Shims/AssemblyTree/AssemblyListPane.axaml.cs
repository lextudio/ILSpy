using System;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using ICSharpCode.ILSpyX.TreeView;

namespace ICSharpCode.ILSpy.Views;

public partial class AssemblyListPane : UserControl
{
    public AssemblyListPane()
    {
        InitializeComponent();
    }

    public TreeView ExplorerTreeView => TreeView;

	public IDisposable LockUpdates()
	{
		// TODO:
		return null;
	}

	public void ScrollIntoView(SharpTreeNode node)
	{
		// TODO:
	}
}

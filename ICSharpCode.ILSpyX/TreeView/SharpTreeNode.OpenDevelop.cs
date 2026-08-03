// SharpTreeView duplicate resolution (2026-08-02, OpenDevelop doc/technotes/ilspy.md). This file
// is authored and owned by OpenDevelop, not upstream ILSpy - it is a NEW file added alongside the
// ILSpy checkout (like ICSharpCode.ILSpyX.TreeView.csproj itself), not an edit to existing ILSpy
// source. It must live in this project (not a downstream OpenDevelop project) because C# partial
// classes only merge within the same assembly compilation - a `partial` declaration in a
// different assembly creates a shadowing duplicate type instead of extending this one (verified:
// CS0436 when this was tried from src/Libraries/SharpTreeView/ICSharpCode.TreeView).
//
// ILSpyX's SharpTreeNode has no equivalent of GetModel()/Model/ShowContextMenu, which OpenDevelop's
// own (now-removed) SharpTreeNode used to declare and which are used pervasively by
// ClassBrowser/Debugger/UnitTesting node classes across the OpenDevelop solution.
//
// ShowContextMenu's parameter is typed `object` rather than WPF's ContextMenuEventArgs: this
// project is deliberately platform-neutral (no WPF reference, matching ILSpyX's own cross-plat
// design - see ICSharpCode.ILSpyX.TreeView.csproj), and a repo-wide search found every existing
// override ignores the parameter entirely (they all just call MenuService.ShowContextMenu with
// their own model, not `e`) and found no caller invoking node.ShowContextMenu(...) at all today -
// so this is a currently-unused extensibility hook, not a behavior change for any live code path.

namespace ICSharpCode.ILSpyX.TreeView
{
	public partial class SharpTreeNode
	{
		/// <summary>
		/// Gets the underlying model object.
		/// </summary>
		/// <remarks>
		/// This property calls the virtual <see cref="GetModel()"/> helper method.
		/// I didn't make the property itself virtual because deriving classes
		/// may wish to replace it with a more specific return type,
		/// but C# doesn't support variance in override declarations.
		/// </remarks>
		public object? Model => GetModel();

		protected virtual object? GetModel() => null;

		public virtual void ShowContextMenu(object? e)
		{
		}
	}
}

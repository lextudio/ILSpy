// Copyright (c) 2019 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Composition;
using System.Windows.Input;
using System.Windows.Media;

using ICSharpCode.ILSpyX.Search;

namespace ICSharpCode.ILSpy.Search
{
	public class SearchModeModel
	{
		public SearchMode Mode { get; init; }
		public string Name { get; init; }
		public ImageSource Image { get; init; }
	}

	// SharpTreeView duplicate resolution / host-neutral pane model vertical slice
	// (doc/technotes/ilspy.md "Immediate next actions" #3, 2026-08-02): derives directly from
	// OpenDevelop's ICSharpCode.SharpDevelop.ViewModels.ToolPaneModel instead of ILSpy's own
	// ICSharpCode.ILSpy.ViewModels.ToolPaneModel, so IlSpyWorkspaceHost can register it with
	// OpenDevelop's DockWorkspace directly (DockWorkspaceExtensibility.AddToolPane) instead of
	// wrapping it in the property-mirroring IlSpyToolPaneAdapter - proving the doc's target
	// contract (one shared PaneModel/ToolPaneModel hierarchy) works for a real ILSpy pane, not
	// just the built-in ProjectBrowserViewModel side of this same vertical slice. This is why
	// [ExportToolPane] (contract type ICSharpCode.ILSpy.ViewModels.ToolPaneModel, see
	// Commands/ExportCommandAttribute.cs) becomes a plain [Export(typeof(SearchPaneModel))]:
	// the only consumer of the "ToolPane" contract enumeration is ILSpy's own (unused here)
	// Docking/DockWorkspace.cs ToolPanes property, and IlSpyWorkspaceHost already fetches this
	// pane by its concrete type (`exportProvider.GetExportedValue<SearchPaneModel>()`), not by
	// that contract.
	[Export(typeof(SearchPaneModel))]
	[Shared]
	public partial class SearchPaneModel : ICSharpCode.SharpDevelop.ViewModels.ToolPaneModel
	{
		public const string PaneContentId = "searchPane";

		private readonly SettingsService settingsService;
		private string searchTerm;

		public SearchPaneModel(SettingsService settingsService)
		{
			this.settingsService = settingsService;
			ContentId = PaneContentId;
			Title = Properties.Resources.SearchPane_Search;
			Icon = "Images/Search";
			ShortcutKey = new(Key.F, ModifierKeys.Control | ModifierKeys.Shift);
			IsCloseable = true;
			// OpenDevelop's ToolPaneModel renders Content via WPF's implicit DataTemplate lookup
			// on its runtime type (AvalonDock's anchorable style binds a ContentPresenter to it) -
			// matching what IlSpyToolPaneAdapter used to do (`Content = the raw view-model`) so the
			// existing [DataTemplate(typeof(SearchPaneModel))] registration (SearchPane.xaml.cs)
			// still resolves the view the same way.
			Content = this;

			MessageBus<ShowSearchPageEventArgs>.Subscribers += (_, e) => {
				SearchTerm = e.SearchTerm;
				Show();
			};
			MessageBus<ApplySessionSettingsEventArgs>.Subscribers += ApplySessionSettings;
		}

		private void ApplySessionSettings(object sender, ApplySessionSettingsEventArgs e)
		{
			e.SessionSettings.SelectedSearchMode = SessionSettings.SelectedSearchMode;
		}

		public SearchModeModel[] SearchModes { get; } = [
			new() { Mode = SearchMode.TypeAndMember, Image = Images.Library, Name = "Types and Members" },
			new() { Mode = SearchMode.Type, Image = Images.Class, Name = "Type" },
			new() { Mode = SearchMode.Member, Image = Images.Property, Name = "Member" },
			new() { Mode = SearchMode.Method, Image = Images.Method, Name = "Method" },
			new() { Mode = SearchMode.Field, Image = Images.Field, Name = "Field" },
			new() { Mode = SearchMode.Property, Image = Images.Property, Name = "Property" },
			new() { Mode = SearchMode.Event, Image = Images.Event, Name = "Event" },
			new() { Mode = SearchMode.Literal, Image = Images.Literal, Name = "Constant" },
			new() { Mode = SearchMode.Token, Image = Images.Library, Name = "Metadata Token" },
			new() { Mode = SearchMode.Resource, Image = Images.Resource, Name = "Resource" },
			new() { Mode = SearchMode.Assembly, Image = Images.Assembly, Name = "Assembly" },
			new() { Mode = SearchMode.Namespace, Image = Images.Namespace, Name = "Namespace" }
		];

		public SessionSettings SessionSettings => settingsService.SessionSettings;

		public string SearchTerm {
			get => searchTerm;
			set => SetProperty(ref searchTerm, value);
		}
	}
}

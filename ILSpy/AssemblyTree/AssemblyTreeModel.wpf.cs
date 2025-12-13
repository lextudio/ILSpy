// this file contains the WPF-specific part of AssemblyTreeModel
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

using ICSharpCode.ILSpy.AssemblyTree;

using ICSharpCode.ILSpy.TextView;
using ICSharpCode.ILSpy.TreeNodes;
using ICSharpCode.ILSpy.ViewModels;
using ICSharpCode.ILSpyX;
using ICSharpCode.ILSpyX.TreeView;

using TomsToolbox.Essentials;

namespace ICSharpCode.ILSpy.AssemblyTree
{
    public partial class AssemblyTreeModel
    {
        private AssemblyListPane? activeView;

        public void SetActiveView(AssemblyListPane activeView)
		{
			this.activeView = activeView;
		}

        private static void LoadInitialAssemblies(AssemblyList assemblyList)
		{
			// Called when loading an empty assembly list; so that
			// the user can see something initially.
			System.Reflection.Assembly[] initialAssemblies = {
				typeof(object).Assembly,
				typeof(Uri).Assembly,
				typeof(System.Linq.Enumerable).Assembly,
				typeof(System.Xml.XmlDocument).Assembly,
				typeof(System.Windows.Markup.MarkupExtension).Assembly,
				typeof(System.Windows.Rect).Assembly,
				typeof(System.Windows.UIElement).Assembly,
				typeof(System.Windows.FrameworkElement).Assembly
			};
			foreach (System.Reflection.Assembly asm in initialAssemblies)
				assemblyList.OpenAssembly(asm.Location);
		}

        private void RefreshInternal()
		{
			using (Keyboard.FocusedElement.PreserveFocus())
			{
				var path = GetPathForNode(SelectedItem);

				ShowAssemblyList(settingsService.AssemblyListManager.LoadList(AssemblyList.ListName));
				SelectNode(FindNodeByPath(path, true), inNewTabPage: false);

				RefreshDecompiledView();
			}
		}
        
		private void NavigateTo(RequestNavigateEventArgs e, bool inNewTabPage = false)
		{
			if (e.Uri.Scheme != "resource")
			{
				return;
			}

			TabPageModel tabPage = DockWorkspace.ActiveTabPage;
			ViewState? oldState = tabPage.GetState();
			ViewState? newState;

			if (navigatingToState == null)
			{
				if (oldState != null)
				{
					history.UpdateCurrent(new NavigationState(tabPage, oldState));
				}

				newState = new ViewState { ViewedUri = e.Uri };

				if (inNewTabPage)
				{
					tabPage = DockWorkspace.AddTabPage();
				}
			}
			else
			{
				newState = navigatingToState.ViewState;
				tabPage = DockWorkspace.ActiveTabPage = navigatingToState.TabPage;
			}

			bool needsNewNavigationEntry = !inNewTabPage && selectedItems?.Length == 0;

			UnselectAll();

			if (e.Uri.Host == "aboutpage")
			{
				MessageBus.Send(this, new ShowAboutPageEventArgs(DockWorkspace.ActiveTabPage));
				e.Handled = true;
			}
			else
			{
				AvalonEditTextOutput output = new AvalonEditTextOutput {
					Address = e.Uri,
					Title = e.Uri.AbsolutePath,
					EnableHyperlinks = true
				};
				using (Stream? s = typeof(App).Assembly.GetManifestResourceStream(typeof(App), e.Uri.AbsolutePath))
				{
					if (s != null)
					{
						using StreamReader r = new StreamReader(s);
						string? line;
						while ((line = r.ReadLine()) != null)
						{
							output.Write(line);
							output.WriteLine();
						}
					}
				}
				DockWorkspace.ShowText(output);
				e.Handled = true;
			}

			if (navigatingToState == null)
			{
				// the call to UnselectAll() above already creates a new navigation entry,
				// we just need to make sure it contains something useful.
				if (!needsNewNavigationEntry)
				{
					history.UpdateCurrent(new NavigationState(tabPage, tabPage.GetState()));
				}
				else
				{
					history.Record(new NavigationState(tabPage, tabPage.GetState()));
				}
			}
		}

        private static bool FormatExceptions(App.ExceptionData[] exceptions, [NotNullWhen(true)] out AvalonEditTextOutput? output)
		{
			output = null;

			var result = exceptions.FormatExceptions();
			if (result.IsNullOrEmpty())
				return false;

			output = new();
			output.Write(result);
			return true;

		}

        private void TreeView_SelectionChanged(SharpTreeNode[] oldSelection, SharpTreeNode[] newSelection)
		{
			var activeTabPage = DockWorkspace.ActiveTabPage;
			ViewState? oldState = activeTabPage.GetState();
			ViewState? newState;

			if (navigatingToState == null)
			{
				if (oldState != null)
				{
					history.UpdateCurrent(new NavigationState(activeTabPage, oldState));
				}

				newState = new ViewState { DecompiledNodes = [.. newSelection.Cast<ILSpyTreeNode>()] };
			}
			else
			{
				newState = navigatingToState.ViewState;
			}

			if (newSelection.Length == 0)
			{
				// To cancel any pending decompilation requests and show an empty tab
				DecompileSelectedNodes(newState);
			}
			else
			{
				var delayDecompilationRequestDueToContextMenu = Mouse.RightButton == MouseButtonState.Pressed;

				if (!delayDecompilationRequestDueToContextMenu)
				{
					var previousNodes = oldState?.DecompiledNodes
						?.Select(n => FindNodeByPath(GetPathForNode(n), true))
						.ExceptNullItems()
						.ToArray() ?? [];

					if (!previousNodes.SequenceEqual(SelectedItems))
					{
						DecompileSelectedNodes(newState);
					}
				}
				else
				{
					// ensure that we are only connected once to the event, else we might get multiple notifications
					ContextMenuProvider.ContextMenuClosed -= ContextMenuClosed;
					ContextMenuProvider.ContextMenuClosed += ContextMenuClosed;
				}
			}

			MessageBus.Send(this, new AssemblyTreeSelectionChangedEventArgs());

			return;

			void ContextMenuClosed(object? sender, EventArgs e)
			{
				ContextMenuProvider.ContextMenuClosed -= ContextMenuClosed;

				Dispatcher.BeginInvoke(DispatcherPriority.Background, () => {
					if (Mouse.RightButton != MouseButtonState.Pressed)
					{
						RefreshDecompiledView();
					}
				});
			}
		}
    }
}
// this file contains the WPF-specific part of AssemblyTreeModel
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpy.AppEnv;
using ICSharpCode.ILSpy.AssemblyTree;
using ICSharpCode.ILSpy.Properties;
using ICSharpCode.ILSpy.TextView;
using ICSharpCode.ILSpy.TreeNodes;
using ICSharpCode.ILSpy.Updates;
using ICSharpCode.ILSpy.ViewModels;
using ICSharpCode.ILSpyX;
using ICSharpCode.ILSpyX.TreeView;

using TomsToolbox.Composition;
using TomsToolbox.Essentials;

namespace ICSharpCode.ILSpy.AssemblyTree
{
    public partial class AssemblyTreeModel
    {
		public AssemblyTreeModel(SettingsService settingsService, LanguageService languageService, IExportProvider exportProvider)
		{
			this.settingsService = settingsService;
			this.languageService = languageService;
			this.exportProvider = exportProvider;

			Title = Resources.Assemblies;
			ContentId = PaneContentId;
			IsCloseable = false;
			ShortcutKey = new KeyGesture(Key.F6);

			MessageBus<NavigateToReferenceEventArgs>.Subscribers += JumpToReference;
			MessageBus<SettingsChangedEventArgs>.Subscribers += (sender, e) => Settings_PropertyChanged(sender, e);
			MessageBus<ApplySessionSettingsEventArgs>.Subscribers += ApplySessionSettings;
			MessageBus<ActiveTabPageChangedEventArgs>.Subscribers += ActiveTabPageChanged;
			MessageBus<TabPagesCollectionChangedEventArgs>.Subscribers += (_, e) => history.RemoveAll(s => !DockWorkspace.TabPages.Contains(s.TabPage));
			MessageBus<ResetLayoutEventArgs>.Subscribers += ResetLayout;
			MessageBus<NavigateToEventArgs>.Subscribers += (_, e) => NavigateTo(e.Request, e.InNewTabPage);
			MessageBus<MainWindowLoadedEventArgs>.Subscribers += (_, _) => {
				Initialize();
				Show();
			};

			EventManager.RegisterClassHandler(typeof(Window), Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler((_, e) => NavigateTo(e)));

			refreshThrottle = new(DispatcherPriority.Background, RefreshInternal);

			AssemblyList = settingsService.CreateEmptyAssemblyList();
		}

		public void Initialize()
		{
			AssemblyList = settingsService.LoadInitialAssemblyList();

			HandleCommandLineArguments(App.CommandLineArguments);

			var loadPreviousAssemblies = settingsService.MiscSettings.LoadPreviousAssemblies;
			if (AssemblyList.GetAssemblies().Length == 0
				&& AssemblyList.ListName == AssemblyListManager.DefaultListName
				&& loadPreviousAssemblies)
			{
				LoadInitialAssemblies(AssemblyList);
			}

			ShowAssemblyList(AssemblyList);

			var sessionSettings = settingsService.SessionSettings;
			if (sessionSettings.ActiveAutoLoadedAssembly != null
				&& File.Exists(sessionSettings.ActiveAutoLoadedAssembly))
			{
				AssemblyList.Open(sessionSettings.ActiveAutoLoadedAssembly, true);
			}

			Dispatcher.BeginInvoke(DispatcherPriority.Loaded, OpenAssemblies);
		}

		private void ShowAssemblyList(ICSharpCode.ILSpyX.AssemblyList assemblyList)
		{
			history.Clear();

			AssemblyList.CollectionChanged -= assemblyList_CollectionChanged;
			AssemblyList = assemblyList;
			assemblyList.CollectionChanged += assemblyList_CollectionChanged;

			assemblyListTreeNode = new(assemblyList) {
				Select = x => SelectNode(x)
			};

			Root = assemblyListTreeNode;

			var mainWindow = Application.Current?.MainWindow;

			if (mainWindow == null)
				return;

			if (assemblyList.ListName == AssemblyListManager.DefaultListName)
#if DEBUG
				mainWindow.Title = $"ILSpy {DecompilerVersionInfo.FullVersion}";
#else
				mainWindow.Title = "ILSpy";
#endif
			else
#if DEBUG
				mainWindow.Title = string.Format(settingsService.MiscSettings.AllowMultipleInstances ? "{1} - {0}" : "{0} - {1}", $"ILSpy {DecompilerVersionInfo.FullVersion}", assemblyList.ListName);
#else
				mainWindow.Title = string.Format(settingsService.MiscSettings.AllowMultipleInstances ? "{1} - {0}" : "{0} - {1}", "ILSpy", assemblyList.ListName);
#endif
		}

		private void LoadAssemblies(IEnumerable<string> fileNames, List<LoadedAssembly>? loadedAssemblies = null, bool focusNode = true)
		{
			using (Keyboard.FocusedElement.PreserveFocus(!focusNode))
			{
				AssemblyTreeNode? lastNode = null;

				var assemblyList = AssemblyList;

				foreach (string file in fileNames)
				{
					var assembly = assemblyList.OpenAssembly(file);

					if (loadedAssemblies != null)
					{
						loadedAssemblies.Add(assembly);
					}
					else
					{
						var node = assemblyListTreeNode?.FindAssemblyNode(assembly);
						if (node != null && focusNode)
						{
							lastNode = node;
							activeView?.ScrollIntoView(node);
							SelectedItems = [.. SelectedItems, node];
						}
					}
				}
				if (focusNode && lastNode != null)
				{
					activeView?.FocusNode(lastNode);
				}
			}
		}

		private async Task NavigateOnLaunch(string? navigateTo, string[]? activeTreeViewPath, UpdateSettings? updateSettings, List<LoadedAssembly> relevantAssemblies)
		{
			var initialSelection = SelectedItem;
			if (navigateTo != null)
			{
				bool found = false;
				if (navigateTo.StartsWith("N:", StringComparison.Ordinal))
				{
					string namespaceName = navigateTo.Substring(2);
					foreach (LoadedAssembly asm in relevantAssemblies)
					{
						var asmNode = assemblyListTreeNode?.FindAssemblyNode(asm);
						if (asmNode != null)
						{
							// FindNamespaceNode() blocks the UI if the assembly is not yet loaded,
							// so use an async wait instead.
							await asm.GetMetadataFileAsync().Catch<Exception>(_ => { });
							NamespaceTreeNode nsNode = asmNode.FindNamespaceNode(namespaceName);
							if (nsNode != null)
							{
								found = true;
								if (SelectedItem == initialSelection)
								{
									SelectNode(nsNode);
								}
								break;
							}
						}
					}
				}
				else if (navigateTo == "none")
				{
					// Don't navigate anywhere; start empty.
					// Used by ILSpy VS addin, it'll send us the real location to navigate to via IPC.
					found = true;
				}
				else
				{
					IEntity? mr = await Task.Run(() => FindEntityInRelevantAssemblies(navigateTo, relevantAssemblies));

					// Make sure we wait for assemblies being loaded...
					// BeginInvoke in LoadedAssembly.LookupReferencedAssemblyInternal
					await Dispatcher.InvokeAsync(delegate { }, DispatcherPriority.Normal);

					if (mr is { ParentModule.MetadataFile: not null })
					{
						found = true;
						if (SelectedItem == initialSelection)
						{
							await JumpToReferenceAsync(mr, null);
						}
					}
				}
				if (!found && SelectedItem == initialSelection)
				{
					AvalonEditTextOutput output = new AvalonEditTextOutput();
					output.Write($"Cannot find '{navigateTo}' in command line specified assemblies.");
					DockWorkspace.ShowText(output);
				}
			}
			else if (relevantAssemblies.Count == 1)
			{
				// NavigateTo == null and an assembly was given on the command-line:
				// Select the newly loaded assembly
				var asmNode = assemblyListTreeNode?.FindAssemblyNode(relevantAssemblies[0]);
				if (asmNode != null && SelectedItem == initialSelection)
				{
					SelectNode(asmNode);
				}
			}
			else if (updateSettings != null)
			{
				SharpTreeNode? node = null;
				if (activeTreeViewPath?.Length > 0)
				{
					foreach (var asm in AssemblyList.GetAssemblies())
					{
						if (asm.FileName == activeTreeViewPath[0])
						{
							// FindNodeByPath() blocks the UI if the assembly is not yet loaded,
							// so use an async wait instead.
							await asm.GetMetadataFileAsync().Catch<Exception>(_ => { });
						}
					}
					node = FindNodeByPath(activeTreeViewPath, true);
				}
				if (SelectedItem == initialSelection)
				{
					if (node != null)
					{
						SelectNode(node);

						// only if not showing the about page, perform the update check:
						MessageBus.Send(this, new CheckIfUpdateAvailableEventArgs());
					}
					else
					{
						MessageBus.Send(this, new ShowAboutPageEventArgs(DockWorkspace.ActiveTabPage));
					}
				}
			}
		}

		public void DecompileSelectedNodes(ViewState? newState = null)
		{
			object? source = this.sourceOfReference;
			this.sourceOfReference = null;
			var activeTabPage = DockWorkspace.ActiveTabPage;

			if (activeTabPage.FrozenContent)
			{
				activeTabPage = DockWorkspace.AddTabPage();
			}

			activeTabPage.SupportsLanguageSwitching = true;

			if (newState != null && navigatingToState == null)
			{
				history.Record(new NavigationState(activeTabPage, newState));
			}

			if (SelectedItems.Length == 1)
			{
				if (SelectedItem is ILSpyTreeNode node && node.View(activeTabPage))
					return;
			}
			if (newState?.ViewedUri != null)
			{
				// TODO: NavigateTo(new(newState.ViewedUri, null));
				return;
			}

			var options = activeTabPage.CreateDecompilationOptions();
			options.TextViewState = newState as DecompilerTextViewState;
			activeTabPage.ShowTextViewAsync(textView => {
				return textView.DecompileAsync(this.CurrentLanguage, this.SelectedNodes, source, options);
			});
		}

		private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (sender is SessionSettings sessionSettings)
			{
				switch (e.PropertyName)
				{
					case nameof(SessionSettings.ActiveAssemblyList):
						ShowAssemblyList(sessionSettings.ActiveAssemblyList);
						RefreshDecompiledView();
						break;
					case nameof(SessionSettings.Theme):
						// update syntax highlighting and force reload (AvalonEdit does not automatically refresh on highlighting change)
						DecompilerTextView.RegisterHighlighting();
						RefreshDecompiledView();
						break;
					case nameof(SessionSettings.CurrentCulture):
						// TODO: MessageBox.Show(Resources.SettingsChangeRestartRequired, "ILSpy");
						break;
				}
			}
			else if (sender is LanguageSettings)
			{
				switch (e.PropertyName)
				{
					case nameof(LanguageSettings.LanguageId) or nameof(LanguageSettings.LanguageVersionId):
						RefreshDecompiledView();
						break;
					default:
						Refresh();
						break;
				}
			}
		}

		private AssemblyListPane? activeView;

        public void SetActiveView(AssemblyListPane activeView)
		{
			this.activeView = activeView;
		}

		public void SelectNode(SharpTreeNode? node, bool inNewTabPage = false)
		{
			if (node == null)
				return;

			if (node.AncestorsAndSelf().Any(item => item.IsHidden))
			{
				MessageBox.Show(Resources.NavigationFailed, "ILSpy", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			if (inNewTabPage)
			{
				DockWorkspace.AddTabPage();
				SelectedItem = null;
			}

			if (SelectedItem == node)
			{
				Dispatcher.BeginInvoke(RefreshDecompiledView);
			}
			else
			{
				activeView?.ScrollIntoView(node);
				SelectedItem = node;

				Dispatcher.BeginInvoke(DispatcherPriority.Background, () => {
					activeView?.ScrollIntoView(node);
				});
			}
		}

		private async Task OpenAssemblies()
		{
			await HandleCommandLineArgumentsAfterShowList(App.CommandLineArguments, settingsService.GetSettings<UpdateSettings>());

			if (FormatExceptions(App.StartupExceptions.ToArray(), out var output))
			{
				output.Title = "Startup errors";

				DockWorkspace.AddTabPage();
				DockWorkspace.ShowText(output);
			}
		}

		public async Task HandleSingleInstanceCommandLineArguments(string[] args)
		{
			var cmdArgs = CommandLineArguments.Create(args);

			await Dispatcher.InvokeAsync(async () => {

				if (!HandleCommandLineArguments(cmdArgs))
					return;

				var window = Application.Current.MainWindow;

				if (!cmdArgs.NoActivate && window is { WindowState: WindowState.Minimized })
				{
					window.WindowState = WindowState.Normal;
				}

				await HandleCommandLineArgumentsAfterShowList(cmdArgs);
			});
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
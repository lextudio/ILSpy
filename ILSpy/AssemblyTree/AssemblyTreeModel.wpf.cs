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
    }
}
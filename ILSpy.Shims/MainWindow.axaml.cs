using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using ICSharpCode.ILSpy.Search;
using ICSharpCode.ILSpy.Views;
using ICSharpCode.ILSpy.TextViewControl;
using System.Windows.Threading;

namespace ICSharpCode.ILSpy
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? viewModel;
        private AssemblyListPane leftDockView = null!;
        private DecompilerTextView centerDockView = null!;
        private SearchPane searchDockView = null!;
        private Document? documentHost;
        private Factory dockFactory = null!;
        private ToolDock? searchDock;
        private ProportionalDock? verticalLayout;
        private ProportionalDockSplitter? searchSplitter;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, () => {
				viewModel.Workspace.InitializeLayout();
				MessageBus.Send(this, new MainWindowLoadedEventArgs());
			});
        }

        public MainWindow(MainWindowViewModel viewModel) : this()
        {
            this.viewModel = viewModel;
            DataContext = viewModel;
            ConfigureDockLayout();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ConfigureDockLayout()
        {
            if (viewModel == null) return;

            leftDockView = new AssemblyListPane { DataContext = viewModel.AssemblyTreeModel };
            try {
                // Ensure the model knows about this view (some UI frameworks don't trigger DataContext property change the same way)
                viewModel.AssemblyTreeModel?.SetActiveView(leftDockView);
                Console.WriteLine($"[Log][MainWindow] Explicitly called SetActiveView on AssemblyTreeModel instance={viewModel.AssemblyTreeModel?.GetHashCode()}");
            } catch { }
            try {
                Console.WriteLine($"[Log][MainWindow] Created AssemblyListPane with DataContext instance={viewModel.AssemblyTreeModel?.GetHashCode()}");
            } catch { }
            centerDockView = new DecompilerTextView { DataContext = viewModel };
            searchDockView = new SearchPane { DataContext = viewModel.SearchPaneModel };
            try {
                Console.WriteLine($"[Log][MainWindow] Created SearchPane with SearchPaneModel.DataContext AssemblyTreeModel instance={viewModel.AssemblyTreeModel?.GetHashCode()}");
            } catch { }

            documentHost = new Document
            {
                Id = "DocumentPane",
                Content = centerDockView,
                Context = viewModel,
                CanClose = false,
                CanFloat = false,
                CanPin = false
            };

            var documentDock = new DocumentDock
            {
                Id = "DocumentDock",
                Title = "DocumentDock",
                VisibleDockables = new ObservableCollection<IDockable> { documentHost },
                ActiveDockable = documentHost
            };

            var tool = new Tool
            {
                Id = "Explorer",
                Title = "Explorer",
                Content = leftDockView,
                Context = viewModel,
                CanClose = false,
                CanFloat = false,
                CanPin = false
            };

            (viewModel.Workspace as ICSharpCode.ILSpy.Docking.DockWorkspace)?.RegisterTool(tool);

            var toolDock = new ToolDock
            {
                Id = "LeftDock",
                Title = "LeftDock",
                Alignment = Alignment.Left,
                VisibleDockables = new ObservableCollection<IDockable> { tool },
                ActiveDockable = tool
            };

            toolDock.Proportion = 0.3;
            documentDock.Proportion = 0.7;

            var searchTool = new Tool
            {
                Id = ICSharpCode.ILSpy.Search.SearchPaneModel.PaneContentId,
                Title = "Search",
                Content = searchDockView,
                Context = viewModel,
                CanClose = true,
                CanFloat = false,
                CanPin = false
            };

            (viewModel.Workspace as ICSharpCode.ILSpy.Docking.DockWorkspace)?.RegisterTool(searchTool);

            searchDock = new ToolDock
            {
                Id = "SearchDock",
                Title = "Search",
                Alignment = Alignment.Top,
                VisibleDockables = new ObservableCollection<IDockable>(),
                ActiveDockable = null,
                CanCloseLastDockable = true
            };
            searchDock.Proportion = 0.25;

            searchSplitter = new ProportionalDockSplitter { Id = "SearchSplitter", CanResize = true };

            var rightDock = new ProportionalDock
            {
                Id = "RightDock",
                Orientation = Dock.Model.Core.Orientation.Vertical,
                VisibleDockables = new ObservableCollection<IDockable>
                {
                    documentDock
                },
                ActiveDockable = documentDock
            };

            if (viewModel.Workspace is ICSharpCode.ILSpy.Docking.DockWorkspace dw)
            {
                dw.RegisterDockable(searchDock);
                dw.RegisterDockable(searchSplitter);
            }

            var mainLayout = new ProportionalDock
            {
                Id = "MainLayout",
                Orientation = Dock.Model.Core.Orientation.Horizontal,
                VisibleDockables = new ObservableCollection<IDockable>
                {
                    toolDock,
                    new ProportionalDockSplitter { CanResize = true },
                    rightDock
                },
                ActiveDockable = rightDock
            };

            var rootDock = new RootDock
            {
                Id = "Root",
                Title = "Root",
                VisibleDockables = new ObservableCollection<IDockable> { mainLayout },
                ActiveDockable = mainLayout
            };

            dockFactory = new Factory();
            var dockHost = this.FindControl<DockControl>("DockHost");
            if (dockHost != null)
            {
                dockHost.Factory = dockFactory;
                dockHost.Layout = rootDock;
            }
        }
    }
}

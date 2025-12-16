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

namespace ICSharpCode.ILSpy.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? viewModel;
        private AssemblyListPane leftDockView = null!;
        private DecompilerTextView centerDockView = null!;
        private SearchPane searchDockView = null!;
        private Document? documentHost;
        private Factory dockFactory = null!;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
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
            centerDockView = new DecompilerTextView { DataContext = viewModel };
            searchDockView = new SearchPane { DataContext = viewModel };

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

            var searchTool = new Tool
            {
                Id = "SearchTool",
                Title = "Search",
                Content = searchDockView,
                Context = viewModel,
                CanClose = false,
                CanFloat = false,
                CanPin = false
            };

            var searchDock = new ToolDock
            {
                Id = "SearchDock",
                Title = "Search",
                Alignment = Alignment.Top,
                VisibleDockables = new ObservableCollection<IDockable> { searchTool },
                ActiveDockable = searchTool
            };
            searchDock.Proportion = 0.25;

            var verticalLayout = new ProportionalDock
            {
                Id = "VerticalLayout",
                Orientation = Dock.Model.Core.Orientation.Vertical,
                VisibleDockables = new ObservableCollection<IDockable>
                {
                    mainLayout
                },
                ActiveDockable = mainLayout
            };

            var rootDock = new RootDock
            {
                Id = "Root",
                Title = "Root",
                VisibleDockables = new ObservableCollection<IDockable> { verticalLayout },
                ActiveDockable = verticalLayout
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

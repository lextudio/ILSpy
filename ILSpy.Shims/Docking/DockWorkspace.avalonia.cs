using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using ICSharpCode.ILSpy.ViewModels;
using ICSharpCode.ILSpy.Search;

namespace ICSharpCode.ILSpy.Docking
{
  /// <summary>
  /// Avalonia-specific extensions to <see cref="DockWorkspace"/>.
  /// </summary>
  public partial class DockWorkspace
  {
    public void InitializeLayout()
    {
      try
      {
        var app = Avalonia.Application.Current;
        var mainWindow = (app?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow
                         ?? Avalonia.Controls.TopLevel.GetTopLevel(null);
        if (mainWindow == null)
          return;

        var dockHost = mainWindow.FindControl<DockControl>("DockHost");
        if (dockHost == null)
          return;

        // If layout is already initialized (by MainWindow.ConfigureDockLayout), don't overwrite it
        if (dockHost.Layout != null && dockHost.Factory != null)
        {
          Console.WriteLine("DockWorkspace.InitializeLayout: Layout already initialized, skipping creation but hooking listeners");
          HookUpToolListeners(dockHost);
          return;
        }

        // Fallback: create a basic layout structure if not already done
        Console.WriteLine("DockWorkspace.InitializeLayout: Creating layout structure");
        var factory = new Factory();

        // Create document dock (for the main decompiler/editor area)
        var documentDock = new DocumentDock
        {
          Id = "DocumentDock",
          Title = "Documents",
          VisibleDockables = new ObservableCollection<IDockable>(),
          ActiveDockable = null
        };

        // Create tool dock (for side panels like Search, etc.)
        var toolDock = new ToolDock
        {
          Id = "ToolDock",
          Title = "Tools",
          Alignment = Alignment.Right,
          VisibleDockables = new ObservableCollection<IDockable>(),
          ActiveDockable = null
        };

        toolDock.Proportion = 0.25;
        documentDock.Proportion = 0.75;

        // Create main horizontal layout
        var mainLayout = new ProportionalDock
        {
          Id = "MainLayout",
          Title = "MainLayout",
          Orientation = Dock.Model.Core.Orientation.Horizontal,
          VisibleDockables = new ObservableCollection<IDockable>
          {
            documentDock,
            new ProportionalDockSplitter { CanResize = true },
            toolDock
          },
          ActiveDockable = documentDock
        };

        // Create root dock
        var rootDock = new RootDock
        {
          Id = "Root",
          Title = "Root",
          VisibleDockables = new ObservableCollection<IDockable> { mainLayout },
          ActiveDockable = mainLayout
        };

        // Populate tools
        foreach (var toolModel in this.ToolPanes)
        {
            var tool = new Tool 
            { 
                Id = toolModel.ContentId, 
                Title = toolModel.Title,
                Content = toolModel 
            };
            toolDock.VisibleDockables.Add(tool);
        }

        // Assign factory and layout to dock host
        dockHost.Factory = factory;
        dockHost.Layout = rootDock;
        dockHost.InitializeFactory = true;
        dockHost.InitializeLayout = true;

        HookUpToolListeners(dockHost);

        Console.WriteLine("DockWorkspace.InitializeLayout: Dock layout initialized successfully");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"DockWorkspace.InitializeLayout error: {ex}");
      }
    }

    private void HookUpToolListeners(DockControl dockHost)
    {
        foreach (var toolModel in this.ToolPanes)
        {
            // Note: We are not unsubscribing here because we don't have the previous handler instance.
            // Assuming InitializeLayout is called once or we accept multiple subscriptions (which is bad).
            // Ideally we should track subscriptions.
            
            toolModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(ToolPaneModel.IsVisible))
                {
                    var tm = (ToolPaneModel)s;
                    if (tm.IsVisible)
                    {
                        ActivateTool(dockHost, tm.ContentId);
                    }
                }
            };
        }
    }

    private Dictionary<string, Dock.Model.Controls.ITool> _registeredTools = new();
    private Dictionary<string, Dock.Model.Core.IDockable> _registeredDockables = new();

    public void RegisterTool(Dock.Model.Controls.ITool tool)
    {
        if (tool.Id != null)
        {
            _registeredTools[tool.Id] = tool;
        }
    }

    public void RegisterDockable(Dock.Model.Core.IDockable dockable)
    {
        if (dockable.Id != null)
        {
            _registeredDockables[dockable.Id] = dockable;
        }
    }

    private void ActivateTool(DockControl dockHost, string contentId)
    {
        try
        {
            var factory = dockHost.Factory;
            var layout = dockHost.Layout;
            if (factory == null || layout == null) return;

            var tool = FindToolById(layout, contentId);
            if (tool != null)
            {
                factory.SetActiveDockable(tool);
                factory.SetFocusedDockable(layout, tool);
                Console.WriteLine($"DockWorkspace: Activated tool {contentId}");
            }
            else
            {
                // Try to find in registered tools
                if (_registeredTools.TryGetValue(contentId, out var registeredTool))
                {
                    // Determine target dock
                    string targetDockId = contentId == SearchPaneModel.PaneContentId ? "SearchDock" : "LeftDock";
                    var targetDock = FindDockById(layout, targetDockId);
                    
                    // If target dock not found in layout, try to find in registered dockables and insert it
                    if (targetDock == null && targetDockId == "SearchDock")
                    {
                        var rightDock = FindDockById(layout, "RightDock");
                        if (rightDock != null && rightDock.VisibleDockables != null)
                        {
                             if (_registeredDockables.TryGetValue("SearchDock", out var searchDock) && searchDock is Dock.Model.Controls.IToolDock sd)
                             {
                                 rightDock.VisibleDockables.Insert(0, sd);
                                 targetDock = sd;

                                 if (_registeredDockables.TryGetValue("SearchSplitter", out var splitter))
                                 {
                                     rightDock.VisibleDockables.Insert(1, splitter);
                                 }
                             }
                        }
                    }

                    if (targetDock is Dock.Model.Controls.IToolDock toolDock)
                    {
                        factory.AddDockable(toolDock, registeredTool);
                        factory.SetActiveDockable(registeredTool);
                        factory.SetFocusedDockable(layout, registeredTool);
                        Console.WriteLine($"DockWorkspace: Restored and activated tool {contentId} in {targetDockId}");
                        return;
                    }
                }

                Console.WriteLine($"DockWorkspace: Tool {contentId} not found for activation");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DockWorkspace: Error activating tool {contentId}: {ex}");
        }
    }

    private static Dock.Model.Controls.ITool? FindToolById(Dock.Model.Core.IDockable root, string id)
    {
      if (root is Dock.Model.Controls.ITool tool && tool.Id == id)
        return tool;

      if (root is Dock.Model.Core.IDock dock && dock.VisibleDockables != null)
      {
        foreach (var dockable in dock.VisibleDockables)
        {
          var found = FindToolById(dockable, id);
          if (found != null)
            return found;
        }
      }
      return null;
    }

    private static Dock.Model.Core.IDock? FindDockById(Dock.Model.Core.IDockable root, string id)
    {
      if (root is Dock.Model.Core.IDock dock)
      {
          if (dock.Id == id) return dock;
          
          if (dock.VisibleDockables != null)
          {
            foreach (var dockable in dock.VisibleDockables)
            {
              var found = FindDockById(dockable, id);
              if (found != null)
                return found;
            }
          }
      }
      return null;
    }
  }
}
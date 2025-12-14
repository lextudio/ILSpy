using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia;
using ICSharpCode.ILSpy.ViewModels;

namespace ProjectRover.Services
{
    /// <summary>
    /// Minimal scaffold implementation of ICSharpCode.ILSpy.IDockWorkspace for Avalonia.
    /// This is intentionally small and returns placeholder objects; extend as needed.
    /// </summary>
    public class AvaloniaDockWorkspace : ICSharpCode.ILSpy.Docking.IDockWorkspace
    {
        public IList<TabPageModel> TabPages { get; private set; } = Array.Empty<TabPageModel>();

        public IReadOnlyList<ToolPaneModel> ToolPanes { get; private set; } = Array.Empty<ToolPaneModel>();

        public TabPageModel? ActiveTabPage { get; set; }

        public TabPageModel AddTabPage(TabPageModel? tabPage = null)
        {
			// TODO:
            try
            {
                var app = Avalonia.Application.Current;
                var main = (app?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow
                           ?? Avalonia.Controls.TopLevel.GetTopLevel(null);
                //if (main == null)
                //    return tabPage ?? new object();

                // find DockControl named DockHost
                var dockHost = main.FindControl<Dock.Avalonia.Controls.DockControl>("DockHost");
                if (dockHost?.Factory == null)
                {
					ActiveTabPage = null; //tabPage ?? new object();
                    return ActiveTabPage!;
                }

                var factory = dockHost.Factory!;

                // create a document if none provided. Use concrete Document type
                // to avoid depending on different factory API surface versions.
                IDocument document;
                if (tabPage is IDocument d)
                {
                    document = d;
                }
                else
                {
                    document = new Dock.Model.Avalonia.Controls.Document();
                }

                // insert into the root layout if possible
                var layout = dockHost.Layout;
                if (layout is Dock.Model.Avalonia.Controls.ProportionalDock proportional && proportional.VisibleDockables != null)
                {
                    // insert at the last position of the documents dock
                    factory.InsertDockable(proportional, (Dock.Model.Core.IDockable)document, proportional.VisibleDockables.Count);
                }

                // set active
                factory.SetActiveDockable(document);

                // If caller expects an ILSpy TabPageModel-like object, wrap the document in our shim
                object returnObj = document;
                try
                {
                    if (!(tabPage is Dock.Model.Controls.IDocument))
                    {
                        // Create a TabPageModel shim and set its Content to a DecompilerPane hosted in the document
                        var tabType = Type.GetType("ICSharpCode.ILSpy.ViewModels.TabPageModel, ProjectRover")
                                      ?? Type.GetType("ICSharpCode.ILSpy.ViewModels.TabPageModel");
                        object? shim = null;
                        if (tabType != null)
                        {
                            try
                            {
                                shim = Activator.CreateInstance(tabType);
                            }
                            catch { shim = null; }
                        }

                        if (shim != null)
                        {
                            // Attempt to set Content on shim to the Document.Content (if any) or a new DecompilerPane
                            try
                            {
                                var mainWindowType = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime
                                    ? (Avalonia.Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow?.GetType()
                                    : null;

                                // Create DecompilerPane from ProjectRover.Views if available
                                var decompType = Type.GetType("ProjectRover.Views.DecompilerPane, ProjectRover") ?? Type.GetType("ProjectRover.Views.DecompilerPane");
                                object? decomp = null;
                                if (decompType != null)
                                {
                                    try { decomp = Activator.CreateInstance(decompType); } catch { decomp = null; }
                                }

                                var contentProp = shim.GetType().GetProperty("Content");
                                if (contentProp != null)
                                {
                                    if (decomp != null)
                                        contentProp.SetValue(shim, decomp);
                                    else
                                        contentProp.SetValue(shim, document);
                                }

                                // set title if available
                                var titleProp = shim.GetType().GetProperty("Title");
                                titleProp?.SetValue(shim, document.Title);

                                returnObj = shim;
                            }
                            catch
                            {
                                // ignore shim failures
                            }
                        }
                    }
                }
                catch
                {
                }

				ActiveTabPage = null; // returnObj;
                // refresh TabPages/ToolPanes from layout
                UpdateCollections(dockHost);

				return null; //				returnObj;
            }
            catch
            {
				ActiveTabPage = null; //		tabPage ?? new object();
                return ActiveTabPage!;
            }
        }

        public bool ShowToolPane(string contentId)
        {
            try
            {
                var app = Avalonia.Application.Current;
                var main = (app?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow
                           ?? Avalonia.Controls.TopLevel.GetTopLevel(null);
                if (main == null)
                    return false;

                var dockHost = main.FindControl<Dock.Avalonia.Controls.DockControl>("DockHost");
                var factory = dockHost?.Factory;
                if (factory == null)
                    return false;

                // find a tool pane with matching Id or Title
                var layout = dockHost.Layout;
                if (layout == null)
                    return false;

                // Search recursively for tool by Id or Title
                Dock.Model.Core.IDockable? found = DockSearchHelpers.FindByContentId(layout, contentId);
                if (found is Dock.Model.Controls.ITool tool)
                {
                    factory.SetActiveDockable(tool);
                    factory.SetFocusedDockable(layout, tool);
                    UpdateCollections(dockHost);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public void Remove(PaneModel model)
        {
            try
            {
                var app = Avalonia.Application.Current;
                var main = (app?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow
                           ?? Avalonia.Controls.TopLevel.GetTopLevel(null);
                if (main == null)
                    return;

                var dockHost = main.FindControl<Dock.Avalonia.Controls.DockControl>("DockHost");
                var factory = dockHost?.Factory;
                if (factory == null)
                {
                    if (ReferenceEquals(model, ActiveTabPage))
                        ActiveTabPage = null;
                    return;
                }

                if (model is Dock.Model.Core.IDockable dockable)
                {
                    factory.RemoveDockable(dockable, true);
                }
                else if (ReferenceEquals(model, ActiveTabPage))
                {
                    ActiveTabPage = null;
                }

                UpdateCollections(dockHost);
            }
            catch
            {
            }
        }

        public void InitializeLayout()
        {
            try
            {
                var app = Avalonia.Application.Current;
                var main = (app?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow
                           ?? Avalonia.Controls.TopLevel.GetTopLevel(null);
                if (main == null)
                    return;

                var dockHost = main.FindControl<Dock.Avalonia.Controls.DockControl>("DockHost");
                if (dockHost == null)
                    return;

                // Ensure factory and layout are initialized
                if (dockHost.Factory != null && dockHost.Layout != null)
                {
                    // update internal cached lists
                    UpdateCollections(dockHost);
                }
            }
            catch
            {
            }
        }

        public void ResetLayout()
        {
            try
            {
                var app = Avalonia.Application.Current;
                var main = (app?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow
                           ?? Avalonia.Controls.TopLevel.GetTopLevel(null);
                if (main == null)
                    return;

                var dockHost = main.FindControl<Dock.Avalonia.Controls.DockControl>("DockHost");
                var factory = dockHost?.Factory;
                if (factory == null)
                    return;

                // Attempt to reset by reinitializing factory/layout
                dockHost.InitializeFactory = true;
                dockHost.InitializeLayout = true;
                UpdateCollections(dockHost);
            }
            catch
            {
            }
        }

        public void CloseAllTabs()
        {
            try
            {
                var app = Avalonia.Application.Current;
                var main = (app?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow
                           ?? Avalonia.Controls.TopLevel.GetTopLevel(null);
                if (main == null)
                    return;

                var dockHost = main.FindControl<Dock.Avalonia.Controls.DockControl>("DockHost");
                var factory = dockHost?.Factory;
                if (factory == null)
                {
                    ActiveTabPage = null;
                    return;
                }

                // Close documents by removing them from layout
                var layout = dockHost.Layout;
                if (layout is Dock.Model.Avalonia.Controls.ProportionalDock proportional && proportional.VisibleDockables != null)
                {
                    var docs = proportional.VisibleDockables.OfType<Dock.Model.Controls.IDocument>().ToArray();
                    foreach (var d in docs)
                    {
                        factory.RemoveDockable(d, true);
                    }
                }

                ActiveTabPage = null;
                UpdateCollections(dockHost);
            }
            catch
            {
            }
        }

        public Task<T> RunWithCancellation<T>(Func<CancellationToken, Task<T>> taskCreation)
        {
            // Directly run the task without UI wrapping for now.
            return taskCreation(CancellationToken.None);
        }

        public void ShowText(string textOutput)
        {
            Console.WriteLine("showtext called");
            // Try to find the main window and set its Document to display the provided text.
            try
            {
                var app = Avalonia.Application.Current;
                if (app == null)
                    return;

                var main = (app.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow
                           ?? Avalonia.Controls.TopLevel.GetTopLevel(Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime ? null : null);
                if (main == null)
                {
                    // fallback: try to find any open Window via Application.Current?.ApplicationLifetime (non-classic lifetimes not supported here)
                    main = (app.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                }
                if (main == null)
                    return;

                if (main.DataContext is ProjectRover.ViewModels.MainWindowViewModel mwvm)
                {
                    string? textFallback = null;
                    switch (textOutput)
                    {
                        case string s:
                            textFallback = s;
                            break;
                        default:
                            try { textFallback = textOutput?.ToString(); } catch { textFallback = null; }
                            break;
                    }

                    if (textFallback != null)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            try
                            {
                                mwvm.Document = new AvaloniaEdit.Document.TextDocument(textFallback);
                            }
                            catch
                            {
                            }
                        });
                    }
                }
            }
            catch
            {
                // best-effort only
            }
        }

        private void UpdateCollections(Dock.Avalonia.Controls.DockControl dockHost)
        {
            try
            {
                var layout = dockHost.Layout;
                if (layout == null)
                    return;

                var docs = new List<TabPageModel>();
                var tools = new List<ToolPaneModel>();

                void Collect(IDock? d)
                {
                    if (d == null)
                        return;

                    if (d is Dock.Model.Controls.IDocumentDock dd && dd.VisibleDockables != null)
                    {
                        foreach (var v in dd.VisibleDockables)
                        {
                            // docs.Add(v);
                        }
                    }

                    if (d is Dock.Model.Controls.IToolDock td && td.VisibleDockables != null)
                    {
                        foreach (var v in td.VisibleDockables)
                        {
                            // tools.Add(v);
                        }
                    }

                    // traverse children if ProportionalDock etc.
                    if (d.VisibleDockables != null)
                    {
                        foreach (var v in d.VisibleDockables)
                        {
                            if (v is IDock childDock)
                                Collect(childDock);
                        }
                    }
                }

                Collect(layout);

                TabPages = docs;
                ToolPanes = tools.AsReadOnly();
            }
            catch
            {
            }
        }

        private static class DockSearchHelpers
        {
            public static Dock.Model.Core.IDockable? FindByContentId(Dock.Model.Core.IDock? dock, string contentId)
            {
                if (dock == null)
                    return null;

                if (dock is Dock.Model.Controls.IToolDock td && td.VisibleDockables != null)
                {
                    foreach (var v in td.VisibleDockables)
                    {
                        if (v is Dock.Model.Controls.ITool tool && (tool.Id == contentId || tool.Title == contentId))
                            return tool;
                    }
                }

                if (dock.VisibleDockables != null)
                {
                    foreach (var v in dock.VisibleDockables)
                    {
                        if (v is Dock.Model.Core.IDock dchild)
                        {
                            var found = FindByContentId(dchild, contentId);
                            if (found != null)
                                return found;
                        }
                        else if (v is Dock.Model.Controls.ITool tool && (tool.Id == contentId || tool.Title == contentId))
                        {
                            return tool;
                        }
                    }
                }

                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ICSharpCode.ILSpy.TextView;
using ICSharpCode.ILSpy.ViewModels;

namespace ICSharpCode.ILSpy.Docking
{
    /// <summary>
    /// Minimal docking workspace abstraction used by view-models to avoid direct WPF/AvalonDock coupling.
    /// Implementations may expose concrete types via <c>object</c> and provide adapters where necessary.
    /// </summary>
    public partial interface IDockWorkspace
    {
        IList<TabPageModel> TabPages { get; }

        IReadOnlyList<ToolPaneModel> ToolPanes { get; }

        TabPageModel? ActiveTabPage { get; set; }

        TabPageModel AddTabPage(TabPageModel? tabPage = null);

        bool ShowToolPane(string contentId);

        void Remove(PaneModel model);

        void InitializeLayout();

        void ResetLayout();

        void CloseAllTabs();

        Task<T> RunWithCancellation<T>(Func<CancellationToken, Task<T>> taskCreation);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ICSharpCode.ILSpy.Docking
{
    /// <summary>
    /// Minimal docking workspace abstraction used by view-models to avoid direct WPF/AvalonDock coupling.
    /// Implementations may expose concrete types via <c>object</c> and provide adapters where necessary.
    /// </summary>
    public interface IDockWorkspace
    {
        IReadOnlyList<object> TabPages { get; }

        IReadOnlyList<object> ToolPanes { get; }

        object? ActiveTabPage { get; set; }

        object AddTabPage(object? tabPage = null);

        bool ShowToolPane(string contentId);

        void Remove(object model);

        void InitializeLayout();

        void ResetLayout();

        void CloseAllTabs();

        Task<T> RunWithCancellation<T>(Func<CancellationToken, Task<T>> taskCreation);

        void ShowText(object textOutput);
    }
}

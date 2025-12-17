using System;
using System.Threading.Tasks;

using ICSharpCode.ILSpy.Docking;

namespace ICSharpCode.ILSpy
{
    public interface IPlatformService
    {
        void InvokeOnUI(Action action);
        Task InvokeOnUIAsync(Func<Task> action);
        bool TryFindResource(object key, out object? value);
        DockWorkspace? DockWorkspace { get; }

        Task SetTextClipboardAsync(string text);
    }
}

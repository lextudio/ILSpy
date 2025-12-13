using System;
using System.Threading.Tasks;

namespace ICSharpCode.ILSpy
{
    public interface IPlatformService
    {
        void InvokeOnUI(Action action);
        Task InvokeOnUIAsync(Func<Task> action);
        bool TryFindResource(object key, out object? value);
        Docking.IDockWorkspace? DockWorkspace { get; }
    }
}

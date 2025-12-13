using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ICSharpCode.ILSpy
{
    public class WpfPlatformService : IPlatformService
    {
        public Docking.IDockWorkspace? DockWorkspace { get; set; }

        public void InvokeOnUI(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }

        public Task InvokeOnUIAsync(Func<Task> action)
        {
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            if (dispatcher.CheckAccess())
                return action();
            var tcs = new TaskCompletionSource<object?>();
            dispatcher.BeginInvoke(new Action(async () =>
            {
                try
                {
                    await action().ConfigureAwait(false);
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return tcs.Task;
        }

        public bool TryFindResource(object key, out object? value)
        {
            if (Application.Current == null)
            {
                value = null;
                return false;
            }
			value = null;
            return Application.Current.TryFindResource(key) is { } v && (value = v) != null;
        }
    }
}

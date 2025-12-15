using System;

using Avalonia.Media;
using Avalonia.Threading;

namespace System.Windows.Threading
{
    public enum DispatcherPriority
    {
        Background,
        Normal,
        Loaded,
        // minimal set used by code
    }

    public class Dispatcher
    {
        // BeginInvoke overload used in code: BeginInvoke(DispatcherPriority, Action)
        public void BeginInvoke(DispatcherPriority priority, Action callback)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(callback, Translate(priority));
        }

		private static Avalonia.Threading.DispatcherPriority Translate(DispatcherPriority priority)
		{
            return priority switch
            {
                DispatcherPriority.Background => Avalonia.Threading.DispatcherPriority.Background,
                DispatcherPriority.Loaded => Avalonia.Threading.DispatcherPriority.Loaded,
                _ => Avalonia.Threading.DispatcherPriority.Normal,
            };
		}

		public void BeginInvoke(Action callback)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(callback);
        }

        public void Invoke(Action callback)
        {
            callback();
        }
    }

    // public class DispatcherTimer
    // {
    //     private Timer? timer;
    //     private readonly DispatcherPriority priority;
    //     private readonly Action callback;

    //     public DispatcherTimer(DispatcherPriority priority, Action callback)
    //     {
    //         this.priority = priority;
    //         this.callback = callback;
    //     }

    //     public void Start()
    //     {
    //         timer = new Timer(_ => callback(), null, 0, Timeout.Infinite);
    //     }

    //     public void Stop()
    //     {
    //         timer?.Dispose();
    //         timer = null;
    //     }
    // }
}

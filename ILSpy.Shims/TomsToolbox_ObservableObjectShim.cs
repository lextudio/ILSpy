// Minimal shim for TomsToolbox.Wpf.ObservableObjectBase used by linked ILSpy sources
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using Avalonia.Threading;

namespace TomsToolbox.Wpf
{
    public class ObservableObjectBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ObservableObject : ObservableObjectBase
    {
		/// <summary>
		/// Gets the dispatcher of the thread where this object was created.
		/// </summary>
		public Dispatcher Dispatcher { get; } = Dispatcher.UIThread; // TODO: verify Avalonia compatibility
	}

	public static class DispatcherHelper
	{
		public static void BeginInvoke(Action action)
		{
			Dispatcher.UIThread.Invoke(action);
		}

		public static void BeginInvoke(string priority, Action action)
		{
			Dispatcher.UIThread.Post(action, Enum.Parse<DispatcherPriority>(priority));
		}

		public static DispatcherOperation InvokeAsync(Action action)
		{
			return Dispatcher.UIThread.InvokeAsync(action);
		}
	}
}

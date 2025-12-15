// Minimal shim for TomsToolbox.Wpf.ObservableObjectBase used by linked ILSpy sources
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			if (propertyName == null)
				return;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

			// raise for dependent properties
			foreach (var dependent in GetDependentProperties(propertyName))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(dependent));
			}
		}

		static readonly ConcurrentDictionary<Type, Dictionary<string, string[]>> _cache = new();

		static IReadOnlyList<string> GetDependentProperties(string propertyName, Type? type = null)
		{
			type ??= typeof(ObservableObject);

			var map = _cache.GetOrAdd(type, t =>
			{
				var dict = new Dictionary<string, List<string>>();

				foreach (var prop in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					var attrs = prop.GetCustomAttributes<PropertyDependencyAttribute>();
					foreach (var attr in attrs)
					{
						foreach (var source in attr.PropertyNames)
						{
							if (!dict.TryGetValue(source, out var list))
								list = dict[source] = new List<string>();

							list.Add(prop.Name);
						}
					}
				}

				return dict.ToDictionary(k => k.Key, v => v.Value.ToArray());
			});

			return map.TryGetValue(propertyName, out var deps) ? deps : Array.Empty<string>();
		}
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

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class PropertyDependencyAttribute : Attribute
	{
		public PropertyDependencyAttribute(params string[] propertyNames)
		{
			PropertyNames = propertyNames;
		}

		public string[] PropertyNames { get; }
	}
}

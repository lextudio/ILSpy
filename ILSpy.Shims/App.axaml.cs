/*
    Copyright 2024 CodeMerx
    Copyright 2025 LeXtudio Inc.
    This file is part of ProjectRover.

    ProjectRover is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ProjectRover is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with ProjectRover.  If not, see<https://www.gnu.org/licenses/>.
*/

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProjectRover.Extensions;
using ProjectRover.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Composition.Convention;
using TomsToolbox.Composition;
using TomsToolbox.Composition.MicrosoftExtensions;
using ICSharpCode.ILSpyX.Analyzers;
using TomsToolbox.Composition;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using ICSharpCode.ILSpy.Views;
using ICSharpCode.ILSpy.AppEnv;

namespace ProjectRover;

public partial class App : Application
{
    public new static App Current => (App)Application.Current!;
    
    public IServiceProvider Services { get; private set; } = null!;
    public object? CompositionHost { get; private set; }
    public static IExportProvider? ExportProvider { get; private set; }

    public static CommandLineArguments CommandLineArguments { get; private set; } = CommandLineArguments.Create(Array.Empty<string>()); // TODO:
    internal static readonly IList<ExceptionData> StartupExceptions = new List<ExceptionData>(); // TODO:
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = CreateServiceCollection();

            // Initialize SettingsService
            var settingsService = new ICSharpCode.ILSpy.Util.SettingsService();
            services.AddSingleton(settingsService);

            // Bind exports from assemblies
            // ILSpyX
            Console.WriteLine("Binding exports from ILSpyX...");
            services.BindExports(typeof(IAnalyzer).Assembly);
            // ILSpy (Original)
            Console.WriteLine("Binding exports from ILSpy...");
            services.BindExports(typeof(ICSharpCode.ILSpy.Views.MainWindow).Assembly);
            // ILSpy.Shims (Rover)
            Console.WriteLine("Binding exports from ILSpy.Shims...");
            services.BindExports(Assembly.GetExecutingAssembly());

            // Add the export provider (circular dependency resolution via factory)
            services.AddSingleton<IExportProvider>(sp => ExportProvider!);

            Console.WriteLine("Building ServiceProvider...");
            var serviceProvider = services.BuildServiceProvider();
            Services = serviceProvider;

            // Create the adapter
            Console.WriteLine("Creating ExportProviderAdapter...");
            ExportProvider = new ExportProviderAdapter(serviceProvider);
            
            Console.WriteLine($"ExportProvider initialized: {ExportProvider != null}");

            Console.WriteLine("Creating MainWindow...");
            desktop.MainWindow = Services.GetRequiredService<ICSharpCode.ILSpy.Views.MainWindow>();
            Console.WriteLine("MainWindow created.");

            // Exercise docking workspace once at startup (diagnostic)
            try
            {
                var dockWorkspace = Services.GetService<ICSharpCode.ILSpy.Docking.IDockWorkspace>();
                if (dockWorkspace != null)
                {
                    // Add a diagnostic tab and show some text
                    var doc = dockWorkspace.AddTabPage(null);
                    dockWorkspace.ShowText(null);//  TODO: "ProjectRover: diagnostic tab created at startup.");
                }
            }
            catch
            {
                // swallow diagnostic errors
            }

                // Runtime MEF diagnostics: try to resolve the exported IlSpy backend wrapper
                try
                {
                    if (ExportProvider != null)
                    {
                        try
                        {
                            // var exportedBackend = ExportProvider.GetExportedValue<ProjectRover.Services.ExportedIlSpyBackend>();
                            // if (exportedBackend != null && exportedBackend.Backend != null)
                            // {
                            //     Console.WriteLine("MEF: Resolved ExportedIlSpyBackend via ExportProvider.");
                            //     try
                            //     {
                            //         // Call a safe method to verify the backend is callable
                            //         exportedBackend.Backend.Clear();
                            //         Console.WriteLine("IlSpyBackend.Clear() invoked successfully.");
                            //     }
                            //     catch (Exception ex)
                            //     {
                            //         Console.WriteLine("IlSpyBackend.Clear failed: " + ex.Message);
                            //     }
                            // }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("MEF: ExportedIlSpyBackend not resolved via ExportProvider: " + ex.Message);
                        }
                    }
                }
                catch
                {
                    // swallow diagnostics
                }

            desktop.ShutdownRequested += (_, _) =>
            {
                // var analyticsService = Services.GetRequiredService<IAnalyticsService>();
                // try
                // {
                //     analyticsService.TrackEvent(AnalyticsEvents.Shutdown);
                // }
                // catch { }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private static IServiceCollection CreateServiceCollection() =>
        new ServiceCollection()
            .ConfigureOptions()
            .ConfigureLogging()
            .AddViews()
            .AddViewModels()
            .AddServices()
            .AddProviders()
            .AddHttpClients();

    private void About_OnClick(object? sender, EventArgs e)
    {
        //_ = Services.GetRequiredService<IAnalyticsService>().TrackEventAsync(AnalyticsEvents.About);
        //Services.GetRequiredService<IDialogService>().ShowDialog<AboutDialog>();
    }

    // Small adapter so existing ILSpy code can call GetExportedValue<T>() against a provider.
    class CompositionHostExportProvider : IExportProvider
    {
        private readonly CompositionHost _host;
        private readonly IServiceProvider _services;

        public CompositionHostExportProvider(CompositionHost host, IServiceProvider services)
        {
            _host = host;
            _services = services;
        }

		public event EventHandler<EventArgs>? ExportsChanged;

		public T GetExportedValue<T>()
        {
            if (_host.TryGetExport<T>(out var export))
                return export;

            // Fallback to service provider
            var svc = (T?)_services.GetService(typeof(T));
            if (svc != null)
                return svc;
            throw new InvalidOperationException($"Export not found: {typeof(T).FullName}");
        }

		public T GetExportedValue<T>(string? contractName = null) where T : class
		{
            if (_host.TryGetExport<T>(contractName, out var export))
                return export;

            if (contractName == null)
            {
                var svc = (T?)_services.GetService(typeof(T));
                if (svc != null)
                    return svc;
            }
            throw new InvalidOperationException($"Export not found: {typeof(T).FullName} (contract: {contractName})");
		}

		public T? GetExportedValueOrDefault<T>(string? contractName = null) where T : class
		{
            if (_host.TryGetExport<T>(contractName, out var export))
                return export;

            if (contractName == null)
            {
                var svc = (T?)_services.GetService(typeof(T));
                if (svc != null) return svc;
            }
            return default;
		}

		public T[] GetExportedValues<T>()
        {
            return System.Linq.Enumerable.ToArray(_host.GetExports<T>());
        }

		public IEnumerable<T> GetExportedValues<T>(string? contractName = null) where T : class
		{
			return _host.GetExports<T>(contractName);
		}

		public IEnumerable<object> GetExportedValues(Type contractType, string? contractName = null)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IExport<object>> GetExports(Type contractType, string? contractName = null)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IExport<T>> GetExports<T>(string? contractName = null) where T : class
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IExport<T, TMetadataView>> GetExports<T, TMetadataView>(string? contractName = null)
			where T : class
			where TMetadataView : class
		{
			throw new NotImplementedException();
		}

		public bool TryGetExportedValue<T>(string? contractName, [NotNullWhen(true)] out T? value) where T : class
		{
			throw new NotImplementedException();
		}
	}

    // Small holder type that can be discovered by MEF consumers if they import IServiceProvider
    // We keep this type internal to avoid adding new public API surface.
    class ProjectRoverExportedServiceProvider
    {
        private readonly IServiceProvider _provider;

        public ProjectRoverExportedServiceProvider(IServiceProvider provider)
        {
            _provider = provider;
        }

        public object GetService(Type serviceType) => _provider.GetService(serviceType)!;
    }
}

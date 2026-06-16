// Copyright (c) 2017 AlphaSierraPapa for the SharpDevelop Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Composition;
using System.Xml.Linq;

#if !ROMA_UNO
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.ILSpy.AppEnv;
using Microsoft.Win32;
#else
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
#endif

using ICSharpCode.ILSpyX.Settings;

using TomsToolbox.Wpf;

namespace ICSharpCode.ILSpy.Options
{
	[ExportOptionPage(Order = 30)]
	[NonShared]
	public class MiscSettingsViewModel : ObservableObjectBase, IOptionPage
	{
		private MiscSettings settings;
		public MiscSettings Settings {
			get => settings;
			set => SetProperty(ref settings, value);
		}

#if !ROMA_UNO
		public ICommand AddRemoveShellIntegrationCommand => new DelegateCommand(() => AppEnvironment.IsWindows, AddRemoveShellIntegration);

		const string rootPath = @"Software\Classes\{0}\shell";
		const string fullPath = @"Software\Classes\{0}\shell\Open with ILSpy\command";

		private void AddRemoveShellIntegration()
		{
			string commandLine = CommandLineTools.ArgumentArrayToCommandLine(Path.ChangeExtension(Assembly.GetEntryAssembly()?.Location, ".exe")) + " \"%L\"";
			if (RegistryEntriesExist())
			{
				if (MessageBox.Show(string.Format(Properties.Resources.RemoveShellIntegrationMessage, commandLine), "ILSpy", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					Registry.CurrentUser
						.CreateSubKey(string.Format(rootPath, "dllfile"))?
						.DeleteSubKeyTree("Open with ILSpy");
					Registry.CurrentUser
						.CreateSubKey(string.Format(rootPath, "exefile"))?
						.DeleteSubKeyTree("Open with ILSpy");
				}
			}
			else
			{
				if (MessageBox.Show(string.Format(Properties.Resources.AddShellIntegrationMessage, commandLine), "ILSpy", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					Registry.CurrentUser
						.CreateSubKey(string.Format(fullPath, "dllfile"))?
						.SetValue("", commandLine);
					Registry.CurrentUser
						.CreateSubKey(string.Format(fullPath, "exefile"))?
						.SetValue("", commandLine);
				}
			}
			OnPropertyChanged(nameof(AddRemoveShellIntegrationText));
		}

		private static bool RegistryEntriesExist()
		{
			return Registry.CurrentUser.OpenSubKey(string.Format(fullPath, "dllfile")) != null
				&& Registry.CurrentUser.OpenSubKey(string.Format(fullPath, "exefile")) != null;
		}

		public string AddRemoveShellIntegrationText {
			get {
				return RegistryEntriesExist() ? Properties.Resources.RemoveShellIntegration : Properties.Resources.AddShellIntegration;
			}
		}
#else
		const string WindowsRootPath = @"Software\Classes\{0}\shell";
		const string WindowsFullPath = @"Software\Classes\{0}\shell\Open with Roma\command";
		const string LinuxDesktopFile = ".local/share/applications/roma.desktop";

		public bool IsShellIntegrationSupported =>
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
			RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

		public bool IsShellIntegrationInstalled => CheckShellIntegration();

		public string AddRemoveShellIntegrationText =>
			IsShellIntegrationInstalled
				? Properties.Resources.RemoveShellIntegration
				: Properties.Resources.AddShellIntegration;

		public string ShellIntegrationDescription
		{
			get {
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return string.Format(
						IsShellIntegrationInstalled
							? Properties.Resources.RemoveShellIntegrationMessage
							: Properties.Resources.AddShellIntegrationMessage,
						GetExePath() + " \"%L\"");
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					return IsShellIntegrationInstalled
						? $"Removes {LinuxDesktopFile} and unregisters file associations."
						: $"Creates {LinuxDesktopFile} and registers .dll/.exe/.winmd associations.";
				return "Shell integration is managed by the application installer on this platform.";
			}
		}

		private static bool CheckShellIntegration()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return Microsoft.Win32.Registry.CurrentUser.OpenSubKey(string.Format(WindowsFullPath, "dllfile")) != null
					&& Microsoft.Win32.Registry.CurrentUser.OpenSubKey(string.Format(WindowsFullPath, "exefile")) != null;
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), LinuxDesktopFile);
				return File.Exists(path);
			}
			return false;
		}

		public void ApplyShellIntegration()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				ApplyWindowsShellIntegration();
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				ApplyLinuxShellIntegration();
			OnPropertyChanged(nameof(IsShellIntegrationInstalled));
			OnPropertyChanged(nameof(AddRemoveShellIntegrationText));
			OnPropertyChanged(nameof(ShellIntegrationDescription));
		}

		private void ApplyWindowsShellIntegration()
		{
			if (CheckShellIntegration())
			{
				foreach (var ext in new[] { "dllfile", "exefile", "winmdfile" })
					Microsoft.Win32.Registry.CurrentUser
						.CreateSubKey(string.Format(WindowsRootPath, ext))?
						.DeleteSubKeyTree("Open with Roma");
			}
			else
			{
				var cmd = GetExePath() + " \"%L\"";
				foreach (var ext in new[] { "dllfile", "exefile", "winmdfile" })
					Microsoft.Win32.Registry.CurrentUser
						.CreateSubKey(string.Format(WindowsFullPath, ext))?
						.SetValue("", cmd);
			}
		}

		private void ApplyLinuxShellIntegration()
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), LinuxDesktopFile);
			if (CheckShellIntegration())
			{
				File.Delete(path);
				RemoveLinuxMimeAssociation();
			}
			else
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				var exe = GetExePath();
				File.WriteAllText(path,
					$"[Desktop Entry]\nType=Application\nName=Roma\nExec={exe} %f\n" +
					$"MimeType=application/x-ms-dos-executable;application/octet-stream;application/x-winmd;\n" +
					$"Categories=Development;\nNoDisplay=false\n");
				AddLinuxMimeAssociation();
			}
		}

		private static void AddLinuxMimeAssociation()
		{
			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("xdg-mime") {
					ArgumentList = { "default", "roma.desktop", "application/x-ms-dos-executable" },
					UseShellExecute = false,
					CreateNoWindow = true,
				})?.WaitForExit(3000);
			}
			catch { }
		}

		private static void RemoveLinuxMimeAssociation()
		{
			var mimeapps = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
				".config", "mimeapps.list");
			if (!File.Exists(mimeapps)) return;
			var lines = File.ReadAllLines(mimeapps);
			File.WriteAllLines(mimeapps, System.Linq.Enumerable.Where(lines, l => !l.Contains("roma.desktop")));
		}

		private static string GetExePath()
		{
			var path = Environment.ProcessPath ?? Assembly.GetEntryAssembly()?.Location ?? string.Empty;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				path = Path.ChangeExtension(path, ".exe");
			return path;
		}
#endif

		public string Title => Properties.Resources.Misc;

		public void Load(SettingsSnapshot settings)
		{
			Settings = settings.GetSettings<MiscSettings>();
		}

		public void LoadDefaults()
		{
			Settings.LoadFromXml(new XElement("dummy"));
		}
	}
}

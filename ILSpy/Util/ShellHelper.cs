// Copyright (c) 2025 sonyps5201314
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

#pragma warning disable CA1060 // Move pinvokes to native methods class

namespace ICSharpCode.ILSpy.Util
{
	static class ShellHelper
	{
		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut);

		[DllImport("shell32.dll")]
		static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

		[DllImport("shell32.dll")]
		static extern IntPtr ILFindLastID(IntPtr pidl);

		[DllImport("ole32.dll")]
		static extern void CoTaskMemFree(IntPtr pv);

		public static void OpenFolder(string folderPath)
		{
			try
			{
				if (string.IsNullOrEmpty(folderPath))
					return;

				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					Process.Start(new ProcessStartInfo("open") { ArgumentList = { folderPath }, UseShellExecute = false });
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					Process.Start(new ProcessStartInfo("xdg-open") { ArgumentList = { folderPath }, UseShellExecute = false });
				else
					Process.Start(new ProcessStartInfo { FileName = folderPath, UseShellExecute = true });
			}
			catch (Exception)
			{
				// Process.Start can throw several errors (not all of them documented),
				// just ignore all of them.
			}
		}

		public static void OpenFolderAndSelectItem(string path)
		{
			if (string.IsNullOrEmpty(path))
				return;
			if (Directory.Exists(path))
			{
				OpenFolder(path);
				return;
			}

			if (!File.Exists(path))
				return;

			// Reveal-and-select the file. Windows uses the shell32 multi-item path below;
			// macOS reveals in Finder; Linux opens the containing folder (no portable select).
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				try
				{
					Process.Start(new ProcessStartInfo("open") { ArgumentList = { "-R", path }, UseShellExecute = false });
				}
				catch (Exception)
				{
					OpenFolder(Path.GetDirectoryName(path)!);
				}
				return;
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				OpenFolder(Path.GetDirectoryName(path)!);
				return;
			}

			OpenFolderAndSelectItems(path);
		}

		public static void OpenFolderAndSelectItems(params IEnumerable<string> paths)
		{
			if (paths == null)
				return;

			// Non-Windows: the shell32 select API is unavailable, reveal each item individually.
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				foreach (var p in paths)
					OpenFolderAndSelectItem(p);
				return;
			}

			// Group by containing folder
			var files = paths.Distinct(StringComparer.OrdinalIgnoreCase).Where(p => !string.IsNullOrEmpty(p) && File.Exists(p)).ToList();
			if (files.Count == 0)
				return;

			var itemPidlAllocs = new List<IntPtr>();
			var relativePidls = new List<IntPtr>();

			foreach (var group in files.GroupBy(Path.GetDirectoryName))
			{
				string folder = group.Key;
				if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
					continue;

				IntPtr folderPidl = IntPtr.Zero;

				try
				{
					int hrFolder = SHParseDisplayName(folder, IntPtr.Zero, out folderPidl, 0, out uint attrs);
					Marshal.ThrowExceptionForHR(hrFolder);

					foreach (var file in group)
					{
						int hrItem = SHParseDisplayName(file, IntPtr.Zero, out var itemPidl, 0, out attrs);
						if (hrItem == 0 && itemPidl != IntPtr.Zero)
						{
							IntPtr relative = ILFindLastID(itemPidl);
							if (relative != IntPtr.Zero)
							{
								relativePidls.Add(relative);
								itemPidlAllocs.Add(itemPidl);
								continue;
							}
						}
						if (itemPidl != IntPtr.Zero)
							CoTaskMemFree(itemPidl);
					}

					if (relativePidls.Count > 0)
					{
						int hr = SHOpenFolderAndSelectItems(folderPidl, (uint)relativePidls.Count, relativePidls.ToArray(), 0);
						Marshal.ThrowExceptionForHR(hr);
					}
					else
					{
						// nothing to select - open folder
						OpenFolder(folder);
					}
				}
				catch (Exception ex) when (ex is COMException or Win32Exception)
				{
					// fall back to Process.Start
					OpenFolder(folder);
				}
				finally
				{
					foreach (var p in itemPidlAllocs)
						CoTaskMemFree(p);
					if (folderPidl != IntPtr.Zero)
						CoTaskMemFree(folderPidl);

					itemPidlAllocs.Clear();
					relativePidls.Clear();
				}
			}
		}
	}
}

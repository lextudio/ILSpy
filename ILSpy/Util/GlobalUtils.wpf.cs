// Copyright (c) 2024 Tom Englert for the SharpDevelop Team
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ICSharpCode.ILSpy.Util
{
	partial class GlobalUtils
	{
		// Cross-platform "open command line here" (adapted from ProjectRover). The original
		// WPF implementation hard-coded cmd.exe, which fails on macOS/Linux where Roma runs.
		public static void OpenTerminalAt(string path)
		{
			try
			{
				if (string.IsNullOrEmpty(path))
					return;
				path = Path.GetFullPath(path);

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					ExecuteCommand("cmd.exe", $"/k \"cd /d {path}\"");
					return;
				}

				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					// Launch Terminal.app at the folder via AppleScript.
					RunAppleScript(TerminalScript, path);
					return;
				}

				TryLaunchLinuxTerminal(path);
			}
			catch
			{
				// Process.Start can throw various (often undocumented) errors; ignore.
			}
		}

		static void TryLaunchLinuxTerminal(string path)
		{
			var candidates = new[]
			{
				new[] { "gnome-terminal", $"--working-directory={path}" },
				new[] { "konsole", $"--workdir {path}" },
				new[] { "xfce4-terminal", $"--working-directory={path}" },
				new[] { "x-terminal-emulator", $"-e bash -lc \"cd '{path}'; exec bash\"" },
				new[] { "xterm", $"-e bash -lc \"cd '{path}'; exec bash\"" },
			};

			foreach (var candidate in candidates)
			{
				try
				{
					Process.Start(new ProcessStartInfo { FileName = candidate[0], Arguments = candidate[1], UseShellExecute = false });
					return;
				}
				catch
				{
					// try the next emulator
				}
			}
		}

		static void RunAppleScript(string script, params string[] args)
		{
			var psi = new ProcessStartInfo { FileName = "osascript", UseShellExecute = false };
			psi.ArgumentList.Add("-e");
			psi.ArgumentList.Add(script);
			if (args.Length > 0)
			{
				psi.ArgumentList.Add("--");
				foreach (var arg in args)
					psi.ArgumentList.Add(arg);
			}

			Process.Start(psi);
		}

		const string TerminalScript = "on run argv\n"
			+ "set targetPath to item 1 of argv\n"
			+ "tell application \"Terminal\"\n"
			+ "do script \"cd \" & quoted form of targetPath & \"; clear\"\n"
			+ "activate\n"
			+ "end tell\n"
			+ "end run";
	}
}
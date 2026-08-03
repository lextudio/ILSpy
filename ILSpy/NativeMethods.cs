// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
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
using System.Runtime.InteropServices;

namespace ICSharpCode.ILSpy
{
	// Uses https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation
	internal static partial class NativeMethods
	{
		const int S_OK = 0;

		[LibraryImport("user32.dll")]
		internal static partial int GetDoubleClickTime();

		[LibraryImport("dwmapi.dll")]
		internal static partial int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);

		public static bool UseImmersiveDarkMode(IntPtr hWnd, bool enable)
		{
			int darkMode = enable ? 1 : 0;
			int hResult = DwmSetWindowAttribute(hWnd, DwmWindowAttribute.UseImmersiveDarkMode, ref darkMode, sizeof(int));
			return hResult > S_OK;
		}
	}

	// internal (not public) for the hosted build: this file compiles into BOTH ILSpyAddIn.dll and
	// the shared ICSharpCode.TreeView.dll, and a public enum of the same name in the same
	// namespace exported by two assemblies trips CS0436 in every project referencing both.
	internal enum DwmWindowAttribute : uint
	{
		NCRenderingEnabled = 1,
		NCRenderingPolicy,
		TransitionsForceDisabled,
		AllowNCPaint,
		CaptionButtonBounds,
		NonClientRtlLayout,
		ForceIconicRepresentation,
		Flip3DPolicy,
		ExtendedFrameBounds,
		HasIconicBitmap,
		DisallowPeek,
		ExcludedFromPeek,
		Cloak,
		Cloaked,
		FreezeRepresentation,
		PassiveUpdateMode,
		UseHostBackdropBrush,
		UseImmersiveDarkMode = 20,
		WindowCornerPreference = 33,
		BorderColor,
		CaptionColor,
		TextColor,
		VisibleFrameBorderThickness,
		SystemBackdropType,
		Last
	}
}

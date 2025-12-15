// this file contains the WPF-specific part of AssemblyTreeModel
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using ICSharpCode.ILSpy.AssemblyTree;
using ICSharpCode.ILSpy.Docking;
using ICSharpCode.ILSpy.TextView;
using ICSharpCode.ILSpy.Updates;
using ICSharpCode.ILSpy.Util;
using ICSharpCode.ILSpy.ViewModels;
using ICSharpCode.ILSpyX;
using ICSharpCode.ILSpyX.TreeView;

using ICSharpCode.ILSpy.Views;

using TomsToolbox.Composition;

using ProjectRover;

namespace ICSharpCode.ILSpy.AssemblyTree
{
    public partial class AssemblyTreeModel
    {
		public AssemblyTreeModel(SettingsService settingsService, LanguageService languageService, IExportProvider exportProvider)
		{
			// TODO:
		}

		public async Task HandleSingleInstanceCommandLineArguments(string[] args)
		{
			// TODO:
		}

		private static void LoadInitialAssemblies(AssemblyList assemblyList)
		{
			// Called when loading an empty assembly list; so that
			// the user can see something initially.
			System.Reflection.Assembly[] initialAssemblies = {
				typeof(object).Assembly,
				typeof(Uri).Assembly,
				typeof(System.Linq.Enumerable).Assembly,
				typeof(System.Xml.XmlDocument).Assembly,
				typeof(Avalonia.Markup.Xaml.MarkupExtension).Assembly,
				typeof(Avalonia.Rect).Assembly,
				typeof(Avalonia.Controls.Control).Assembly,
				typeof(Avalonia.Controls.UserControl).Assembly
			};
			//foreach (System.Reflection.Assembly asm in initialAssemblies)
				// TODO: assemblyList.OpenAssembly(asm.Location);
		}
	}
}
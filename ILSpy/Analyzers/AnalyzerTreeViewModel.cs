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
using System.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpy.Analyzers.TreeNodes;
using ICSharpCode.ILSpy.AssemblyTree;
using ICSharpCode.ILSpy.TreeNodes;
using ICSharpCode.ILSpyX.TreeView;

using TomsToolbox.Wpf;

namespace ICSharpCode.ILSpy.Analyzers
{
	// SharpTreeView duplicate resolution / host-neutral pane model vertical slice
	// (doc/technotes/ilspy.md "Immediate next actions" #3, 2026-08-02): derives directly from
	// OpenDevelop's ICSharpCode.SharpDevelop.ViewModels.ToolPaneModel instead of ILSpy's own
	// ICSharpCode.ILSpy.ViewModels.ToolPaneModel, mirroring the SearchPaneModel migration earlier
	// in this pass. [ExportToolPane] (contract type ICSharpCode.ILSpy.ViewModels.ToolPaneModel)
	// is dropped since the bare [Export] already exports this concrete type - IlSpyWorkspaceHost
	// resolves it via exportProvider.GetExportedValue<AnalyzerTreeViewModel>(), not the "ToolPane"
	// contract (whose only consumer is ILSpy's own unused Docking/DockWorkspace.cs).
	[Shared]
	[Export]
	public class AnalyzerTreeViewModel : ICSharpCode.SharpDevelop.ViewModels.ToolPaneModel
	{
		public const string PaneContentId = "analyzerPane";

		public AnalyzerTreeViewModel(AssemblyTreeModel assemblyTreeModel)
		{
			ContentId = PaneContentId;
			Title = Properties.Resources.Analyze;
			ShortcutKey = new(Key.R, ModifierKeys.Control);
			AssociatedCommand = new AnalyzeCommand(assemblyTreeModel, this);
			// OpenDevelop's ToolPaneModel renders Content via implicit DataTemplate lookup on its
			// runtime type - IlSpyToolPaneAdapter used to set this (Content = the raw view-model);
			// setting it directly here now that this class IS the OpenDevelop ToolPaneModel.
			Content = this;
		}

		public AnalyzerRootNode Root { get; } = new();

		public ICommand AnalyzeCommand => new DelegateCommand(AnalyzeSelected);

		private SharpTreeNode[] selectedItems = [];

		public SharpTreeNode[] SelectedItems {
			get => selectedItems ?? [];
			set {
				if (SelectedItems.SequenceEqual(value))
					return;

				selectedItems = value;
				OnPropertyChanged();
			}
		}

		private void AnalyzeSelected()
		{
			foreach (var node in SelectedItems.OfType<IMemberTreeNode>())
			{
				Analyze(node.Member);
			}
		}

		void AddOrSelect(AnalyzerTreeNode node)
		{
			Show();

			AnalyzerTreeNode target = default;

			if (node is AnalyzerEntityTreeNode { Member: { } member })
			{
				target = this.Root.Children.OfType<AnalyzerEntityTreeNode>().FirstOrDefault(item => item.Member == member);
			}

			if (target == null)
			{
				this.Root.Children.Add(node);
				target = node;
			}

			target.IsExpanded = true;
			this.SelectedItems = [target];
		}

		public void Analyze(IEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (entity.MetadataToken.IsNil)
			{
				MessageBox.Show(Properties.Resources.CannotAnalyzeMissingRef, "ILSpy");
				return;
			}

			switch (entity)
			{
				case ITypeDefinition td:
					AddOrSelect(new AnalyzedTypeTreeNode(td, null));
					break;
				case IField fd:
					if (!fd.IsConst)
						AddOrSelect(new AnalyzedFieldTreeNode(fd, null));
					break;
				case IMethod md:
					AddOrSelect(new AnalyzedMethodTreeNode(md, null));
					break;
				case IProperty pd:
					AddOrSelect(new AnalyzedPropertyTreeNode(pd, null));
					break;
				case IEvent ed:
					AddOrSelect(new AnalyzedEventTreeNode(ed, null));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(entity), $@"Entity {entity.GetType().FullName} is not supported.");
			}
		}
	}
}


// Copyright (c) 2019 AlphaSierraPapa for the SharpDevelop Team
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;
using ICSharpCode.ILSpy.AppEnv;
using ICSharpCode.ILSpy.Properties;
using ICSharpCode.ILSpy.TextView;
using ICSharpCode.ILSpy.TreeNodes;
using ICSharpCode.ILSpy.Updates;
using ICSharpCode.ILSpy.Util;
using ICSharpCode.ILSpy.ViewModels;
using ICSharpCode.ILSpyX;
using ICSharpCode.ILSpyX.TreeView;

using TomsToolbox.Composition;
using TomsToolbox.Essentials;
using TomsToolbox.Wpf;

#nullable enable


namespace ICSharpCode.ILSpy.AssemblyTree
{
	[ExportToolPane]
	[Shared]
	public partial class AssemblyTreeModel : ToolPaneModel
	{
		public const string PaneContentId = "assemblyListPane";

		private AssemblyListTreeNode? assemblyListTreeNode;
		private readonly DispatcherThrottle refreshThrottle;

		private readonly NavigationHistory<NavigationState> history = new();
		private NavigationState? navigatingToState;
		private object? sourceOfReference;
		private readonly SettingsService settingsService;
		private readonly LanguageService languageService;
		private readonly IExportProvider exportProvider;

		public ICSharpCode.ILSpyX.AssemblyList AssemblyList { get; private set; }

		private SharpTreeNode? root;
		public SharpTreeNode? Root {
			get => root;
			set => SetProperty(ref root, value);
		}

		public SharpTreeNode? SelectedItem {
			get => SelectedItems.FirstOrDefault();
			set => SelectedItems = value is null ? [] : [value];
		}

		private SharpTreeNode[] selectedItems = [];
		public SharpTreeNode[] SelectedItems {
			get => selectedItems;
			set {
				if (selectedItems.SequenceEqual(value))
					return;

				var oldSelection = selectedItems;
				selectedItems = value;
				OnPropertyChanged();
				TreeView_SelectionChanged(oldSelection, selectedItems);
			}
		}

		public string[]? SelectedPath => GetPathForNode(SelectedItem);

		private readonly List<LoadedAssembly> commandLineLoadedAssemblies = [];

		private bool HandleCommandLineArguments(CommandLineArguments args)
		{
			LoadAssemblies(args.AssembliesToLoad, commandLineLoadedAssemblies, focusNode: false);
			if (args.Language != null)
				languageService.Language = languageService.GetLanguage(args.Language);
			return true;
		}

		/// <summary>
		/// Called on startup or when passed arguments via WndProc from a second instance.
		/// In the format case, updateSettings is non-null; in the latter it is null.
		/// </summary>
		private async Task HandleCommandLineArgumentsAfterShowList(CommandLineArguments args, UpdateSettings? updateSettings = null)
		{
			var sessionSettings = settingsService.SessionSettings;

			var relevantAssemblies = commandLineLoadedAssemblies.ToList();
			commandLineLoadedAssemblies.Clear(); // clear references once we don't need them anymore

			await NavigateOnLaunch(args.NavigateTo, sessionSettings.ActiveTreeViewPath, updateSettings, relevantAssemblies);

			if (args.Search != null)
			{
				MessageBus.Send(this, new ShowSearchPageEventArgs(args.Search));
			}
		}

		public static IEntity? FindEntityInRelevantAssemblies(string navigateTo, IEnumerable<LoadedAssembly> relevantAssemblies)
		{
			ITypeReference typeRef;
			IMemberReference? memberRef = null;
			if (navigateTo.StartsWith("T:", StringComparison.Ordinal))
			{
				typeRef = IdStringProvider.ParseTypeName(navigateTo);
			}
			else
			{
				memberRef = IdStringProvider.ParseMemberIdString(navigateTo);
				typeRef = memberRef.DeclaringTypeReference;
			}
			foreach (LoadedAssembly asm in relevantAssemblies.ToList())
			{
				var module = asm.GetMetadataFileOrNull();
				if (module != null && CanResolveTypeInPEFile(module, typeRef, out var typeHandle))
				{
					ICompilation compilation = typeHandle.Kind == HandleKind.ExportedType
						? new DecompilerTypeSystem(module, module.GetAssemblyResolver())
						: new SimpleCompilation((PEFile)module, MinimalCorlib.Instance);
					return memberRef == null
						? typeRef.Resolve(new SimpleTypeResolveContext(compilation)) as ITypeDefinition
						: memberRef.Resolve(new SimpleTypeResolveContext(compilation));
				}
			}
			return null;
		}

		private static bool CanResolveTypeInPEFile(MetadataFile module, ITypeReference typeRef, out EntityHandle typeHandle)
		{
			// We intentionally ignore reference assemblies, so that the loop continues looking for another assembly that might have a usable definition.
			if (module.IsReferenceAssembly())
			{
				typeHandle = default;
				return false;
			}

			switch (typeRef)
			{
				case GetPotentiallyNestedClassTypeReference topLevelType:
					typeHandle = topLevelType.ResolveInPEFile(module);
					return !typeHandle.IsNil;
				case NestedTypeReference nestedType:
					if (!CanResolveTypeInPEFile(module, nestedType.DeclaringTypeReference, out typeHandle))
						return false;
					if (typeHandle.Kind == HandleKind.ExportedType)
						return true;
					var typeDef = module.Metadata.GetTypeDefinition((TypeDefinitionHandle)typeHandle);
					typeHandle = typeDef.GetNestedTypes().FirstOrDefault(t => {
						var td = module.Metadata.GetTypeDefinition(t);
						var typeName = ReflectionHelper.SplitTypeParameterCountFromReflectionName(module.Metadata.GetString(td.Name), out int typeParameterCount);
						return nestedType.AdditionalTypeParameterCount == typeParameterCount && nestedType.Name == typeName;
					});
					return !typeHandle.IsNil;
				default:
					typeHandle = default;
					return false;
			}
		}

		private void ShowAssemblyList(string name)
		{
			var list = settingsService.AssemblyListManager.LoadList(name);
			//Only load a new list when it is a different one
			if (list.ListName != AssemblyList.ListName)
			{
				ShowAssemblyList(list);
				SelectNode(Root?.Children.FirstOrDefault());
			}
		}

		private void assemblyList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				history.RemoveAll(_ => true);
			}
			if (e.OldItems != null)
			{
				var oldAssemblies = new HashSet<LoadedAssembly>(e.OldItems.Cast<LoadedAssembly>());
				history.RemoveAll(n => n.TreeNodes.Any(
					nd => nd.AncestorsAndSelf().OfType<AssemblyTreeNode>().Any(
						a => oldAssemblies.Contains(a.LoadedAssembly))));
			}

			MessageBus.Send(this, new CurrentAssemblyListChangedEventArgs(e));
		}

		public AssemblyTreeNode? FindAssemblyNode(LoadedAssembly asm)
		{
			return assemblyListTreeNode?.FindAssemblyNode(asm);
		}

		#region Node Selection

		public void SelectNodes(IEnumerable<SharpTreeNode> nodes)
		{
			// Ensure nodes exist
			var nodesList = nodes.Select(n => FindNodeByPath(GetPathForNode(n), true))
				.ExceptNullItems()
				.ToArray();

			if (!nodesList.Any() || nodesList.Any(n => n.AncestorsAndSelf().Any(a => a.IsHidden)))
			{
				return;
			}

			foreach (var node in nodesList)
			{
				activeView?.ScrollIntoView(node);
			}

			SelectedItems = nodesList.ToArray();
		}

		/// <summary>
		/// Retrieves a node using the .ToString() representations of its ancestors.
		/// </summary>
		public SharpTreeNode? FindNodeByPath(string[]? path, bool returnBestMatch)
		{
			if (path == null)
				return null;
			var node = Root;
			var bestMatch = node;
			foreach (var element in path)
			{
				if (node == null)
					break;
				bestMatch = node;
				node.EnsureLazyChildren();
				if (node is ILSpyTreeNode ilSpyTreeNode)
					ilSpyTreeNode.EnsureChildrenFiltered();
				node = node.Children.FirstOrDefault(c => c.ToString() == element);
			}

			return returnBestMatch ? node ?? bestMatch : node;
		}

		/// <summary>
		/// Gets the .ToString() representation of the node's ancestors.
		/// </summary>
		public static string[]? GetPathForNode(SharpTreeNode? node)
		{
			if (node == null)
				return null;
			List<string> path = new List<string>();
			while (node.Parent != null)
			{
				path.Add(node.ToString()!);
				node = node.Parent;
			}
			path.Reverse();
			return path.ToArray();
		}

		public ILSpyTreeNode? FindTreeNode(object? reference)
		{
			if (assemblyListTreeNode == null)
				return null;

			switch (reference)
			{
				case LoadedAssembly lasm:
					return assemblyListTreeNode.FindAssemblyNode(lasm);
				case MetadataFile asm:
					return assemblyListTreeNode.FindAssemblyNode(asm);
				case Resource res:
					return assemblyListTreeNode.FindResourceNode(res);
				case ValueTuple<Resource, string> resName:
					return assemblyListTreeNode.FindResourceNode(resName.Item1, resName.Item2);
				case ITypeDefinition type:
					return assemblyListTreeNode.FindTypeNode(type);
				case IField fd:
					return assemblyListTreeNode.FindFieldNode(fd);
				case IMethod md:
					return assemblyListTreeNode.FindMethodNode(md);
				case IProperty pd:
					return assemblyListTreeNode.FindPropertyNode(pd);
				case IEvent ed:
					return assemblyListTreeNode.FindEventNode(ed);
				case INamespace nd:
					return assemblyListTreeNode.FindNamespaceNode(nd);
				default:
					return null;
			}
		}

		private void JumpToReference(object? sender, NavigateToReferenceEventArgs e)
		{
			JumpToReferenceAsync(e.Reference, e.Source, e.InNewTabPage).HandleExceptions();
			IsActive = true;
		}

		/// <summary>
		/// Jumps to the specified reference.
		/// </summary>
		/// <returns>
		/// Returns a task that will signal completion when the decompilation of the jump target has finished.
		/// The task will be marked as canceled if the decompilation is canceled.
		/// </returns>
		private Task JumpToReferenceAsync(object? reference, object? source, bool inNewTabPage = false)
		{
			this.sourceOfReference = source;
			var decompilationTask = Task.CompletedTask;

			switch (reference)
			{
				case Decompiler.Disassembler.OpCodeInfo opCode:
					GlobalUtils.OpenLink(opCode.Link);
					break;
				case EntityReference unresolvedEntity:
					string protocol = unresolvedEntity.Protocol;
					var file = unresolvedEntity.ResolveAssembly(AssemblyList);
					if (file == null)
					{
						break;
					}
					if (protocol != "decompile")
					{
						foreach (var handler in exportProvider.GetExportedValues<IProtocolHandler>())
						{
							var node = handler.Resolve(protocol, file, unresolvedEntity.Handle, out bool newTabPage);
							if (node != null)
							{
								SelectNode(node, newTabPage || inNewTabPage);
								return decompilationTask;
							}
						}
					}
					var possibleToken = MetadataTokenHelpers.TryAsEntityHandle(MetadataTokens.GetToken(unresolvedEntity.Handle));
					if (possibleToken != null)
					{
						var typeSystem = new DecompilerTypeSystem(file, file.GetAssemblyResolver(), TypeSystemOptions.Default | TypeSystemOptions.Uncached);
						reference = typeSystem.MainModule.ResolveEntity(possibleToken.Value);
						goto default;
					}
					break;
				default:
					var treeNode = FindTreeNode(reference);
					if (treeNode != null)
						SelectNode(treeNode, inNewTabPage);
					break;
			}
			return decompilationTask;
		}

		#endregion

		#region Decompile (TreeView_SelectionChanged)

		public void RefreshDecompiledView()
		{
			DecompileSelectedNodes(DockWorkspace.ActiveTabPage.GetState() as DecompilerTextViewState);
		}

		public Language CurrentLanguage => languageService.Language;

		public LanguageVersion? CurrentLanguageVersion => languageService.LanguageVersion;

		public IEnumerable<ILSpyTreeNode> SelectedNodes => GetTopLevelSelection().OfType<ILSpyTreeNode>();

		#endregion

		public void NavigateHistory(bool forward)
		{
			try
			{
				TabPageModel tabPage = DockWorkspace.ActiveTabPage;
				var state = tabPage.GetState();
				if (state != null)
					history.UpdateCurrent(new NavigationState(tabPage, state));
				var newState = forward ? history.GoForward() : history.GoBack();
				navigatingToState = newState;

				TabPageModel activeTabPage = newState.TabPage;

				Debug.Assert(DockWorkspace.TabPages.Contains(activeTabPage));
				DockWorkspace.ActiveTabPage = activeTabPage;

				if (newState.TreeNodes.Any())
				{
					SelectNodes(newState.TreeNodes);
				}
				else if (newState.ViewState.ViewedUri != null)
				{
					// TODO: NavigateTo(new(newState.ViewState.ViewedUri, null));
				}
			}
			finally
			{
				navigatingToState = null;
			}
		}

		public bool CanNavigateBack => history.CanNavigateBack;

		public bool CanNavigateForward => history.CanNavigateForward;

		public void Refresh()
		{
			refreshThrottle.Tick();
		}

		private void UnselectAll()
		{
			SelectedItems = [];
		}

		private IEnumerable<SharpTreeNode> GetTopLevelSelection()
		{
			var selection = this.SelectedItems;
			var selectionHash = new HashSet<SharpTreeNode>(selection);

			return selection.Where(item => item.Ancestors().All(a => !selectionHash.Contains(a)));
		}

		public void SortAssemblyList()
		{
			using (activeView?.LockUpdates())
			{
				AssemblyList.Sort(AssemblyComparer.Instance);
			}
		}

		private class AssemblyComparer : IComparer<LoadedAssembly>
		{
			public static readonly AssemblyComparer Instance = new();
			int IComparer<LoadedAssembly>.Compare(LoadedAssembly? x, LoadedAssembly? y)
			{
				return string.Compare(x?.ShortName, y?.ShortName, StringComparison.CurrentCulture);
			}
		}

		public void CollapseAll()
		{
			using (activeView?.LockUpdates())
			{
				CollapseChildren(Root);
			}
		}

		private static void CollapseChildren(SharpTreeNode? node)
		{
			if (node is null)
				return;

			foreach (var child in node.Children)
			{
				if (!child.IsExpanded)
					continue;

				CollapseChildren(child);
				child.IsExpanded = false;
			}
		}

		public void OpenFiles(string[] fileNames, bool focusNode = true)
		{
			if (fileNames == null)
				throw new ArgumentNullException(nameof(fileNames));

			if (focusNode)
				UnselectAll();

			LoadAssemblies(fileNames, focusNode: focusNode);
		}

		private void ApplySessionSettings(object? sender, ApplySessionSettingsEventArgs e)
		{
			var settings = e.SessionSettings;

			settings.ActiveAssemblyList = AssemblyList.ListName;
			settings.ActiveTreeViewPath = SelectedPath;
			settings.ActiveAutoLoadedAssembly = GetAutoLoadedAssemblyNode(SelectedItem);
		}

		private static string? GetAutoLoadedAssemblyNode(SharpTreeNode? node)
		{
			var assemblyTreeNode = node?
				.AncestorsAndSelf()
				.OfType<AssemblyTreeNode>()
				.FirstOrDefault();

			var loadedAssembly = assemblyTreeNode?.LoadedAssembly;

			return loadedAssembly is not { IsLoaded: true, IsAutoLoaded: true }
				? null
				: loadedAssembly.FileName;
		}

		private void ActiveTabPageChanged(object? sender, ActiveTabPageChangedEventArgs e)
		{
			if (e.ViewState is not { } state)
				return;

			if (state.DecompiledNodes != null)
			{
				SelectNodes(state.DecompiledNodes);
			}
			else
			{
				// TODO: NavigateTo(new(state.ViewedUri, null));
			}
		}

		private void ResetLayout(object? sender, ResetLayoutEventArgs e)
		{
			RefreshDecompiledView();
		}
	}
}

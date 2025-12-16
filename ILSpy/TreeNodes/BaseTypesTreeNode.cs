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
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Threading;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpyX;
using ICSharpCode.ILSpyX.TreeView;

namespace ICSharpCode.ILSpy.TreeNodes
{
	/// <summary>
	/// Lists the base types of a class.
	/// </summary>
	sealed class BaseTypesTreeNode : ILSpyTreeNode
	{
		readonly MetadataFile module;
		readonly ITypeDefinition type;

		public BaseTypesTreeNode(MetadataFile module, ITypeDefinition type)
		{
			this.module = module;
			this.type = type;
			this.LazyLoading = true;
		}

		public override object Text => Properties.Resources.BaseTypes;

		public override object Icon => Images.SuperTypes;

		class ErrorTreeNode : SharpTreeNode
		{
			readonly string text;
			public ErrorTreeNode(string text) { this.text = text; }
			public override object Text => text;
		}

		protected override void LoadChildren()
		{
			try
			{
				if (module == null)
				{
					this.Children.Add(new ErrorTreeNode("Error: Module is null"));
					return;
				}
				AddBaseTypes(this.Children, module, type);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"BaseTypesTreeNode.LoadChildren failed: {ex}");
				this.Children.Add(new ErrorTreeNode("Error: " + ex.Message));
			}
		}

		internal static void AddBaseTypes(SharpTreeNodeCollection children, MetadataFile module, ITypeDefinition typeDefinition)
		{
			try 
			{
				TypeDefinitionHandle handle = (TypeDefinitionHandle)typeDefinition.MetadataToken;
				DecompilerTypeSystem typeSystem = new DecompilerTypeSystem(module, module.GetAssemblyResolver(),
					TypeSystemOptions.Default | TypeSystemOptions.Uncached);
				var t = typeSystem.MainModule.ResolveEntity(handle) as ITypeDefinition;
				if (t == null)
				{
					children.Add(new ErrorTreeNode("Error: Failed to resolve type"));
					return;
				}
				
				var baseTypes = t.GetAllBaseTypeDefinitions().ToList();
				
				foreach (var td in baseTypes.Skip(1).Reverse())
				{
					if (t.Kind != TypeKind.Interface || t.Kind == td.Kind)
					{
						children.Add(new BaseTypesEntryNode(td));
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"BaseTypesTreeNode.AddBaseTypes failed: {ex}");
				throw;
			}
		}

		public override void Decompile(Language language, ITextOutput output, DecompilationOptions options)
		{
			App.Current.Dispatcher.Invoke(new Action(EnsureLazyChildren));
			foreach (ILSpyTreeNode child in this.Children)
			{
				child.Decompile(language, output, options);
			}
		}
	}
}
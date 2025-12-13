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
using System.Reflection.Metadata;

using ICSharpCode.Decompiler;

namespace ICSharpCode.ILSpy.TreeNodes
{
	using ICSharpCode.Decompiler.TypeSystem;
	using ICSharpCode.ILSpyX;

	/// <summary>
	/// Represents a field in the TreeView.
	/// </summary>
	public sealed partial class FieldTreeNode : ILSpyTreeNode, IMemberTreeNode
	{
		public IField FieldDefinition { get; }

		public FieldTreeNode(IField field)
		{
			this.FieldDefinition = field ?? throw new ArgumentNullException(nameof(field));
		}

		public override object Text => GetText(GetFieldDefinition(), Language) + GetSuffixString(FieldDefinition);

		private IField GetFieldDefinition()
		{
			return ((MetadataModule)FieldDefinition.ParentModule?.MetadataFile
				?.GetTypeSystemWithCurrentOptionsOrNull(SettingsService, AssemblyTreeModel.CurrentLanguageVersion)
				?.MainModule)?.GetDefinition((FieldDefinitionHandle)FieldDefinition.MetadataToken) ?? FieldDefinition;
		}

		public static object GetText(IField field, Language language)
		{
			return language.FieldToString(field, includeDeclaringTypeName: false, includeNamespace: false, includeNamespaceOfDeclaringTypeName: false);
		}

		public override object Icon => GetIcon(GetFieldDefinition());

		public override FilterResult Filter(LanguageSettings settings)
		{
			if (settings.ShowApiLevel == ApiVisibility.PublicOnly && !IsPublicAPI)
				return FilterResult.Hidden;
			if (settings.SearchTermMatches(FieldDefinition.Name) && (settings.ShowApiLevel == ApiVisibility.All || LanguageService.Language.ShowMember(FieldDefinition)))
				return FilterResult.Match;
			else
				return FilterResult.Hidden;
		}

		public override void Decompile(Language language, ITextOutput output, DecompilationOptions options)
		{
			language.DecompileField(FieldDefinition, output, options);
		}

		public override bool IsPublicAPI {
			get {
				switch (GetFieldDefinition().Accessibility)
				{
					case Accessibility.Public:
					case Accessibility.Protected:
					case Accessibility.ProtectedOrInternal:
						return true;
					default:
						return false;
				}
			}
		}

		IEntity IMemberTreeNode.Member => FieldDefinition;

		public override string ToString()
		{
			return FieldDefinition.Name;
		}
	}
}

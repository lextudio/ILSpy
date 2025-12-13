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
using System;
using System.Collections.Concurrent;
using System.Threading;

using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpyX.Abstractions;

namespace ICSharpCode.ILSpyX.Search
{
	public class MemberSearchStrategy : AbstractEntitySearchStrategy
	{
		readonly MemberSearchKind searchKind;

		public MemberSearchStrategy(ILanguage language, ApiVisibility apiVisibility, SearchRequest searchRequest,
			IProducerConsumerCollection<SearchResult> resultQueue, MemberSearchKind searchKind = MemberSearchKind.All)
			: base(language, apiVisibility, searchRequest, resultQueue)
		{
			this.searchKind = searchKind;
		}

		public override void Search(MetadataFile module, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Console.WriteLine($"[MemberSearchStrategy] Search called for module: {module.FileName}, searchKind: {searchKind}");
			var metadata = module.Metadata;
			try
			{
				Console.WriteLine($"[MemberSearchStrategy] Metadata counts: TypeDefs={metadata.TypeDefinitions.Count}, MethodDefs={metadata.MethodDefinitions.Count}, FieldDefs={metadata.FieldDefinitions.Count}, PropertyDefs={metadata.PropertyDefinitions.Count}, EventDefs={metadata.EventDefinitions.Count}");
			}
			catch { }
			var typeSystem = module.GetTypeSystemWithDecompilerSettingsOrNull(searchRequest.DecompilerSettings);
			if (typeSystem == null)
			{
				Console.WriteLine($"[MemberSearchStrategy] TypeSystem is null for module: {module.FileName}");
				return;
			}

			if (searchKind == MemberSearchKind.All || searchKind == MemberSearchKind.Type)
			{
				foreach (var handle in metadata.TypeDefinitions)
				{
					cancellationToken.ThrowIfCancellationRequested();
					string languageSpecificName = language.GetEntityName(module, handle, fullNameSearch, omitGenerics);
					if (languageSpecificName != null && !IsMatch(languageSpecificName))
						continue;
					var type = ((MetadataModule)typeSystem.MainModule).GetDefinition(handle);
					if (!CheckVisibility(type) || !IsInNamespaceOrAssembly(type))
						continue;
					OnFoundResult(type);
				}
			}

			if (searchKind == MemberSearchKind.All || searchKind == MemberSearchKind.Member || searchKind == MemberSearchKind.Method)
			{
				int sampleCount = 0;
				foreach (var handle in metadata.MethodDefinitions)
				{
					cancellationToken.ThrowIfCancellationRequested();
					string languageSpecificName = language.GetEntityName(module, handle, fullNameSearch, omitGenerics);
					if (languageSpecificName != null)
					{
						var term0 = (searchTerm != null && searchTerm.Length > 0) ? searchTerm[0] : string.Empty;
						if (!string.IsNullOrEmpty(term0) && languageSpecificName.IndexOf(term0, System.StringComparison.OrdinalIgnoreCase) >= 0)
						{
							Console.WriteLine($"[MemberSearchStrategy] languageSpecificName CONTAINS term: '{term0}' in '{languageSpecificName}'");
						}
						var isMatch = IsMatch(languageSpecificName);
						if (sampleCount < 10)
						{
							Console.WriteLine($"[MemberSearchStrategy] Method name sample: {languageSpecificName}, IsMatch={isMatch}");
							sampleCount++;
						}
						if (!isMatch)
							continue;
					}
					else
					{
						if (sampleCount < 10)
						{
							Console.WriteLine($"[MemberSearchStrategy] Method name sample: null (handle {handle})");
							sampleCount++;
						}
						continue;
					}
					var method = ((MetadataModule)typeSystem.MainModule).GetDefinition(handle);
					if (method == null)
						continue;
						try
						{
							Console.WriteLine($"[MemberSearchStrategy] Method details: FullName={method.FullName}, ParentModule={method.ParentModule?.MetadataFile?.FileName}, Assembly={method.ParentModule?.AssemblyName}, Namespace={method.Namespace}, Accessibility={method.Accessibility}");
							Console.WriteLine($"[MemberSearchStrategy] Visibility check: {CheckVisibility(method)}, InNamespaceOrAssembly: {IsInNamespaceOrAssembly(method)}");
						}
						catch { }
					if (!CheckVisibility(method) || !IsInNamespaceOrAssembly(method))
						continue;
						if (method != null)
						{
							Console.WriteLine($"[MemberSearchStrategy] Candidate match: Method {method.FullName}, Handle: {handle}, Name: {languageSpecificName}");
							try
							{
								var sr = searchRequest.SearchResultFactory.Create(method);
								Console.WriteLine($"[MemberSearchStrategy] Direct factory created SearchResult: {sr?.Name} ({sr?.GetType().Name})");
								if (sr != null)
								{
									OnFoundResult(sr);
									Console.WriteLine($"[MemberSearchStrategy] Direct OnFoundResult called for: {sr.Name}");
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine($"[MemberSearchStrategy] Direct Create/TryAdd failed: {ex}");
							}
							OnFoundResult(method);
						}
				}
			}

			if (searchKind == MemberSearchKind.All || searchKind == MemberSearchKind.Member || searchKind == MemberSearchKind.Field)
			{
				foreach (var handle in metadata.FieldDefinitions)
				{
					cancellationToken.ThrowIfCancellationRequested();
					string languageSpecificName = language.GetEntityName(module, handle, fullNameSearch, omitGenerics);
					if (languageSpecificName != null && !IsMatch(languageSpecificName))
						continue;
					var field = ((MetadataModule)typeSystem.MainModule).GetDefinition(handle);
					if (!CheckVisibility(field) || !IsInNamespaceOrAssembly(field))
						continue;
					OnFoundResult(field);
				}
			}

			if (searchKind == MemberSearchKind.All || searchKind == MemberSearchKind.Member || searchKind == MemberSearchKind.Property)
			{
				foreach (var handle in metadata.PropertyDefinitions)
				{
					cancellationToken.ThrowIfCancellationRequested();
					string languageSpecificName = language.GetEntityName(module, handle, fullNameSearch, omitGenerics);
					if (languageSpecificName != null && !IsMatch(languageSpecificName))
						continue;
					var property = ((MetadataModule)typeSystem.MainModule).GetDefinition(handle);
					if (!CheckVisibility(property) || !IsInNamespaceOrAssembly(property))
						continue;
					OnFoundResult(property);
				}
			}

			if (searchKind == MemberSearchKind.All || searchKind == MemberSearchKind.Member || searchKind == MemberSearchKind.Event)
			{
				foreach (var handle in metadata.EventDefinitions)
				{
					cancellationToken.ThrowIfCancellationRequested();
					string languageSpecificName = language.GetEntityName(module, handle, fullNameSearch, omitGenerics);
					if (!IsMatch(languageSpecificName))
						continue;
					var @event = ((MetadataModule)typeSystem.MainModule).GetDefinition(handle);
					if (!CheckVisibility(@event) || !IsInNamespaceOrAssembly(@event))
						continue;
					OnFoundResult(@event);
				}
			}
		}
	}

	public enum MemberSearchKind
	{
		All,
		Type,
		Member,
		Field,
		Property,
		Event,
		Method
	}
}

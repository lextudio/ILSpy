// Copyright (c) 2021 Siegfried Pammer
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

using System.Collections.ObjectModel;
using System.Composition;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.Decompiler;
using ICSharpCode.ILSpy.AssemblyTree;
using ICSharpCode.ILSpy.Docking;
using ICSharpCode.ILSpyX;
using DecompilationLanguage = ProjectRover.Services.DecompilationLanguage;

using TomsToolbox.Wpf;

namespace ICSharpCode.ILSpy
{
    public record ThemeOption(string Name, ThemeVariant Variant);
    public record LanguageOption(string Name, DecompilationLanguage Language);

	[Export]
	[Shared]
	public class MainWindowViewModel : ObservableObject
	{
        private readonly SettingsService settingsService;
        private readonly LanguageService languageService;
        private readonly IDockWorkspace dockWorkspace;
        private readonly IPlatformService platformService;
        private readonly AssemblyTreeModel assemblyTreeModel;

        public MainWindowViewModel(SettingsService settingsService, LanguageService languageService, IDockWorkspace dockWorkspace, IPlatformService platformService, AssemblyTreeModel assemblyTreeModel)
        {
            this.settingsService = settingsService;
            this.languageService = languageService;
            this.dockWorkspace = dockWorkspace;
            this.platformService = platformService;
            this.assemblyTreeModel = assemblyTreeModel;

            OpenFileCommand = new RelayCommand(OpenFile);
            ExitCommand = new RelayCommand(Exit);

            Themes = new ObservableCollection<ThemeOption>
            {
                new("Light", ThemeVariant.Light),
                new("Dark", ThemeVariant.Dark)
            };
            SelectedTheme = Themes[0];
        }

		public IDockWorkspace Workspace => dockWorkspace;

        public AssemblyTreeModel AssemblyTreeModel => assemblyTreeModel;

		public LanguageService LanguageService => languageService;

		public AssemblyListManager AssemblyListManager => settingsService.AssemblyListManager;

		public IPlatformService PlatformService => platformService;

        public ObservableCollection<ThemeOption> Themes { get; }

        private ThemeOption selectedTheme;
        public ThemeOption SelectedTheme
        {
            get => selectedTheme;
            set => SetProperty(ref selectedTheme, value);
        }

        public IRelayCommand OpenFileCommand { get; }
        public IRelayCommand ExitCommand { get; }

        private void OpenFile()
        {
            // TODO: Implement OpenFile
        }

        private void Exit()
        {
            // TODO: Implement Exit
        }
	}
}

// Shimbed Avalonia-compatible AboutPage for ILSpy.Shims
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Text.RegularExpressions;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Interactivity;

using ICSharpCode.Decompiler;
using ICSharpCode.ILSpy.Properties;
using ICSharpCode.ILSpy.TextViewControl;
using ICSharpCode.ILSpy.Themes;
using ICSharpCode.ILSpy.Updates;
using ICSharpCode.ILSpy.ViewModels;

using AvaloniaEdit.Rendering;
using Avalonia;

namespace ICSharpCode.ILSpy
{
    // Keep the original export attributes so MEF can discover this shim as before.
    [ExportMainMenuCommand(ParentMenuID = nameof(Resources._Help), Header = nameof(Resources._About), MenuOrder = 99999)]
    [Shared]
    public sealed class AboutPage : SimpleCommand
    {
        readonly SettingsService settingsService;
        readonly IEnumerable<IAboutPageAddition> aboutPageAdditions;

        public AboutPage(SettingsService settingsService, IEnumerable<IAboutPageAddition> aboutPageAdditions)
        {
            this.settingsService = settingsService;
            this.aboutPageAdditions = aboutPageAdditions;
            MessageBus<ShowAboutPageEventArgs>.Subscribers += (_, e) => ShowAboutPage(e.TabPage);
        }

        public override void Execute(object parameter)
        {
            MessageBus.Send(this, new NavigateToEventArgs(new RequestNavigateEventArgs(new Uri("resource://aboutpage"), null), inNewTabPage: true));
        }

        private void ShowAboutPage(TabPageModel tabPage)
        {
            tabPage.ShowTextView(Display);
        }

        private void Display(DecompilerTextView textView)
        {
            AvalonEditTextOutput output = new AvalonEditTextOutput() {
                Title = Resources.About,
                EnableHyperlinks = true
            };
            output.WriteLine(Resources.ILSpyVersion + DecompilerVersionInfo.FullVersionWithCommitHash);

            string prodVersion = GetDotnetProductVersion();
            output.WriteLine(Resources.NETFrameworkVersion + prodVersion);

            output.AddUIElement(
            delegate {
                var stackPanel = new StackPanel {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Orientation = Avalonia.Layout.Orientation.Horizontal
                };
                if (UpdateService.LatestAvailableVersion == null)
                {
                    AddUpdateCheckButton(stackPanel, textView);
                }
                else
                {
                    ShowAvailableVersion(UpdateService.LatestAvailableVersion, stackPanel);
                }
                var checkBox = new CheckBox {
                    Margin = new Thickness(4),
                    Content = Resources.AutomaticallyCheckUpdatesEveryWeek
                };

                var settings = settingsService.GetSettings<UpdateSettings>();
                // Bind IsChecked to settings.AutomaticUpdateCheckEnabled
                var binding = new Binding("AutomaticUpdateCheckEnabled") { Source = settings, Mode = BindingMode.TwoWay };
                checkBox.Bind(ToggleButton.IsCheckedProperty, binding);
                return new StackPanel {
                    Margin = new Thickness(0, 4, 0, 0),
                    Cursor = new Cursor(StandardCursorType.Arrow),
                    Children = { stackPanel, checkBox }
                };
            });
            output.WriteLine();

            foreach (var plugin in aboutPageAdditions)
                plugin.Write(output);
            output.WriteLine();
            output.Address = new Uri("resource://AboutPage");
            using (Stream s = typeof(AboutPage).Assembly.GetManifestResourceStream(typeof(AboutPage), Resources.ILSpyAboutPageTxt))
            {
                if (s != null)
                {
                    using (StreamReader r = new StreamReader(s))
                    {
                        while (r.ReadLine() is { } line)
                        {
                            output.WriteLine(line);
                        }
                    }
                }
            }
            output.AddVisualLineElementGenerator(new MyLinkElementGenerator("MIT License", "resource:license.txt"));
            output.AddVisualLineElementGenerator(new MyLinkElementGenerator("third-party notices", "resource:third-party-notices.txt"));
            textView.ShowText(output);
        }

        private static string GetDotnetProductVersion()
        {
            // In case of AOT .Location is null, we need a fallback for that
            string assemblyLocation = typeof(Uri).Assembly.Location;

            if (!String.IsNullOrWhiteSpace(assemblyLocation))
            {
                return System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
            }
            else
            {
                var version = typeof(Object).Assembly.GetName().Version;
                if (version != null)
                {
                    return version.ToString();
                }
            }

            return "UNKNOWN";
        }

        sealed class MyLinkElementGenerator : LinkElementGenerator
        {
            readonly Uri uri;

            public MyLinkElementGenerator(string matchText, string url) : base(new Regex(Regex.Escape(matchText)))
            {
                this.uri = new Uri(url);
                this.RequireControlModifierForClick = false;
            }

            protected override Uri GetUriFromMatch(Match match)
            {
                return uri;
            }
        }

        static void AddUpdateCheckButton(StackPanel stackPanel, DecompilerTextView textView)
        {
            Button button = ThemeManager.Current.CreateButton();
            button.Content = Resources.CheckUpdates;
            button.Cursor = new Cursor(StandardCursorType.Arrow);
            stackPanel.Children.Add(button);

            button.Click += async delegate {
                button.Content = Resources.Checking;
                button.IsEnabled = false;

                try
                {
                    AvailableVersionInfo vInfo = await UpdateService.GetLatestVersionAsync();
                    stackPanel.Children.Clear();
                    ShowAvailableVersion(vInfo, stackPanel);
                }
                catch (Exception ex)
                {
                    AvalonEditTextOutput exceptionOutput = new AvalonEditTextOutput();
                    exceptionOutput.WriteLine(ex.ToString());
                    textView.ShowText(exceptionOutput);
                }
            };
        }

        static void ShowAvailableVersion(AvailableVersionInfo availableVersion, StackPanel stackPanel)
        {
            if (AppUpdateService.CurrentVersion == availableVersion.Version)
            {
                stackPanel.Children.Add(
                    new Image {
                        Width = 16, Height = 16,
                        Source = Images.LoadImage(Images.OK),
                        Margin = new Thickness(4, 0, 4, 0)
                    });
                stackPanel.Children.Add(
                    new TextBlock {
                        Text = Resources.UsingLatestRelease,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom
                    });
            }
            else if (AppUpdateService.CurrentVersion < availableVersion.Version)
            {
                stackPanel.Children.Add(
                    new TextBlock {
                        Text = string.Format(Resources.VersionAvailable, availableVersion.Version),
                        Margin = new Thickness(0, 0, 8, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom
                    });
                if (availableVersion.DownloadUrl != null)
                {
                    Button button = ThemeManager.Current.CreateButton();
                    button.Content = Resources.Download;
                    button.Cursor = new Cursor(StandardCursorType.Arrow);
                    button.Click += delegate {
                        GlobalUtils.OpenLink(availableVersion.DownloadUrl);
                    };
                    stackPanel.Children.Add(button);
                }
            }
            else
            {
                stackPanel.Children.Add(new TextBlock { Text = Resources.UsingNightlyBuildNewerThanLatestRelease });
            }
        }
    }

    /// <summary>
    /// Interface that allows plugins to extend the about page.
    /// </summary>
    public interface IAboutPageAddition
    {
        void Write(ISmartTextOutput textOutput);
    }
}

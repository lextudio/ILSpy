using System;
using System.Linq;
using Avalonia.Controls;
using TomsToolbox.Composition;
using System.Collections.Generic;
using ICSharpCode.ILSpy.Commands;
using System.Windows.Input;

namespace ICSharpCode.ILSpy.Controls
{
    public partial class MainMenu : UserControl
    {
        public MainMenu()
        {
            InitializeComponent();

            this.AttachedToVisualTree += (_, _) => {
                var exportProvider = ProjectRover.App.ExportProvider;
                if (exportProvider != null)
                {
                    InitMainMenu(Menu, exportProvider);
                }
            };
        }

        static void InitMainMenu(Menu mainMenu, IExportProvider exportProvider)
        {
            // Get all main menu commands exported with contract "MainMenuCommand"
            var mainMenuCommands = exportProvider.GetExports<ICommand, IMainMenuCommandMetadata>("MainMenuCommand").ToArray();

            var parentMenuItems = new Dictionary<string, MenuItem>();
            var menuGroups = mainMenuCommands.OrderBy(c => c.Metadata?.MenuOrder).GroupBy(c => c.Metadata?.ParentMenuID).ToArray();

            foreach (var menu in menuGroups)
            {
                var parentMenuItem = GetOrAddParentMenuItem(mainMenu, parentMenuItems, menu.Key);
                foreach (var category in menu.GroupBy(c => c.Metadata?.MenuCategory))
                {
                    if (parentMenuItem.Items.Count > 0)
                    {
                        parentMenuItem.Items.Add(new Separator());
                    }
                    foreach (var entry in category)
                    {
                        if (menuGroups.Any(g => g.Key == entry.Metadata?.MenuID))
                        {
                            var subParent = GetOrAddParentMenuItem(mainMenu, parentMenuItems, entry.Metadata?.MenuID);
                            subParent.Header = entry.Metadata?.Header ?? subParent.Header?.ToString();
                            parentMenuItem.Items.Add(subParent);
                        }
                        else
                        {
                            var cmd = entry.Value;
                            var headerText = ICSharpCode.ILSpy.Util.ResourceHelper.GetString(entry.Metadata?.Header);
                            var menuItem = new MenuItem
                            {
                                Command = cmd,
                                Tag = entry.Metadata?.MenuID,
                                Header = headerText ?? entry.Metadata?.Header
                            };
                            parentMenuItem.Items.Add(menuItem);
                        }
                    }
                }
            }

            foreach (var item in parentMenuItems.Values.Where(i => i.Parent == null))
            {
                if (!mainMenu.Items.Contains(item))
                    mainMenu.Items.Add(item);
            }

            static MenuItem GetOrAddParentMenuItem(Menu mainMenu, Dictionary<string, MenuItem> parentMenuItems, string menuId)
            {
                if (menuId == null) menuId = string.Empty;
                if (!parentMenuItems.TryGetValue(menuId, out var parentMenuItem))
                {
                    var topLevel = mainMenu.Items.OfType<MenuItem>().FirstOrDefault(m => (string?)m.Tag == menuId);
                    if (topLevel == null)
                    {
                        parentMenuItem = new MenuItem { Header = menuId, Tag = menuId };
                        parentMenuItems.Add(menuId, parentMenuItem);
                    }
                    else
                    {
                        parentMenuItems.Add(menuId, topLevel);
                        parentMenuItem = topLevel;
                    }
                }
                return parentMenuItem;
            }
        }
    }
}
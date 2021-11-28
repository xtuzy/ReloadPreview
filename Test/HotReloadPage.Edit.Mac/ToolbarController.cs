using AppKit;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotReloadPage.Edit.Mac
{
    public class ToolbarController
    {
        public NSViewController ViewController;
        public ToolbarController()
        {
            ViewController = new NSViewController();

            Init();
        }

        private void Init()
        {
            ViewController.Title = nameof(ToolbarController);
            ViewController.View = new NSView(new CGRect(12, 10, 500, 200));
            ViewController.View.WantsLayer = true;
            ViewController.View.Layer.BackgroundColor = NSColor.Cyan.CGColor;
            Console.WriteLine("Windows's " + nameof(ToolbarController) + this.GetHashCode());
        }

        public void SetToolBarForCurrentWindow()
        {
            var currentWindow = ViewController.View.Window;//https://stackoverflow.com/questions/19517183/self-window-not-working-inside-view-controller
            currentWindow.Toolbar = CreateToolbar();
            //currentWindow.TitleVisibility = NSWindowTitleVisibility.Hidden;
            currentWindow.ToolbarStyle = NSWindowToolbarStyle.Preference;
            Console.WriteLine("ToolBarItem Count:" + currentWindow.Toolbar.Items.Length);
        }

        public NSToolbar CreateToolbar()
        {
            NSToolbar toolbar = new NSToolbar("AppToolbar");// identify is "AppToolbar"
            toolbar.AllowsUserCustomization = false;
            toolbar.AutosavesConfiguration = false;
            toolbar.DisplayMode = NSToolbarDisplayMode.IconAndLabel;
            toolbar.SizeMode = NSToolbarSizeMode.Default;
            //toolbar.Delegate = toolbar;

            //Set Item
            toolbar.DefaultItemIdentifiers = (tool) =>
            {
                return new string[] { "Save", "Setting","Search" };//显示的是哪些
            };

            toolbar.AllowedItemIdentifiers = (tool) =>
            {
                return new string[] { "Save", "Setting", "Search", "File" };//貌似没用
            };

            toolbar.CenteredItemIdentifier = "Search";//居中的是谁
            

            toolbar.WillInsertItem = (tool, id, send) =>
            {  //https://searchcode.com/file/115714185/main/src/addins/MacPlatform/MainToolbar/MainToolbar.cs/
                switch (id)
                {
                    case "Save":
                        return CreateToolbarItem("Save", string.Empty);
                    case "Setting":
                        return CreateToolbarItem("Setting", string.Empty);
                    case "Search":
                        return CreateToolbarItem("Search", String.Empty);
                    case "File":
                        return CreateToolbarItem("File", String.Empty);
                    default:
                        return null;
                }
            };
            return toolbar;
        }

        NSToolbarItem CreateToolbarItem(string idName, string iconName)
        {
            NSToolbarItem toolbarItem = new NSToolbarItem(idName);
            toolbarItem.Label = idName;
            toolbarItem.PaletteLabel = idName;
            toolbarItem.ToolTip = idName;//鼠标提示
            //toolbarItem.Image = NSImage.GetSystemSymbol("car.fill",null);
            //toolbarItem.Tag = kSaveToolbatItemTag;

            toolbarItem.MinSize = new CGSize(25, 25);
            toolbarItem.MaxSize = new CGSize(50, 50);
            //toolbarItem.Target=self;
            //toolbarItem.Action= @selector(toolbarItemClicked:);
            switch (idName)
            {
                case "Save":
                    toolbarItem.Image = NSImage.GetSystemSymbol("folder", "toggle");
                    toolbarItem.Activated += (sender, e) =>
                    {
                        var alert = new NSAlert()
                        {
                            AlertStyle = NSAlertStyle.Critical,
                            InformativeText = "We need to save the document here...",
                            MessageText = "Save Document",
                        };
                        alert.RunModal();
                    };
                    break;
                case "Setting":
                    var image = NSImage.GetSystemSymbol("gearshape", "setting");
                    //var config = NSImage.SymbolConfiguration(textStyle: .body, scale: .large);
                    //config = config.applying(.init(paletteColors: [.systemTeal, .systemGray]));
                    //toolbarItem.Image = image.withSymbolConfiguration(config);
                    toolbarItem.Image = image;
                    toolbarItem.Activated += (sender, e) =>
                    {
                        var alert = new NSAlert()
                        {
                            AlertStyle = NSAlertStyle.Critical,
                            InformativeText = "We need to set the app",
                            MessageText = "Setting",
                        };
                        alert.RunModal();
                    };
                    break;
                case "Search":
                    toolbarItem = new NSSearchToolbarItem("Search");
                    break;
            }

            return toolbarItem;
        }
    }
}

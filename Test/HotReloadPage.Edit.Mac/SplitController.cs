using AppKit;
using CoreGraphics;
using System;

namespace HotReloadPage.Edit.Mac
{
    internal class SplitController
    {
        public NSSplitViewController ViewController;
        public SplitController(NSCollectionViewDataSource dataSource)
        {
            ViewController = new NSSplitViewController();

            // sidebar
             var sidebarContent = new NSView() { };
            var sidebarController = new NSViewController() {  View = sidebarContent };
            
            // sidebar background
            var sidebarbacBgroundView = new NSVisualEffectView(new CGRect(x: 0, y: 0, NSScreen.MainScreen.Frame.Width, NSScreen.MainScreen.Frame.Height));
            sidebarbacBgroundView.BlendingMode = NSVisualEffectBlendingMode.BehindWindow;
            sidebarbacBgroundView.Material = NSVisualEffectMaterial.MediumLight;
            sidebarbacBgroundView.State = NSVisualEffectState.Active;
            sidebarController.View.AddSubview(sidebarbacBgroundView);

            var sidebar = CreateSidebar();
            //var sidebar = CreateSidebar(dataSource);
            sidebarContent.AddSubview(sidebar);

            sidebar.TopAnchor.ConstraintEqualToAnchor(sidebarContent.TopAnchor).Active = true;
            //sidebar.BottomAnchor.ConstraintEqualToAnchor(sidebarContent.BottomAnchor).Active = true;
            sidebar.WidthAnchor.ConstraintEqualToAnchor(sidebarContent.WidthAnchor).Active = true;
            //sidebar.HeightAnchor.ConstraintEqualToAnchor(sidebarContent.HeightAnchor).Active = true;

            var sidebarItem = NSSplitViewItem.FromViewController(sidebarController);
            sidebarItem.MinimumThickness = 120;//sidebar最小宽度,105刚好可以让Toggle图像不被遮挡,
            // right show content
            var rightC = new NSViewController() { View = new NSView(new CGRect(10, 10, 400, 500)) };
            rightC.View.WantsLayer = true;
            rightC.View.Layer.BackgroundColor = NSColor.White.CGColor;
            var rightItem = NSSplitViewItem.FromViewController(rightC);

            // add to SplitViewController
            ViewController.AddSplitViewItem(sidebarItem);
            ViewController.AddSplitViewItem(rightItem);

            // set divide line
            ViewController.SplitView.WantsLayer = true;
            ViewController.SplitView.Layer.BackgroundColor = NSColor.Red.CGColor;// Black
            ViewController.SplitView.DividerStyle = NSSplitViewDividerStyle.Thin;
        }

        /// <summary>
        /// Must load after create window
        /// </summary>
        public void SetToolBarForCurrentWindow()
        {
            var currentWindow = ViewController.View.Window;//https://stackoverflow.com/questions/19517183/self-window-not-working-inside-view-controller
            currentWindow.TitlebarAppearsTransparent = true;
            currentWindow.TitleVisibility = NSWindowTitleVisibility.Hidden;//Hiden Title, Item can move to left
            
            currentWindow.Toolbar = CreateToolbar();
            currentWindow.ToolbarStyle = NSWindowToolbarStyle.UnifiedCompact;//紧凑
            Console.WriteLine("currentWindow.ContentView:" + currentWindow.ContentView);
            currentWindow.StyleMask = NSWindowStyle.Titled |
                //NSWindowStyle.FullSizeContentView |
                NSWindowStyle.Closable |
                NSWindowStyle.Miniaturizable|
                NSWindowStyle.Resizable ;

            Console.WriteLine("ToolBarItem Count:" + currentWindow.Toolbar.Items.Length);
        }

         NSToolbar CreateToolbar()
        {
            NSToolbar toolbar = new NSToolbar("AppToolbar");// identify is "AppToolbar"
            toolbar.AllowsUserCustomization = false;
            toolbar.AutosavesConfiguration = false;
            toolbar.DisplayMode = NSToolbarDisplayMode.IconAndLabel;
            toolbar.SizeMode = NSToolbarSizeMode.Small;
            //toolbar.Delegate = toolbar;

            //Set Item
            toolbar.DefaultItemIdentifiers = (tool) =>
            {
                return new string[] { "Toggle", "MainPanel", "Setting" };
            };

            /*toolbar.AllowedItemIdentifiers = (tool) =>
            {
                return new string[] { "Search", "File" };
            };*/

            toolbar.WillInsertItem = (tool, id, send) =>
            {  //https://searchcode.com/file/115714185/main/src/addins/MacPlatform/MainToolbar/MainToolbar.cs/
                switch (id)
                {
                    case "MainPanel":
                        return NSTrackingSeparatorToolbarItem.GetTrackingSeparatorToolbar("MainPanel",ViewController.SplitView,0);
                    case "Toggle":
                        return CreateToolbarItem("Toggle", string.Empty);
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

            /*var mainPanelSeparatorIdentifier = "MainPanel";
            toolbar.InsertItem(mainPanelSeparatorIdentifier, 0);*/

            return toolbar;
        }

        NSToolbarItem CreateToolbarItem(string idName, string iconName)
        {
            //NSToolbar.NSToolbarSidebarTrackingSeparatorItemIdentifier
            NSToolbarItem toolbarItem = new NSToolbarItem(idName);
            toolbarItem.Label = idName;
            toolbarItem.PaletteLabel = idName;
            toolbarItem.ToolTip = idName;//鼠标提示
            toolbarItem.MinSize = new CGSize(10, 10);
            toolbarItem.MaxSize = new CGSize(24, 24);
            //toolbarItem.Image = NSImage.GetSystemSymbol("car.fill",null);
            switch (idName)
            {
                case "Toggle":
                    //toolbarItem.Image =  NSImage.ImageNamed(NSImageName.TouchBarListViewTemplate);
                    //toolbarItem.Image = NSImage.GetSystemSymbol("rectangle.split.2x1", "toggle");
                    toolbarItem.Image = NSImage.GetSystemSymbol("sidebar.right", "toggle");
                    toolbarItem.Activated += (sender, e) =>
                    {
                        //Toggle方法都无效
                        //ViewController.View.Window.FirstResponder.TryToPerformwith(new ObjCRuntime.Selector("toggleSidebar"), null);
                        //ViewController.ToggleSidebar(toolbarItem);

                        //Collapse的也无效
                        /*ViewController.SplitViewItems[0].CanCollapse = true;
                        var o = ViewController.SplitViewItems[1].Animator;
                        (o as NSSplitViewItem).CanCollapse = true;
                        (o as NSSplitViewItem).Collapsed = false;
                        Console.WriteLine("o:"+o.GetType());*/

                        //生效
                        if(ViewController.SplitView.Subviews[0].Hidden)
                            ViewController.SplitView.Subviews[0].Hidden = false;
                        else
                            ViewController.SplitView.Subviews[0].Hidden = true;
                    };
                    break;
                case "Setting":
                    var image = NSImage.GetSystemSymbol("gearshape","setting");
                    //var config = NSImage.SymbolConfiguration(textStyle: .body, scale: .large);
                    //config = config.applying(.init(paletteColors: [.systemTeal, .systemGray]));
                    //toolbarItem.Image = image.withSymbolConfiguration(config);
                    toolbarItem.Image = image;
                    toolbarItem.Activated += (sender, e) =>
                    {
                        ViewController.View.Window.ToggleToolbarShown(toolbarItem);//hiden or show toolbar
                    };
                    break;

            }
            
            return toolbarItem;
        }

        NSView CreateSidebar()
        {
            //var sidebar = new NSView();
            var sidebar = new NSStackView() { TranslatesAutoresizingMaskIntoConstraints =false};
            //sidebar.WantsLayer = true;
            //sidebar.Layer.BackgoundColor
            sidebar.Alignment = NSLayoutAttribute.Left;
            sidebar.Orientation = NSUserInterfaceLayoutOrientation.Vertical;
            sidebar.Distribution = NSStackViewDistribution.Fill;
            sidebar.Spacing = 10;
            for(var i = 0; i < 10; i++)
            {
                var Lable = new NSTextField() { TranslatesAutoresizingMaskIntoConstraints=false};
                Lable.StringValue = "I'm a Lable";
                Lable.TextColor = NSColor.Blue;
                Lable.Editable = false;
                Lable.BackgroundColor = NSColor.Clear;
                sidebar.AddArrangedSubview(Lable);
                //Lable.LeftAnchor.ConstraintEqualToAnchor(sidebar.LeftAnchor).Active = true;
                Lable.RightAnchor.ConstraintEqualToAnchor(sidebar.RightAnchor).Active = true;
                //Lable.WidthAnchor.ConstraintGreaterThanOrEqualToConstant(120).Active = true;
            }
            
            return sidebar;
        }


        

            NSView CreateSidebar(NSCollectionViewDataSource dataSource)
        {
            //var sidebar = new NSView();
            var sidebar = new NSCollectionView() { TranslatesAutoresizingMaskIntoConstraints = false };
            sidebar.DataSource =  dataSource;
            sidebar.Delegate = new NSCollectionViewDelegate();
            sidebar.CollectionViewLayout =new NSCollectionViewLayout();

            return sidebar;
        }

    }
}

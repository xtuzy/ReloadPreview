using System;
using System.Collections.Generic;
using System.Text;
using AppKit;
using CoreGraphics;
using ReloadPreview;
using Xamarin.Helper.Layouts;
using Xamarin.Helper.Views;

namespace HotReloadPage.Edit.Mac
{
    public class ReloadMainWindowPage : IReload
    {
        private NSViewController ViewController;
        NSCollectionViewDataSource dataSource;
        private NSWindow Window;

        public void Reload(object controller, object view)
        {
            Console.WriteLine("Windows's " + nameof(ReloadMainWindowPage) + this.GetHashCode());

            ViewController = controller as NSViewController;
            Window = ViewController.View.Window;
            dataSource = view as NSCollectionViewDataSource;
            Window.TitlebarAppearsTransparent = true;
            Window.TitlebarSeparatorStyle = NSTitlebarSeparatorStyle.Line;
            Window.TitleVisibility = NSWindowTitleVisibility.Hidden;
            Window.ToolbarStyle = NSWindowToolbarStyle.UnifiedCompact;
            Window.Toolbar = new ToolbarController().CreateToolbar();

            ViewController.View.Layer.BackgroundColor =NSColor.Cyan.CGColor;

            foreach (var v in ViewController.View.Subviews)
            {
                v.RemoveFromSuperview();
            }
            
            //TransToobarViewController(ViewController);
            TransSplitViewController(ViewController);
            
        }

        private void TransSplitViewController(NSViewController controller)
        {
            NSView page = ViewController.View;
            var DefaultButtonRound = new NSButton() { TranslatesAutoresizingMaskIntoConstraints = false, Title = "DefaultButtonRound" };
            DefaultButtonRound.BezelStyle = NSBezelStyle.Rounded;
            page.AddSubview(DefaultButtonRound);
            NSSplitViewController splitController = null;
            DefaultButtonRound.Activated += (sender, e) =>
            {

                Console.WriteLine("Windows's Click " + nameof(ReloadMainWindowPage) + this.GetHashCode());
                
                var c = new SplitController(dataSource);
                controller.PresentViewControllerAsModalWindow(c.ViewController);
                c.SetToolBarForCurrentWindow();
            };
            

            var set = new ConstrainSet();
            set.Clone(page);
            set.AddConnect(DefaultButtonRound, NSLayoutAttribute.CenterX, page, NSLayoutAttribute.CenterX, 0)
                .AddConnect(DefaultButtonRound, NSLayoutAttribute.CenterY, page, NSLayoutAttribute.CenterY, 0);
            set.ApplyTo(page);
        }

        private void TransToobarViewController(NSViewController controller)
        {
            NSView page = ViewController.View;
            var DefaultButtonRound = new NSButton() { TranslatesAutoresizingMaskIntoConstraints = false, Title = "DefaultButtonRound" };
            DefaultButtonRound.BezelStyle = NSBezelStyle.Rounded;
            page.AddSubview(DefaultButtonRound);
            DefaultButtonRound.Activated += (sender, e) =>
            {

                Console.WriteLine("Windows's Click " + nameof(ReloadMainWindowPage) + this.GetHashCode());
                var c = new ToolbarController();
                Console.WriteLine("Windows's Clicked " + nameof(ReloadMainWindowPage) + this.GetHashCode());
                controller.PresentViewControllerAsModalWindow(c.ViewController);
                c.SetToolBarForCurrentWindow();
            };

            var set = new ConstrainSet();
            set.Clone(page);
            set.AddConnect(DefaultButtonRound, NSLayoutAttribute.CenterX, page, NSLayoutAttribute.CenterX, 0)
                .AddConnect(DefaultButtonRound, NSLayoutAttribute.CenterY, page, NSLayoutAttribute.CenterY, 0);

            set.ApplyTo(page);
        }

        private void ShowButtons(NSView page)
        {
            var PureTextButton = new NSButton(new CGRect(100, 100, 40, 20)) { TranslatesAutoresizingMaskIntoConstraints = false, Title = "PureTextButton" };
            //PureTextButton.Activated += ClickMeButton_Activated;

            PureTextButton.BezelStyle = NSBezelStyle.Rounded;
            PureTextButton.BezelColor = NSColor.White;
            PureTextButton.Bordered = false; //Important

            var PureBackgroundColorButton = new NSButton() { TranslatesAutoresizingMaskIntoConstraints = false, Title = "PureBackgroundColorButton" };
            PureBackgroundColorButton.BezelStyle = NSBezelStyle.Rounded;
            PureBackgroundColorButton.Bordered = false; //remove border
            PureBackgroundColorButton.WantsLayer = true; //add layer
            PureBackgroundColorButton.Layer.BackgroundColor = NSColor.SystemPinkColor.CGColor;
            PureBackgroundColorButton.Layer.CornerRadius = 5;//set background layer round
            //PureBackgroundColorButton.ContentTintColor = NSColor.Black;
            PureBackgroundColorButton.Layer.BorderColor = NSColor.SystemGreenColor.CGColor;
            PureBackgroundColorButton.Layer.BorderWidth = 1;

            var DefaultButton = new NSButton() { TranslatesAutoresizingMaskIntoConstraints = false, Title = "DefaultButton" };

            var DefaultButtonRound = new NSButton() { TranslatesAutoresizingMaskIntoConstraints = false, Title = "DefaultButtonRound" };
            DefaultButtonRound.BezelStyle = NSBezelStyle.Rounded;
            //DefaultButtonRound.BezelColor = NSColor.SystemPinkColor;


            page.AddSubview(PureTextButton);
            page.AddSubview(PureBackgroundColorButton);
            page.AddSubview(DefaultButton);
            page.AddSubview(DefaultButtonRound);


            var set = new ConstrainSet();
            set.Clone(page);
            set.AddConnect(DefaultButton, NSLayoutAttribute.CenterX, page, NSLayoutAttribute.CenterX, 0)
                .AddConnect(DefaultButton, NSLayoutAttribute.Top, page, NSLayoutAttribute.Top, 50)


                .AddConnect(DefaultButtonRound, NSLayoutAttribute.CenterX, page, NSLayoutAttribute.CenterX, 0)
                .AddConnect(DefaultButtonRound, NSLayoutAttribute.Top, DefaultButton, NSLayoutAttribute.Bottom, 10)


                .AddConnect(PureTextButton, NSLayoutAttribute.CenterX, page, NSLayoutAttribute.CenterX, 0)
                .AddConnect(PureTextButton, NSLayoutAttribute.Top, DefaultButtonRound, NSLayoutAttribute.Bottom, 10)
                .AddConnect(PureTextButton, NSLayoutAttribute.Width, 100, NSLayoutRelation.Equal)
                .AddConnect(PureTextButton, NSLayoutAttribute.Height, 50, NSLayoutRelation.Equal)


                .AddConnect(PureBackgroundColorButton, NSLayoutAttribute.CenterX, page, NSLayoutAttribute.CenterX, 0)
                .AddConnect(PureBackgroundColorButton, NSLayoutAttribute.Top, PureTextButton, NSLayoutAttribute.Bottom, 10)
                .AddConnect(PureBackgroundColorButton, NSLayoutAttribute.Width, 200, NSLayoutRelation.Equal)
                .AddConnect(PureBackgroundColorButton, NSLayoutAttribute.Height, 50, NSLayoutRelation.Equal);

            set.ApplyTo(page);


            //Event

            DefaultButton.Activated += (sender, e) =>
            {
                PureTextButton.Title = "Clicked A";

                var newSet = new ConstrainSet();
                newSet.Clone(page);
                newSet.Clear(PureTextButton);
                newSet.AddConnect(PureTextButton, NSLayoutAttribute.CenterX, page, NSLayoutAttribute.CenterX, 0)
                    .AddConnect(PureTextButton, NSLayoutAttribute.Top, page, NSLayoutAttribute.Top, 0)
                    .AddConnect(PureTextButton, NSLayoutAttribute.Width, 100, NSLayoutRelation.Equal)
                    .AddConnect(PureTextButton, NSLayoutAttribute.Height, 50, NSLayoutRelation.Equal);
                newSet.ApplyTo(page);
            };

            PureBackgroundColorButton.Activated += (sender, e) =>
            {
                var alert = new NSAlert()
                {
                    AlertStyle = NSAlertStyle.Critical,
                    InformativeText = "We need to save the document here...",
                    MessageText = "Save Document",
                };
                alert.RunModal();
            };
        }


        private void ShowText(NSView page)
        {
            var Lable = new NSTextField() { TranslatesAutoresizingMaskIntoConstraints = false };
            Lable.StringValue = "I'm a Lable, YYYYYYYYYYY";
            Lable.TextColor = NSColor.Blue;
            Lable.Editable = false;
            Lable.BackgroundColor = NSColor.Yellow;
            page.AddSubview(Lable);
            var set = new ConstrainSet();
            set.Clone(page);
            set.AddConnect(Lable, NSLayoutAttribute.CenterX, page, NSLayoutAttribute.CenterX, 0)
                .AddConnect(Lable, NSLayoutAttribute.CenterY, page, NSLayoutAttribute.CenterY, 0)
                //.AddConnect(Lable, NSLayoutAttribute.Width, 50)
                .AddConnect(Lable, NSLayoutAttribute.Height, 100);
            set.ApplyTo(page);
        }

    }
}

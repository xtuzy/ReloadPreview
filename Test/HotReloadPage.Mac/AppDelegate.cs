using System;
using AppKit;
using Foundation;
using HotReload;

namespace HotReloadPage.Mac
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        public static string IP = "192.168.0.107";
        public static int Port = 300;
        public static HotReloadClient ReloadClient;

        public AppDelegate()
        {
        }

        MainWindowController mainWindowController;


        public override void DidFinishLaunching(NSNotification notification)
        {
            ReloadClient = new HotReloadClient(IP, Port);
            ReloadClient.Start();

            mainWindowController = new MainWindowController();
            mainWindowController.Window.MakeKeyAndOrderFront(this);

            


            // Create a Status Bar Menu
            NSStatusBar statusBar = NSStatusBar.SystemStatusBar;

            var item = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
            item.Title = "Phrases";
            item.HighlightMode = true;
            item.Menu = new NSMenu("Phrases");

            var address = new NSMenuItem("Address");
            address.Activated += (sender, e) => {
                Console.WriteLine("Address Selected");
            };
            item.Menu.AddItem(address);

            var date = new NSMenuItem("Date");
            date.Activated += (sender, e) => {
                Console.WriteLine("Date Selected");
            };
            item.Menu.AddItem(date);

            var greeting = new NSMenuItem("Greeting");
            greeting.Activated += (sender, e) => {
                Console.WriteLine("Greetings Selected");
            };
            item.Menu.AddItem(greeting);

            var signature = new NSMenuItem("Signature");
            signature.Activated += (sender, e) => {
                Console.WriteLine("Signature Selected");
            };
            item.Menu.AddItem(signature);
        }

    }
}

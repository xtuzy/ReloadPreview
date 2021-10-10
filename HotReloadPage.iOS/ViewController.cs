
using Foundation;
using HotReload.Message.iOS;
using HotReloadPage.Edit.iOS;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Essentials;

namespace HotReloadPage.iOS
{
    public partial class ViewController : UIViewController
    {
        HotReloadClient Client;
        public static string IP = "192.168.0.107";
        public static int Port = 350;
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            View = new UIView();
            //第一次加载
            new ViewController_Init().Init(this,View);
            Client = new HotReload.Message.iOS.HotReloadClient(this, "HotReloadPage.Edit.iOS",IP,Port);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            Client.Start();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            Client.Stop();
        }

        //弹出输入服务器IP的对话框
        private void ShowInputDialog()
        {
            UIAlertController actionSheetController = UIAlertController.Create("服务器IP", null, UIAlertControllerStyle.Alert);

            actionSheetController.AddTextField((e) =>
            {
                e.Text = "192.168.0.";
                e.EditingChanged += (sender, arg) =>
                {
                    ViewController.IP = e.Text;
                };
            });
            UIAlertAction cancelAction = UIAlertAction.Create(@"取消", style: UIAlertActionStyle.Cancel, (e) => { });
            UIAlertAction commentAction = UIAlertAction.Create(@"确定", style: UIAlertActionStyle.Default, (e) =>
            {
                //do something...
                
            });
           

            actionSheetController.AddAction(cancelAction);
            actionSheetController.AddAction(commentAction);

            this.PresentViewController(actionSheetController, true, null);
        }
        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
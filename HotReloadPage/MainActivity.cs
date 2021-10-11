using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Android.Content;
using HotReload;

namespace HotReloadPage
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public static string IP = "192.168.0.107";
        public static int Port = 400;
        public static HotReloadClient ReloadClient;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            
            SetContentView(Resource.Layout.activity_main);
            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.frame_layout, new MainFragment());
            transaction.Commit();

            ReloadClient = new HotReloadClient(IP, Port);
            ReloadClient.Start();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //弹出输入服务器IP的对话框
        private void ShowInputDialog()
        {
            EditText inputServer = new EditText(this);
            inputServer.Text = "192.168.0.";
            AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            builder.SetTitle("填写服务IP")
                //.SetIcon(Resource.Drawable.ic_dialog_info)
                .SetView(inputServer)
                .SetNegativeButton("取消", new DialogInterfaceOnClickListenerNeg());
            builder.SetPositiveButton("确定", new DialogInterfaceOnClickListenerPos(inputServer));
            builder.Show();
        }
    }


    public class DialogInterfaceOnClickListenerNeg : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        public void OnClick(IDialogInterface dialog, int which)
        {
            dialog.Dismiss();
        }
    }

    public class DialogInterfaceOnClickListenerPos : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        EditText inputServer;
        public DialogInterfaceOnClickListenerPos(EditText inputServer) : base()
        {
            this.inputServer = inputServer;
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            String mMeetName = inputServer.Text;
            //do something...
            dialog.Dismiss();
            inputServer = null;
        }
    }
}


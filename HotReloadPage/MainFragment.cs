

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Fragment.App;
using HotReload.Message.Droid;
using HotReloadPage.Edit.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotReloadPage
{
    public class MainFragment : Fragment
    {
        HotReloadClient Client;
        public static string IP = "192.168.0.107";
        public static int Port = 400;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            Client = new HotReloadClient(this, "HotReloadPage.Edit.Droid", IP,Port);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            var rootView = new ConstraintLayout(this.Activity) { Id = View.GenerateViewId() };
            var page = new ConstraintLayout(this.Activity) { Id = View.GenerateViewId() };
            rootView.AddView(page, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            new MainFragment_Init().Init(this, page);
            return rootView;
        }

        public override void OnStart()
        {
            base.OnStart();
            Client.Start();
        }

        public override void OnStop()
        {
            base.OnStop();
            Client.Stop();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            Client.Dispose();
        }
    }
}
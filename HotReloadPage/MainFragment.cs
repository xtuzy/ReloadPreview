

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Fragment.App;
using HotReloadPage.Edit.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace HotReloadPage
{
    public class MainFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            var rootView = new ConstraintLayout(this.Activity) { Id = View.GenerateViewId() };
            var page = new ConstraintLayout(this.Activity) { Id = View.GenerateViewId() };
            rootView.AddView(page, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            new MainFragment_Init().Reload(this, page);
            return rootView;
        }

        public override void OnStart()
        {
            base.OnStart();
            MainActivity.ReloadClient.Reload += ReloadClient_Reload;
        }

        private void ReloadClient_Reload(string path)
        {
            //重新设置MainPage
            ((ViewGroup)this.View).RemoveAllViews();//根View不知道怎么替换,选择移除根View的子View
            var peerRootView = new ConstraintLayout(this.Activity);//根View的相邻子View
            ((ViewGroup)this.View).AddView(peerRootView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            MainActivity.ReloadClient.ReloadType<MainFragment_Init>(this, this.View);
        }

        public override void OnStop()
        {
            base.OnStop();
            MainActivity.ReloadClient.Reload -= ReloadClient_Reload;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
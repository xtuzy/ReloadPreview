using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ReloadPreview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotReloadPage
{
    public class ReloadPage : IReload
    {
        public void Reload(object controller, object view)
        {
            ViewGroup page = view as ViewGroup;
            page.SetBackgroundColor(Color.Blue);
            page.AddView(new TextView(page.Context) { Text = "Why? Do you know? .", TextSize = 100 });
            var button = new Button(page.Context);
            button.Text = "Click";
            button.Click += (sender, e) =>
            {
                Toast.MakeText(page.Context, "Clicked", ToastLength.Long).Show();
            };
            page.AddView(button);
        }
    }
}
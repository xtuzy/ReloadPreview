using Android.Content;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReloadPreview.Maui.Android.Demo
{
    public class MainPage  : RelativeLayout
    {
        private Button MainButton;

        public MainPage(Context? context) : base(context)
        {
            this.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            MainButton = new Button(context)
            {
                Text = "MainButton",
                LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            ((RelativeLayout.LayoutParams)(MainButton.LayoutParameters)).SetMargins(50, 50, 0, 0);
            this.AddView(MainButton);
            //this.AddView(new TextView(context) { Text = "Text" });
        }

        public View Get()
        {
            return MainButton;
        }
    }
}

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotReloadPage
{
    public class MainPage : ConstraintLayout
    {
        public MainPage(Context context) :
            base(context)
        {
            Initialize();
        }

        public MainPage(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public MainPage(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
        }
    }
}
using CoreGraphics;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace HotReloadPage.iOS
{
    [Register("MainPage")]
    public class MainPage : UIView
    {
        public MainPage()
        {
            Initialize();
        }

        public MainPage(RectangleF bounds) : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Red;
        }
    }
}
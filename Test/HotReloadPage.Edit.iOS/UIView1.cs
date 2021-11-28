using CoreGraphics;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace HotReloadPage.Edit.iOS
{
    [Register("UIView1")]
    public class UIView1 : UIView
    {
        public UIView1()
        {
            Initialize();
        }

        public UIView1(RectangleF bounds) : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Red;
        }
    }
}
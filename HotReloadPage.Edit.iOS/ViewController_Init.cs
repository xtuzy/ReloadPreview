using Foundation;
using HotReload.Message.iOS;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace HotReloadPage.Edit.iOS
{
    public partial  class ViewController_Init:IUIViewController_Init
    {
        UIView Page;
        UITextField text;
        public void Init(UIViewController controller,UIView page)
        {
            this.Page = page;
            Page.BackgroundColor = UIColor.Orange;

            text = new UITextField() { Text = "好的啊宝贝", TranslatesAutoresizingMaskIntoConstraints = false };
            Page.AddSubview(text);

            var button = new UIButton(UIButtonType.RoundedRect) { TranslatesAutoresizingMaskIntoConstraints = false };
            button.SetTitle("OKHkkjjj", UIControlState.Normal);
            button.SetTitleColor(UIColor.Black, UIControlState.Normal);
            Page.AddSubview(button);
            button.TouchUpInside += Button_Click;

            var skiaView = new SkiaSharp.Views.iOS.SKCanvasView() { TranslatesAutoresizingMaskIntoConstraints = false };
            skiaView.PaintSurface += SkiaView_PaintSurface;
            Page.Add(skiaView);
            

            /*var model = new TempModel();
            text.Text = model.GetString();*/

            text.CenterXAnchor.ConstraintEqualTo(Page.CenterXAnchor).Active = true;
            text.CenterYAnchor.ConstraintEqualTo(Page.CenterYAnchor).Active = true;

            /*view1.CenterXAnchor.ConstraintEqualTo(Page.CenterXAnchor).Active =true;
            view1.CenterYAnchor.ConstraintEqualTo(Page.CenterYAnchor).Active =true;
            view1.HeightAnchor.ConstraintEqualTo(50).Active = true;
            view1.WidthAnchor.ConstraintEqualTo(50).Active = true;*/

            button.CenterXAnchor.ConstraintEqualTo(Page.CenterXAnchor).Active = true;
            button.TopAnchor.ConstraintEqualTo(Page.TopAnchor, 50).Active = true;

            skiaView.CenterXAnchor.ConstraintEqualTo(Page.CenterXAnchor).Active = true;
            skiaView.CenterYAnchor.ConstraintEqualTo(Page.CenterYAnchor).Active = true;
            skiaView.HeightAnchor.ConstraintEqualTo(100).Active=true;
            skiaView.WidthAnchor.ConstraintEqualTo(100).Active=true;
        }

        private void SkiaView_PaintSurface(object sender, SkiaSharp.Views.iOS.SKPaintSurfaceEventArgs e)
        {
            var h = e.Info.Height;
            var w = e.Info.Width;
            e.Surface.Canvas.DrawRect(new SKRect(0, 0, w, h), new SKPaint() { Color = SKColors. Black});
        }

        private void Button_Click(object sender, System.EventArgs e)
        {
            text.TextColor = UIColor.Blue;
        }
    }
}
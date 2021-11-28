using AppKit;
using CoreGraphics;
using ReloadPreview;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotReloadPage.Edit.Mac
{
    public class ReloadMainWindow : IReload
    {
        NSView Page;
        NSButton ClickMeButton;
        public void Reload(object Controller, object view)
        {
            Page = (NSView)view;
            foreach(var v in Page.Subviews)
            {
                v.RemoveFromSuperview();
            }
            ClickMeButton = new NSButton(new CGRect(250, Page.Frame.Height - 100, 100, 30))
            {

                AutoresizingMask = NSViewResizingMask.MinYMargin
            };
            Page.AddSubview(ClickMeButton);
            ClickMeButton.Activated += ClickMeButton_Activated;
        }

        private void ClickMeButton_Activated(object sender, EventArgs e)
        {
            ClickMeButton.Title = "Success";
        }
    }
}

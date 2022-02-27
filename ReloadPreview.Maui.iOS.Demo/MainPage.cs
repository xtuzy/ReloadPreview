using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReloadPreview.Maui.iOS.Demo
{
    public class MainPage
    {
        public UIView Page;
        public MainPage(CGRect frame)
        {
             Page =  new UILabel(frame)
             {
                 BackgroundColor = UIColor.Red,
                 TextAlignment = UITextAlignment.Center,
                 Text = "Hello,net6-iOS!"
             };
        }

        public UIView Get()
        {
            return Page;
        }
    }
}

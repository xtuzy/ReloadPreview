using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReloadPreview.Maui.iOS.Demo
{
    internal class MainPage:IReload
    {
        public UIView Page;
        public MainPage(CGRect frame)
        {
             Page =  new UILabel(frame)
             {
                 BackgroundColor = UIColor.Yellow,
                 TextAlignment = UITextAlignment.Center,
                 Text = "Hello,net6-iOS!"
             };
        }

        public object Get()
        {
            return Page;
        }
    }
}

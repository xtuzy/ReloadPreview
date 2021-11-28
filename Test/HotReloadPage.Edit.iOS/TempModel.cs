using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace HotReloadPage.Edit.iOS
{
    public class TempModel
    {
        public string GetString()
        {
            return "牛啊"+Do();

        }

        public string Do()
        {
            return "哈弗看了觉得很疯狂骄傲和法律科技";
        }
    }
}
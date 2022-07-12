using ReloadPreview.Maui.Demo.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReloadPreview.Maui.Demo
{
    public class ReloadPageManager
    {
        public Page ReloadPage()
        {
            return HotReload.Instance.ReloadClass<SecondPage>();
        }
    }
}

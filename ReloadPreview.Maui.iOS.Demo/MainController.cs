using ReloadPreview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReloadPreview.Maui.iOS.Demo
{
    internal class MainController : UIViewController
    {
        public MainController(UIWindow window)
        {
            HotReload.Instance.Init("192.168.0.108");
            HotReload.Instance.Reload += () =>
            {
                dynamic view = HotReload.Instance.ReloadClass<MainPage>(window!.Frame);
                this.View = view.Get() as UIView;
            };
            this.View = new MainPage(window!.Frame).Page;
        }
    }
}

using Android.App;
using Android.Runtime;
using System;

namespace HotReloadPage
{
    [Application]
    public class MainApp : Application
    {
        public MainApp(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
        }

        public override void OnCreate()
        {
            Console.WriteLine("RelaodApp App Start:"+this.GetHashCode());
            base.OnCreate();
        }

        public override void OnTerminate()
        {
            Console.WriteLine("RelaodApp App Stop:" + this.GetHashCode());
            base.OnTerminate();
        }
    }
}
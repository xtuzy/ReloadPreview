# HotReloadPage
Reload object in Xamarin.iOS and Xamarin.Android let you enjoy coding UI

# How it work

Use socket send class library's .dll to android or ios app,then app load the method to run,
so if you use UIView as the parameter of the method, you can redraw UIView at the .dll.

# How to use
## First
For your project creat a xamarin.android or xamarin.ios class library project.
Install nuget [HotReload.iOS](https://www.nuget.org/packages/HotReload.iOS/) or [HotReload.Droid](https://www.nuget.org/packages/HotReload.Droid/) at your project and class library project.

At App.cs Creat static ReloadClient object:
```
using Android.App;
using Android.Runtime;
using System;

namespace MyAndroidApp
{
    [Application]
    public class App : Application
    {
        public static HotReload.HotReloadClient  Client;
        public App(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
        }

        public override void OnCreate()
        {
            //Here input the http and port information at server app
            Client = new HotReload.HotReloadClient("192.168.0.107", 400);
            Client.Start();
            base.OnCreate();
        }

        public override void OnTerminate()
        {
            Client.Stop();
            base.OnTerminate();
        }
    }
}
```

## Second
From Release Download [HotReload.Server.ConsoleApp.zip](https://github.com/xtuzy/HotReloadPage/releases/download/V1.0/HotReload.Server.ConsoleApp.zip), Unzip , Run it on Windows.
If need choose y/n , all choose y. 
If need input port number, input it, you need use it in you app.
If need input dll path, 

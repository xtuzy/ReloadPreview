# ReloadPreview

[![NuGet version(ReloadPreview)](https://img.shields.io/nuget/v/ReloadPreview?label=ReloadPreview)](https://www.nuget.org/packages/ReloadPreview/)

[VS2022ReloadExtension and ConsoleApp](https://github.com/xtuzy/ReloadPreview/releases)

## What is ReloadPreview

We know we can preview the ui when use xaml,xml,storyboard,swiftui, but if you want use C# code to write ui, no preview? Now, you can use this library to do it. 
It not only preview, because it need run app, that mean you can **show data and load service**, and debug by print info.

**Support platform**

- [x] Xamarin.Android
- [x] Xamarin.iOS
- [x] Xamarin.Mac
- [x] Wpf

Need other platform? override [`InvokeInMainThread`](https://github.com/xtuzy/ReloadPreview/blob/91de63909a1fb480e3a0f6ac7f6acf6f44bbe20d/ReloadPreview/ReloadClient.cs#L161) method.

## How it work

It base on Reflection to reload the dll. 


Use socket send class library's .dll to android or ios app,then app load the method in library to run,
so if you use UIView as the parameter of the method, you can redraw UIView at the .dll.

Very simple, you can do it by youself,let it more fit youself,such as auto build.

## How to use
I upload a example at youtube https://www.youtube.com/watch?v=nbjJQ9UNVDQ

### First
For your project creat a xamarin.android or xamarin.ios or xamarin.mac *Class Library* project.
Install [![NuGet version(ReloadPreview)](https://img.shields.io/nuget/v/ReloadPreview?label=ReloadPreview)](https://www.nuget.org/packages/ReloadPreview/) at your App project and *Class Library* project.

At Android's App.cs and iOS's Main.cs or AppDelegate.cs Creat static ReloadClient object, such as:
```
using Android.App;
using Android.Runtime;
using System;

namespace MyAndroidApp
{
    [Application]
    public class App : Application
    {
        public static ReloadClient  Client;
        public App(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
        }

        public override void OnCreate()
        {
            //Here input the http and port information at server app
            Client = new ReloadClient("192.168.0.107", 400);
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
At activity or fragment or viewcontroller load Reload event, 

```
 public partial class ViewController : UIViewController
{
    public ViewController(IntPtr handle) : base(handle)
    {
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        // Perform any additional setup after loading the view, typically from a nib.
        View = new UIView();
        // First time load
        new ReloadUIView().Reload(this,View);
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);
        //Subscribe Reload event
        AppDelegate.ReloadClient.Reload += ReloadClient_Reload;
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        AppDelegate.ReloadClient.Reload -= ReloadClient_Reload;
    }

    private void ReloadClient_Reload(object sender, EventArgs e)
    {
        //Here use you want redraw view as para.
        //ReloadUIView is a class in class library, it constains Reload method
        AppDelegate.ReloadClient.ReloadType<ReloadUIView>(this, this.View);
    }
}

```

At Class Library , you can create class extend IReload, then redraw View, you can do many things, *but you can't create a class extend android or ios's native object, and when you use nuget at  class library,also need install it at main app project.*
```
public  class ReloadUIView:IReload
{
    UIView Page;
    public void Reload(object controller,object view)
    {
        this.Page = (UIView)view;
        Page.BackgroundColor = UIColor.Green;
    }
}

```
### Second

From Release Download ReloadPreview.Server.ConsoleApp.zip Unzip , Run it on Windows.
From Release Download and Install Extension, use it to auto build you project at background thread when you save code.
If need choose y/n , all choose y. 
If need input port number, input it, you need use it in you app.
If need input dll path, copy the .dll path of the class library project generate.

### Final
Run app, let it install,then you don't need VisualStudio to run it,just need open it on Device or Emulater. Change class library code, At visual studio build it, app will change something.

## Tips

- 2021.10.21
  
  I found when you close "Edit and Continue" at Visual Studio 2022, you can use "Apply Changes" of Visual Studio to continue simple debug Android, At Android, "Apply Changes" will kill current Activity, and reload the Activity in apk, it can't let app auto load the new dll, so ReloadPreview can help you reload all the things of the activity. Limit is you just can use old breakpoint.

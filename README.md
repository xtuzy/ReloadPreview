
# ReloadPreview 
![image](https://github.com/xtuzy/ReloadPreview/blob/master/README.assets/hotreload.png)

[![NuGet version(ReloadPreview)](https://img.shields.io/nuget/v/ReloadPreview?label=ReloadPreview)](https://www.nuget.org/packages/ReloadPreview/)

[Command Line Server Tool](https://github.com/xtuzy/ReloadPreview/releases)

## What is ReloadPreview

We know we have a designer to preview the ui when use wpf's xaml, android's xml, ios's storyboard, ios's swiftui. But when you use Maui xaml or use C# code to write ui, no designer to preview! Now, you can use this library to preview ui when you use xaml or code describe ui.

It like xaml HotReload, because it need run app, but it not rely on VisualStudio, it only need app run. Another, it also work when you add new xaml.
It like designer, because it only preview Page, but it run at device, if you use it at Maui, you can get more correct ui than designer.

It have a shortcoming, slower than HotReload and designer, build WinUi project need 10s, 
**Support platform**


- [x] net6.0-android (tested)
- [x] net6.0-ios (tested, can't directly reload object that extend object-c)
- [x] net6.0 and netstandard (such as maui-windows,or avalonia,need load ReloadClass< Control > in UI thread)


Need other platform? Invoke UI thread when change ui at Reload event, or override [`InvokeInMainThread`](https://github.com/xtuzy/ReloadPreview/blob/91de63909a1fb480e3a0f6ac7f6acf6f44bbe20d/ReloadPreview/ReloadClient.cs#L161) method.

## How it work

After code changed, *Command Line Server Tool* use msbuild to create new dll, then use socket send the .dll to android or ios app, app can use reflection to create instance from dll. 
So, you can remove old view form window, and use this instance of new view to do something.

**Notice: you must let reloaded class is public, because it is reload another assembly.**

## How to use
Install and Run *Command Line Server* , input proj and target, such as `D:/RelodPreview/ReloadPreview.csproj -t=android`.
Them install nuget, use it, such as at MAUI app:
```
public App()
{
    HotReload.Instance.Init("192.168.0.108");
    InitializeComponent();
    HotReload.Instance.Reload += () =>
    {
        var view = HotReload.Instance.ReloadClass<ReloadPageManager>().ReloadPage();
        Console.WriteLine(view is null);
        MainPage = view;
    };
    MainPage = new MainPage();
}

public class ReloadPageManager
{
    public Page ReloadPage()
    {
        //You can specify Page that you want to reload after change code. It will create Page object use new code.
        return HotReload.Instance.ReloadClass<OtherPage>()
    }
}
```
At ios platform,you can't reload class that extend from native object-c class, such as UIView,UIViewController,you can reload view like this:
```
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
```
```
public class MainPage
{
    public UIView Page;
    public MainPage(CGRect frame)
    {
        Page =  new UILabel(frame)
        {
            BackgroundColor = UIColor.Red,
            TextAlignment = UITextAlignment.Center,
            Text = "Hello,net6-ios!"
        };
    }

    public UIView Get()
    {
        return Page;
    }
}
```
Notice:If you build at windows, you need let VisualStudio for Windows link to Mac, otherwise maybe generate dll is not correct.

At not Android and iOS, maybe you need load ReloadClass< T > in UI thread,such as at Avalonia:
```
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

#if DEBUG
        HotReload.Instance.Reload += () =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                var view = HotReload.Instance.ReloadClass<MainPage>();
                this.Content = view;
            });
        };
        HotReload.Instance.Init("192.168.0.108");
#endif
    }
}
```
## Tips

- 2021.10.21
  
  I found when you close "Edit and Continue" at Visual Studio 2022, you can use "Apply Changes" of Visual Studio to continue simple debug Android, At Android, "Apply Changes" will kill current Activity, and reload the Activity in apk, it can't let app auto load the new dll, so ReloadPreview can help you reload all the things of the activity. Limit is you just can use old breakpoint.
  

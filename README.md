# ReloadPreview

[![NuGet version(ReloadPreview)](https://img.shields.io/nuget/v/ReloadPreview?label=ReloadPreview)](https://www.nuget.org/packages/ReloadPreview/)

[Command Line Server](https://github.com/xtuzy/ReloadPreview/releases)

## What is ReloadPreview

We know we can preview the ui when use xaml,xml,storyboard,swiftui, but if you want use C# code to write ui, no preview? Now, you can use this library to do it. 
It not only preview, because it need run app, that mean you can **show data and load service**, and debug by print info.

**Support platform**

- [x] net6.0-android (tested)
- [x] net6.0-iOS (tested, can't reload object that extend object-c)
- [ ] new6.0-mac
- [ ] WPF
- [ ] Xamarin

Need other platform? Invoke UI thread when change ui at Reload event, or override [`InvokeInMainThread`](https://github.com/xtuzy/ReloadPreview/blob/91de63909a1fb480e3a0f6ac7f6acf6f44bbe20d/ReloadPreview/ReloadClient.cs#L161) method.

## How it work

After code changed, *Command Line Server* use msbuild to create new dll, then use socket send the .dll to android or ios app, app can use reflection to create instance from dll. 
So, you can remove old view form window, and use this instance of new view to do something.

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
        var view = HotReload.Instance.ReloadClass<MainPage>() as Page;
        Console.WriteLine(view is null);
        MainPage = view;
    };
    MainPage = new MainPage();
}
```

## Tips

- 2021.10.21
  
  I found when you close "Edit and Continue" at Visual Studio 2022, you can use "Apply Changes" of Visual Studio to continue simple debug Android, At Android, "Apply Changes" will kill current Activity, and reload the Activity in apk, it can't let app auto load the new dll, so ReloadPreview can help you reload all the things of the activity. Limit is you just can use old breakpoint.
  
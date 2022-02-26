using Android.Views;

namespace ReloadPreview.Maui.Android.Demo
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            HotReload.Instance.Init("192.168.0.108");
            base.OnCreate(savedInstanceState);
            HotReload.Instance.Reload += () =>
            {
                var view = HotReload.Instance.ReloadClass<MainPage>(this) as View;
                Console.WriteLine(view is null);
                SetContentView(view);
            };
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
        }
    }
}
namespace ReloadPreview.Maui.Demo
{
    public partial class App : Application
    {
        public App()
        {
            HotReload.Instance.Init("192.168.0.108");
            InitializeComponent();
            HotReload.Instance.Reload += () =>
            {
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    var view = HotReload.Instance.ReloadClass<MainPage>();
                    Console.WriteLine($"view.Get() is Lable:{view.Get()}");
                    MainPage = view;
                });
            };
            MainPage = new MainPage();
        }
    }
}
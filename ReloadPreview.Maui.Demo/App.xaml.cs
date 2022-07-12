namespace ReloadPreview.Maui.Demo
{
    public partial class App : Application
    {
        public App()
        {
            HotReload.Instance.Init("192.168.0.144");
            InitializeComponent();
            HotReload.Instance.Reload += () =>
            {
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    var view = HotReload.Instance.ReloadClass<ReloadPageManager>().ReloadPage();
                    MainPage = view;
                });
            };
            MainPage = new MainPage();
        }
    }
}
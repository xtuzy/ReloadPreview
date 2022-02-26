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
                var view = HotReload.Instance.ReloadClass<MainPage>() as Page;
                Console.WriteLine(view is null);
                MainPage = view;
            };
            MainPage = new MainPage();
        }
    }
}
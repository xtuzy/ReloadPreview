using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using HotReload;



namespace HotReloadPage.Edit.Droid
{
    public  class MainFragment_Init:IReload
    {
        public void Reload(object controller, object view)
        {
            ViewGroup page = view as ViewGroup;
            page.SetBackgroundColor(Color.Green);
            page.AddView(new TextView(page.Context) { Text="Why? Do you know? If you don't know?",TextSize=100});
        }
    }
}
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using HotReload.Message.Droid;


namespace HotReloadPage.Edit.Droid
{
    public  class MainFragment_Init:IFragment_Init
    {

        public void Init(Fragment controller, ViewGroup page)
        {
            page.SetBackgroundColor(Color.Orange);
            page.AddView(new TextView(page.Context) { Text="Why"});
        }
    }
}
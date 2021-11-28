using ReloadPreview;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace HotReloadPage.Edit.Wpf
{
    public class ReloadPage : IReload
    {
        public void Reload(object controller, object view)
        {
            Grid Page  = view as Grid;
            Page.Background = new SolidColorBrush(Colors.Blue);
            Page.InvalidateVisual();
        }
    }
}

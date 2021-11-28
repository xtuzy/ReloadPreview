using HotReloadPage.Edit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HotReloadPage.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.ReloadClient.Reload += ReloadClient_Reload;

        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            
        }

        private void ReloadClient_Reload(object sender, EventArgs e)
        {
            App.ReloadClient.ReloadType<ReloadPage>(this, Page);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            //App.ReloadClient.Reload -= ReloadClient_Reload;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Page.Background = new SolidColorBrush(Colors.Yellow);
            var button = new Button() { Width=10,Height=300,Margin=new Thickness(50,50,50,50),};
            Page.Children.Add(button);
            button.Click += Button_Click1;
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            Page.Background = new SolidColorBrush(Colors.Green);
        }
    }
}

using Setting;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ReloadPreview.Server.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ReloadFileModel> models = new ObservableCollection<ReloadFileModel>();
        private MySettings? Setting;
        internal Dictionary<string, (MessageServer, FileSystemWatcher)> ServerList = new Dictionary<string, (MessageServer, FileSystemWatcher)>();
        public MainWindow()
        {
            InitializeComponent();
            //读取设置
            Setting = MySettings.Read();
            if (Setting == null)
                Setting = new MySettings() { ReloadFileModels = new List<ReloadFileModel>() };
            foreach (var item in Setting.ReloadFileModels)
            {
                models.Add(item);
            }
            listview.ItemsSource = models;
        }

        private void AddReloadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var newModel = new ReloadFileModel() { Path = NewPathTextBox.Text, Port = NewPortTextBox.Text, State = false, };
            models.Add(newModel);
            Setting.ReloadFileModels.Add(newModel);
            MySettings.Save(Setting);
            NewPathTextBox.Text = "";
            NewPortTextBox.Text = "";
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var item = button.DataContext as ReloadFileModel;
            var path = item.Path;
            var port = item.Port;
            var state = item.State;
            if (state == false)//打开
            {
                // Create server
                try
                {
                    //打开Socket
                    var Server = new MessageServer(int.Parse(port));
                    if (Server != null)
                    {
                        this.Title = Server.MyIp.ToString();
                        Server.ConnectEvent += (sender, e) =>
                          {
                              listview.Dispatcher.Invoke(() =>
                              {
                                  item.LinkCount = Server.CurrentClients.Count;
                              });
                          };
                        //打开文件监听
                        var watcher = FileMinitor(path);
                        state = true;
                        item.State = true;
                        ServerList.Add(path, (Server,watcher));

                    }
                }
                catch (Exception ex)
                {

                }
            }
            else//关闭
            {
                try
                {
                    //关闭Socket
                    ServerList[path].Item1.Close();

                    //关闭文件监听
                    ServerList[path].Item2.EnableRaisingEvents = false;
                    ServerList[path].Item2.Dispose();
                    state = false;
                    item.State = false;
                    ServerList.Remove(path);
                }
                catch (Exception ex)
                {

                }

            }
        }

        FileSystemWatcher FileMinitor(string path)
        {
            string dllPath = string.Empty;
            //if path in ""
            if (path[0] == '\"' && path[path.Length - 1] == '\"')
            {
                dllPath = path.Substring(1, path.Length - 2);
            }
            else
                dllPath = path;

            var dllFileInfo = new FileInfo(dllPath);
            var dllDirectory = dllFileInfo.DirectoryName;
            var dllName = dllFileInfo.Name;
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = dllDirectory;
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.Size;
            // Only watch text files.
            watcher.Filter = dllName;

            var isSingleChange = false;
            // Add event handlers.
            watcher.Changed += (s, e) =>
            {
                //Console.WriteLine("File Changed");
                if (isSingleChange == false)
                {
                    isSingleChange = true;
                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(150);
                            //send dll to app
                            if (ServerList.ContainsKey(path))
                            {
                                ServerList[path].Item1?.SendFile(dllPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Send File Exception");
                        }
                        finally
                        {
                            await Task.Delay(5000);
                            isSingleChange = false;
                        }
                    });
                }
            };
            // Begin watching.
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var item = button.DataContext as ReloadFileModel;
            var path = item.Path;
            var state = item.State;
            if (state == true)//正在运行,先关闭Socket
            {
                //关闭Socket
                ServerList[path].Item1.Close();
                //关闭文件监听
                ServerList[path].Item2.EnableRaisingEvents = false;
                ServerList[path].Item2.Dispose();
                //移除
                ServerList.Remove(path);
            }
            models.Remove(item);
            Setting.ReloadFileModels.Remove(item);
            MySettings.Save(Setting);
        }
    }
}

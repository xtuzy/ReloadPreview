#if __ANDROID__
using Android.Views;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Fragment.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace HotReload.Message.Droid
{
    public class HotReloadClient:IDisposable
    {
        ClientProgram ClientProgram;
        Fragment Controller;
        string IP;
        int  Port;
        string NamespaceName;

        /// <summary>
        /// 在创建Fragment时调用.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="namespaceName">controller_Init所在命名空间</param>
        /// <param name="ip">服务器ip</param>
        public HotReloadClient(Fragment controller,string namespaceName,string ip,int port)
        {
            Controller = controller;
            NamespaceName = namespaceName;
            IP = ip;
            Port = port;
        }

        /// <summary>
        /// 在进入Fragment时调用(类似Activity.Start)
        /// </summary>
        public void Start()
        {
            //从网络获取Dll的任务
            var ClientTask = new Task(() =>
            {
                Console.WriteLine("创建代理");
                ClientProgram = new ClientProgram(IP,Port);
                ClientProgram.Connect();
                ClientProgram.AcceptedFileEvent += ClientProgram_AcceptedMessageEvent;
            });

            //500毫秒后显示填写服务器IP的对话框
            Task.Run(async () =>
            {
                await Task.Delay(500);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ClientTask.Start();
                });
            });
        }

         /// <summary>
         /// 接受服务器消息时调用
         /// </summary>
         /// <param name="message"></param>
        private void ClientProgram_AcceptedMessageEvent(string path)
        {
            //重载页面属于Ui线程,在主线程中处理
            var className = NamespaceName + '.' + Controller.GetType().Name + "_Init";
            MainThread.BeginInvokeOnMainThread(() => 
            {
                DynamicLoadPage(path, Controller, className);
            });
              
        }

        private void DynamicLoadPage(string path, Fragment controller, string className)
        {
            //var b = assemblyStream.ToArray();
            //Console.WriteLine("此次接受Dll大小:" + b.Length);
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFile(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("加载dll出错:" + ex);
                return;
            }

            //重新设置MainPage
            ((ViewGroup)controller.View).RemoveAllViews();//根View不知道怎么替换,选择移除根View的子View
            var peerRootView = new ConstraintLayout(controller.Activity);//根View的相邻子View

            ((ViewGroup)controller.View).AddView(peerRootView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            //从Dll解析出PageEdit,执行
            if (assembly != null)
            {
                try
                {
                    var type = assembly.GetType(className);//获得类

                    if (type != null)
                    {
                        dynamic dynamic_ec = Activator.CreateInstance(type);//动态创建类
                        dynamic_ec.Init(controller, (ViewGroup)peerRootView);//执行
                        controller.View.Invalidate();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private void DynamicLoadPage(MemoryStream assemblyStream,Fragment controller,string className)
        {
            var b = assemblyStream.ToArray();
            Console.WriteLine("此次接受Dll大小:" + b.Length);
            Assembly assembly=null;
            try
            {
                assembly = Assembly.Load(b);
            }
            catch (Exception ex)
            {
                Console.WriteLine("加载dll出错:"+ex);
                return;
            }

            //重新设置MainPage
            ((ViewGroup)controller.View).RemoveAllViews();//根View不知道怎么替换,选择移除根View的子View
            var peerRootView = new ConstraintLayout(controller.Activity);//根View的相邻子View

            ((ViewGroup)controller.View).AddView(peerRootView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            //从Dll解析出PageEdit,执行
            if (assembly != null)
            {
                try
                {
                    var type = assembly.GetType(className);//获得类

                    if (type != null)
                    {
                        dynamic dynamic_ec = Activator.CreateInstance(type);//动态创建类
                        dynamic_ec.Init(controller, (ViewGroup)peerRootView);//执行
                        controller.View.Invalidate();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// 在Fragment进入后台时调用(类似Activity.Stop),还可以重启
        /// </summary>
        public void Stop()
        {
            if (ClientProgram != null)
            {
                ClientProgram.Close();
            }
        }

        /// <summary>
        /// 在退出Fragment时调用(类似Activity.Finish),不可重启
        /// </summary>
        public void Dispose()
        {
            Controller = null;

            if (ClientProgram != null)
            {
                ClientProgram.Close();
            }
        }
    }

    public interface IFragment_Init
    {
        public void Init(Fragment controller, ViewGroup page);
    }
}
#endif
#if __IOS__
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Essentials;

namespace HotReload.Message.iOS
{
    public class HotReloadClient:IDisposable
    {
        ClientProgram ClientProgram;
        UIViewController Controller;
        string IP;
        int Port;
        string NamespaceName;

        public HotReloadClient(UIViewController controller, string namespaceName, string ip,int port)
        {
            Controller = controller;
            NamespaceName = namespaceName;
            IP = ip;
            Port = port;
        }

        
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

            /////500毫秒后
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
            
                    //加载完dll,在主线程中处理
                    var className = NamespaceName + '.' + Controller.GetType().Name + "_Init";
                    MainThread.BeginInvokeOnMainThread(() =>
                    {

                        DynamicLoadPage(path, Controller, className);
                        //DynamicLoadPage(m, Controller, className);
                        
                    });
              
        }

        private void DynamicLoadPage(string path, UIViewController controller, string className)
        {
           /* var b = assemblyStream.ToArray();
            Console.WriteLine("此次接受Dll大小:" + b.Length);*/
            Assembly assembly = Assembly.LoadFile(path);
            //重新设置MainPage
            controller.View = new UIView() { BackgroundColor = UIColor.Green };
            //从Dll解析出PageEdit,执行
            if (assembly != null)
            {
                try
                {
                    var type = assembly.GetType(className);//获得类

                    if (type != null)
                    {
                        dynamic dynamic_ec = Activator.CreateInstance(type);//动态创建类
                        dynamic_ec.Init(controller, controller.View);//执行
                        controller.View.SetNeedsDisplay();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void DynamicLoadPage(MemoryStream assemblyStream, UIViewController controller,string className)
        {
            var b = assemblyStream.ToArray();
            Console.WriteLine("此次接受Dll大小:" + b.Length);
            Assembly assembly = Assembly.Load(b);
            //重新设置MainPage
            controller.View = new UIView() { BackgroundColor =UIColor.Green};
            //从Dll解析出PageEdit,执行
            if (assembly != null)
            {
                try
                {
                    var type = assembly.GetType(className);//获得类

                    if (type != null)
                    {
                        dynamic dynamic_ec = Activator.CreateInstance(type);//动态创建类
                        dynamic_ec.Init(controller, controller.View);//执行
                        controller.View.SetNeedsDisplay();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void Stop()
        {
            if (ClientProgram != null)
            {
                ClientProgram.Close();
            }
        }

        public void Dispose()
        {
            Controller = null;
           

            if (ClientProgram != null)
            {
                ClientProgram.Close();
            }
        }
    }

    public interface IUIViewController_Init
    {
        void Init(UIViewController controller, UIView page);
    }
}
#endif
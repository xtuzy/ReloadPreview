#if ! __CONSOLE__
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if __WPF__
               
#elif WINDOWS_UWP

#elif __MACOS__
                
#elif __ANDROID__
using Android.OS;
#elif __IOS__
#else
#endif
namespace ReloadPreview
{
    /// <summary>
    /// Reload dll's client.<br/>
    /// Notice:<br/>
    /// 1.Android app need set internet permission at manifest.xml <br/>
    /// 2.If not xamarin.android,ios,mac,wpf,uwp, you should load ReloadType in ui thread in Reload event, <br/>
    /// because normally set ui element need ui thread, you also can extend this class, realize it in InvokeInMainThread method.
    /// </summary>
    public class ReloadClient
    {
        /// <summary>
        /// You can use this at all project to reload, not need recreate client in preview project.
        /// </summary>
        public static ReloadClient GlobalInstance;
        MessageClient MessageClientProgram;
        string IP;
        int Port;

        /// <summary>
        /// it save the dll data, share to everywhere that need reload dll.
        /// </summary>
        MemoryStream memoryStream = null;

        /// <summary>
        /// Reload dll event,it will be loaded at MainThread
        /// </summary>
        public event Action Reload;

        /// <summary>
        /// Creat client, Use it at App start
        /// </summary>
        /// <param name="ip">server ip</param>
        /// <param name="port">server port</param>
        public ReloadClient(string ip, int port)
        {
            IP = ip;
            Port = port;
        }

        /// <summary>
        /// Use at App start<br/>
        /// start client,start accept dll from server.
        /// </summary>
        public void Start()
        {
            var ClientTask = new Task(() =>
            {
                Console.WriteLine("Creat Client.");
                MessageClientProgram = new MessageClient(IP, Port);
                MessageClientProgram.Connect();

                MessageClientProgram.AcceptedStreamEvent += (stream) =>
                {
                    memoryStream = stream;
                    InvokeInMainThread(() =>
                    {
                        if(Reload != null)
                            Reload.Invoke();
                    });

                };

            });
            //Delay 500ms, Avoid start slowly?
            Task.Run(async () =>
            {
                await Task.Delay(500);
                InvokeInMainThread(() => ClientTask.Start());
            });
        }

        /// <summary>
        /// Reload Type that extend IReload.<br/>
        /// You should load this method in event, you also should load this method in Controller's init, <br/>
        /// that will let you can load new assemble when navigate to different Controller.
        /// </summary>
        /// <param name="controller">object need deal with</param>
        /// <param name="view">object need deal with</param>
        public void ReloadType<T>(object controller, object view) where T : IReload
        {
            if (memoryStream == null) return;
            var classFullName = typeof(T).FullName;
            Console.WriteLine("Will reload class {0}.", classFullName);
            Assembly assembly = null;
            try
            {
                    assembly = Assembly.Load(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when load dll:{0}", ex);
                return;
            }

            //get class by reflect 
            if (assembly != null)
            {
                try
                {
                    var type = assembly.GetType(classFullName);//get class

                    if (type != null)
                    {
                        dynamic dynamic_ec = Activator.CreateInstance(type);//creat object
                        dynamic_ec.Reload(controller, view);//load method
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// stop client, stop accept dll from server.
        /// </summary>
        public void Stop()
        {
            if (MessageClientProgram != null)
            {
                MessageClientProgram.Close();
            }
        }





#if __WPF__
               
#elif WINDOWS_UWP

#elif __MACOS__
                
#elif __ANDROID__
        static volatile Handler handler;
#elif __IOS__
#else
#endif

        /// <summary>
        /// Xamarin.Essential no WPF,so not use it.<br/>
        /// https://github.com/xamarin/Essentials/tree/8657192a8963877e389a533b8feb324af6f89c8b/Xamarin.Essentials/
        /// </summary>
        /// <param name="action"></param>
        public virtual void InvokeInMainThread(Action action)
        {
#if __WPF__
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    action.Invoke();
                });
#elif WINDOWS_UWP
            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView?.CoreWindow?.Dispatcher;

            if (dispatcher == null)
                throw new InvalidOperationException("Unable to find main thread.");
            dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => action()).AsTask().WatchForError();
#elif __MACOS__
                AppKit.NSApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    action.Invoke();
                });
#elif __ANDROID__
            if (handler?.Looper != Android.OS.Looper.MainLooper)
             handler = new Handler(Looper.MainLooper);
            handler.Post(action);
#elif __IOS__
            Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
#else
            action.Invoke();
#endif
        }

    }

    /// <summary>
    /// Reload class in class library that must extend this.
    /// </summary>
    public interface IReload
    {
        /// <summary>
        /// When dll reload,will reload this method, you can deal with view in this.<br/>
        /// Notice:<br/>
        /// 1.If you creat model class and viewmodel at this class library,<br/> 
        /// you can load then at this method, or after this method.<br/>
        /// But when you creat class at this class library that extend native object of android or ios,<br/>
        /// you can't load them, you should creat them at other class library, <br/>
        /// then reference it at main project and this class library project, <br/>
        /// because extend native class will generate some native language things, <br/>
        /// reload just reload dll.<br/>
        /// 2.According mvvm,best practice is use share library store viewmodel and model,<br/>
        /// you can use test project reference them to test,<br/>
        /// and also use this class library reference them to design ui.<br/>
        /// </summary>
        /// <param name="controller">maybe activity, fragment, uiviewcontroller, or anything, just need you know what it is</param>
        /// <param name="view">maybe view,viewgroup, uiview, or anything, just need you know what it is</param>
        void Reload(object controller, object view);
    }

#if WINDOWS_UWP
    /// <summary>
    /// https://github.com/xamarin/Essentials/blob/8657192a8963877e389a533b8feb324af6f89c8b/Xamarin.Essentials/MainThread/MainThreadExtensions.uwp.cs
    /// </summary>
    internal static partial class MainThreadExtensions
    {
        internal static void WatchForError(this Windows.Foundation.IAsyncAction self) =>
            self.AsTask().WatchForError();

        internal static void WatchForError<T>(this Windows.Foundation.IAsyncOperation<T> self) =>
            self.AsTask().WatchForError();

        internal static void WatchForError(this Task self)
        {
            var context = SynchronizationContext.Current;
            if (context == null)
                return;

            self.ContinueWith(
                t =>
                {
                    var exception = t.Exception.InnerExceptions.Count > 1 ? t.Exception : t.Exception.InnerException;

                    context.Post(e => { throw (Exception)e; }, exception);
                }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
        }
    }
#endif
}



#endif
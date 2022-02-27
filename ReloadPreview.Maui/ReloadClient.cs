using System.Reflection;
using System.Text.Json;

#if ANDROID
using Android.OS;
#endif
namespace ReloadPreview
{
    /// <summary>
    /// Reload dll's client.<br/>
    /// Notice:<br/>
    /// 1.Android app need set internet permission at manifest.xml <br/>
    /// 2.If not net6.0-ios,net6.0-android,net6.0-windows, Reload event invoked not in UI thread, <br/>
    /// you need invoke UI thread by youself, or you can extend this class, rewrite it in InvokeInMainThread method.
    /// </summary>
    public class HotReload
    {
        /// <summary>
        /// You can use this at all project to reload, not need recreate client in preview project.
        /// </summary>
        public static HotReload Instance = new HotReload();

        /// <summary>
        /// 替代Ioc来暂时存储ViewModel,以在Reload时保存页面状态.
        /// </summary>

        public Dictionary<string, object> Datas = new Dictionary<string, object>();
        /// <summary>
        /// 存储类型,以在Reload项目中使用
        /// </summary>
        public Dictionary<string, Type> Types = new Dictionary<string, Type>();

        MessageClient MessageClientProgram;
        string IP;
        int Port;

        /// <summary>
        /// it save the dll data, share to everywhere that need reload dll.
        /// </summary>
        MemoryStream memoryStream = null;

        /// <summary>
        /// Reload dll event,it will be loaded at UI Thread when target is net6.0-ios,net6.0-android,net6.0-windows,
        /// if others, it will be loaded at other thread, so if you want update ui, you need Invoke UI Thread.
        /// </summary>
        public event Action Reload;

        private HotReload()
        {
        }

        /// <summary>
        /// Start client,start accept dll from server for reload.
        /// </summary>
        /// <param name="ip">server ip</param>
        /// <param name="port">server port</param>
        public void Init(string ip, int port=500)
        {
            IP = ip;
            Port = port;

            Task.Run(() =>
            {
                Console.WriteLine("Creat Client.");
                MessageClientProgram = new MessageClient(IP, Port);
                MessageClientProgram.Connect();

                MessageClientProgram.AcceptedStreamEvent += (stream) =>
                {
                    memoryStream = stream;
                    InvokeInMainThread(() =>
                    {
                        if (Reload != null)
                            Reload.Invoke();
                    });
                };
            });
        }

        public dynamic ReloadClass<T>(object arg1=null,object arg2=null,object arg3=null)
        {
            if (memoryStream == null) return null;
            var classFullName = typeof(T).FullName;
            Console.WriteLine("Will Reload Class {0}.", classFullName);
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error When Load Dll:{0}", ex);
                return null;
            }

            //get class by reflect 
            if (assembly != null)
            {
                try
                {
                    var type = assembly.GetType(classFullName);//get class

                    if (type != null)
                    {
                        if(arg3!=null)
                            return Activator.CreateInstance(type,new object[] {arg1,arg2,arg3});//creat object
                        if (arg2 != null)
                            return Activator.CreateInstance(type, new object[] { arg1, arg2});//creat object
                        if (arg1 != null)
                            return Activator.CreateInstance(type, new object[] { arg1});//creat object
                        return Activator.CreateInstance(type);//creat object
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error When GetType From Assembly:{0}", ex.Message);
                }
            }
            return null;
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

#if  ANDROID
        static volatile Handler handler;
#endif

        /// <summary>
        /// Xamarin.Essential no WPF,so not use it.<br/>
        /// https://github.com/xamarin/Essentials/tree/8657192a8963877e389a533b8feb324af6f89c8b/Xamarin.Essentials/
        /// </summary>
        /// <param name="action"></param>
        public virtual void InvokeInMainThread(Action action)
        {
#if WINDOWS
            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView?.CoreWindow?.Dispatcher;

            if (dispatcher == null)
                throw new InvalidOperationException("Unable to find main thread.");
            dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => action()).AsTask().WatchForError();
            // #if WPF
            //                System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //                {
            //                    action.Invoke();
            //                });
            // #elif WINDOWS_UWP
            //            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView?.CoreWindow?.Dispatcher;

            //            if (dispatcher == null)
            //                throw new InvalidOperationException("Unable to find main thread.");
            //            dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => action()).AsTask().WatchForError();
#elif MACOS
            AppKit.NSApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                action.Invoke();
            });
#elif ANDROID
            if (handler?.Looper != Android.OS.Looper.MainLooper)
             handler = new Handler(Looper.MainLooper);
            handler.Post(action);
#elif IOS
            Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
#else
            action.Invoke();
#endif
        }

    }

    #if __IOS__ || __MACCATALYST__
    /// <summary>
    /// Reload class in class library that must extend this.
    /// </summary>
    public interface IReload
    {
        /// <summary>
        /// Beacuse Apple's limit, if you want get object at you want reloaded class,
        /// you can't do it like other platform, you need realize this method to get.
        /// </summary>
        /// <returns></returns>
        object Get();
    }
    #endif

#if WINDOWS
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

    /// <summary>
    /// Reload中需要对存储ViewModel做序列化保存才能持续使用,这个类做此用.
    /// </summary>
    public static class ReloadData
    {
        /// <summary>
        /// Reload dll 时会导致类不相等,无法直接使用as转换,因此通过序列化反序列化转换和存储.
        /// 没有已存储的数据时返回null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public static T Reload<T>(string dataKey = null) where T : class
        {
            if (dataKey == null)
                dataKey = typeof(T).Name;
            ///如果包含,则存储了,反序列化获取
            if (ReloadPreview.HotReload.Instance.Datas.ContainsKey(dataKey))
            {
                var viewmodel = ReloadPreview.HotReload.Instance.Datas[dataKey] as string;
                return JsonSerializer.Deserialize<T>(viewmodel);
            }
            else//如果不包含,返回null
            { return default; }
        }

        /// <summary>
        /// 保存一次data,保存后可以持续在Reload中使用该data保存的数据.
        /// 默认存储data的键值为类型名,如果需要存储同类型的data多个,请设置不同键值.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataKey"></param>
        /// <param name="data"></param>
        public static void Save<T>(T data, string dataKey = null) where T : class
        {
            if (dataKey == null)
                dataKey = typeof(T).Name;
            var str = JsonSerializer.Serialize<T>(data);
            if (ReloadPreview.HotReload.Instance.Datas.ContainsKey(dataKey))
            {
                ReloadPreview.HotReload.Instance.Datas[dataKey] = str;
            }
            else
            {
                ReloadPreview.HotReload.Instance.Datas.Add(dataKey, str);
            }
        }

        /// <summary>
        /// 删除存储的data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datalKey"></param>
        public static void Delete<T>(string datalKey = null) where T : class
        {
            if (datalKey == null)
                datalKey = typeof(T).Name;
            if (ReloadPreview.HotReload.Instance.Datas.ContainsKey(datalKey))
            {
                ReloadPreview.HotReload.Instance.Datas.Remove(datalKey);
            }
        }
    }

    public static class ReloadType
    {
        /// <summary>
        /// 记录已经创建了这种类型,之后可以在其他项目中使用
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="theType"></param>
        public static void Record(string TypeName, Type theType)
        {
            if (ReloadPreview.HotReload.Instance.Types.ContainsKey(TypeName)) return;
            ReloadPreview.HotReload.Instance.Types.Add(TypeName, theType);
        }

        /// <summary>
        /// 新建该类型的对象
        /// </summary>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public static object NewInstance(string TypeName)
        {
            if (!ReloadPreview.HotReload.Instance.Types.ContainsKey(TypeName)) return null;
            try
            {
                dynamic dynamic_ec = Activator.CreateInstance(ReloadPreview.HotReload.Instance.Types[TypeName]);
                return dynamic_ec as object;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

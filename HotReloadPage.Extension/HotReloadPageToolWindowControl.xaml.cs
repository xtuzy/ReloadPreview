using ClientApp;
using Microsoft.CSharp;
using ServerApp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

namespace HotReloadPage.Extension
{
    /// <summary>
    /// Interaction logic for HotReloadPageToolWindowControl.
    /// </summary>
    public partial class HotReloadPageToolWindowControl : UserControl
    {
        static ServerAppProgram Server;
        /// <summary>
        /// 需要安卓或者iOS项目的地址,用来加载正确的using数据
        /// 如"I:\PlayCode\HotReloadPage\HotReloadPage"
        /// </summary>
        static string ClientAppProjectMainPath =string.Empty;

        public static void Update(string fileText)
        {
            //编译
            var asm = DebugRun(fileText).CompiledAssembly;
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream assemblyStream = new MemoryStream();
            formatter.Serialize(assemblyStream, asm);
            //服务端发送assembly
            if (Server != null)
                Server.SendMessage(assemblyStream.ToArray());

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HotReloadPageToolWindowControl"/> class.
        /// </summary>
        public HotReloadPageToolWindowControl()
        {
            this.InitializeComponent();
            ProjectPath_TextBox.TextChanged += ProjectPath_TextBox_TextChanged;
        }

        private void ProjectPath_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClientAppProjectMainPath = ProjectPath_TextBox.Text;
        }



        /// <summary>
        /// 动态编译并执行代码,参考https://www.cnblogs.com/swtool/p/7053104.html
        /// </summary>
        /// <param name="code">代码</param>
        /// <returns>返回输出内容</returns>
        public static CompilerResults DebugRun(string code)
        {
            //ICodeCompiler complier = new CSharpCodeProvider().CreateCompiler();
            CSharpCodeProvider provider = new CSharpCodeProvider();
            //设置编译参数
            CompilerParameters paras = new CompilerParameters();


            foreach (var dllName in AnalysisReferencedAssemblies(code))
            {
                paras.ReferencedAssemblies.Add(dllName);
            }
            //是否内存中生成输出 
            paras.GenerateInMemory = true;
            //是否生成可执行文件
            paras.GenerateExecutable = false;
            paras.OutputAssembly = @"D:\temp" + ".dll";

            //编译代码
            CompilerResults result = provider.CompileAssemblyFromSource(paras, code);
            return result;
        }

        /// <summary>
        /// 分析using的dll位置
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        static List<string> AnalysisReferencedAssemblies(string code)
        {
            var allDllList = new List<string>();
            //引入第三方dll
            allDllList.Add("System.dll");
            allDllList.Add(Assembly.GetExecutingAssembly().Location);//扩展的路径
            // 引入自定义dll
            //list.Add(@"D:\自定义方法\自定义方法\bin\LogHelper.dll");

            //分析全部程序集名字
            string[] codeLineArray;
            if (code.Contains("\r\n"))
                codeLineArray = code.Replace("\r\n", "\n").Split('\n');
            else
                codeLineArray = code.Split('\n');
            var allUsingList = new List<string>();
            foreach (var line in codeLineArray)
            {
                if (line.Contains("using "))
                {
                    var l = line;
                    var usingName = l.Replace("using", "").Replace(";", "").Replace(" ", "");
                    allUsingList.Add(usingName);
                }
            }

            //安卓和iOS的部分Nuget依赖信息在
            //"I:\PlayCode\HotReloadPage\HotReloadPage\obj\Debug\110\HotReloadPage.csproj.FileListAbsolute.txt"
            //"I:\PlayCode\HotReloadPage\HotReloadPage.iOS\obj\iPhoneSimulator\Debug\HotReloadPage.iOS.csproj.FileListAbsolute.txt"
            string inforFilePath = string.Empty;
            if (ClientAppProjectMainPath == string.Empty)
                return null;

            var t = ClientAppProjectMainPath.Split('\\');
            var projectName = t[t.Length - 1];
            if (code.Contains("using Android"))
            {
                inforFilePath = ClientAppProjectMainPath + @"\obj\Debug\110\" + projectName + ".csproj.FileListAbsolute.txt";
            }
            else if (code.Contains("using UIKit"))
            {
                inforFilePath = ClientAppProjectMainPath + @"\obj\iPhoneSimulator\Debug\" + projectName + ".csproj.FileListAbsolute.txt";
            }

            //分析属于nuget下载的程序集名字和所在位置
            var nugetDllList = new List<string>();
            var inforFileText = File.OpenText(inforFilePath).ReadToEnd();
            string[] nugetInforFileLineArray;
            if (inforFileText.Contains("\r\n"))
                nugetInforFileLineArray= inforFileText.Replace("\r\n","\n").Split('\n');
            else
                nugetInforFileLineArray= inforFileText.Split('\n');
            var nugetUsingList = new List<string>();
            foreach (var line in nugetInforFileLineArray)
            {
                if (line.Contains(".dll"))
                {
                    nugetDllList.Add(line);
                    allDllList.Add(line);
                    var temp = line.Split('\\');
                    var dllName = temp[temp.Length - 1];
                    var usingName = dllName.Replace(".dll","");
                    nugetUsingList.Add(usingName);
                }
            }

            //分析属于系统自带的程序集
            foreach(var usingName in allUsingList)
            {
                var isSystemUsing = true;
                foreach(var nugetName in nugetUsingList)
                {
                    if (nugetName.Contains(usingName))
                        isSystemUsing = false;
                }
                if (isSystemUsing)
                {
                    allDllList.Add(usingName);
                }
            }

            return allDllList;
        }

        private void StartServer_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Server == null)
                Server = ServerAppProgram.CreatServerAppProgram();
            IP_TextBlock.Text = Server.MyIp.ToString();
            Server.AcceptedConnectEvent += (s, ex) =>
            {
                Client_TextBlock.Text = "已连接";
                Client_TextBlock.InvalidateVisual();
            };

            Server.AcceptedMessageEvent += (s, ex) =>
            {

            };
        }

        private void StopServer_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Server != null)
                Server.Dispose();
        }
    }
}
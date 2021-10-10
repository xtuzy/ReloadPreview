using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HotReloadPage.Server.ConsoleApp
{

    internal class CompileClassServer
    {
        /// <summary>
        /// 需要安卓或者iOS项目的地址,用来加载正确的using数据
        /// 如"I:\PlayCode\HotReloadPage\HotReloadPage"
        /// </summary>
        public string ClientAppProjectMainPath;

        /// <summary>
        /// 动态编译并执行代码,参考https://www.cnblogs.com/swtool/p/7053104.html
        /// </summary>
        /// <param name="code">代码</param>
        /// <returns>返回输出内容</returns>
        public MemoryStream DebugRun(string code)
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
            paras.GenerateInMemory = false;
            //是否生成可执行文件
            paras.GenerateExecutable = false;
            paras.OutputAssembly =  @"temp.dll";

            //编译代码
            CompilerResults result = provider.CompileAssemblyFromSource(paras, code);
            return ResultAnalysis(result);
        }

        /// <summary>
        /// 分析using的dll位置
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        List<string> AnalysisReferencedAssemblies(string code)
        {
            var allDllList = new List<string>();
            //引入第三方dll
            allDllList.Add("System.dll");
            allDllList.Add("System.Collections.dll");
            allDllList.Add("System.Core.dll");
            allDllList.Add("System.IO.dll");
            //allDllList.Add("System.Linq.dll");

            if (code.Contains("using Android"))
            {
                allDllList.Add("Mono.Android.dll");
            }
            else if (code.Contains("using UIKit"))
            {
                allDllList.Add("Xamarin.iOS.dll");
            }
            else
            {
                return allDllList;
            }

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

            //分析属于该项目的程序集名字和所在位置
            var nugetDllList = new List<string>();
            var inforFileText = File.OpenText(inforFilePath).ReadToEnd();
            string[] nugetInforFileLineArray;
            if (inforFileText.Contains("\r\n"))
                nugetInforFileLineArray = inforFileText.Replace("\r\n", "\n").Split('\n');
            else
                nugetInforFileLineArray = inforFileText.Split('\n');
            var nugetUsingList = new List<string>();
            foreach (var line in nugetInforFileLineArray)
            {
                if (line.Contains(".dll"))
                {
                    nugetDllList.Add(line);
                    var temp = line.Split('\\');
                    var dllName = temp[temp.Length - 1];
                    var usingName = dllName.Replace(".dll", "");
                    nugetUsingList.Add(usingName);
                }
            }

            //分析属于该文件的程序集,可能会有错误,因为多个命名空间会属于同一个dll
            foreach (var usingName in allUsingList)
            {
                foreach (var nugetName in nugetDllList)
                {
                    if (nugetName.Contains(usingName+".dll"))
                        allDllList.Add(nugetName);
                }
            }

            return allDllList;
        }

        MemoryStream ResultAnalysis(CompilerResults result)
        {
            if(result.Errors.Count> 0)
            {
                foreach (CompilerError error in result.Errors)
                    Console.WriteLine(error.ErrorText);
                return null;
            }

            var stream = new MemoryStream();
            using (var fileStream = File.OpenRead(result.PathToAssembly))
            {
                fileStream.CopyTo(stream);
                return stream;
            }
                
        }
    }
}

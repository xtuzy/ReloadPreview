using Mono.Options;
using ReloadPreview;
using Spectre.Console;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace ReloadPreview.Maui.CommandLine
{
    class Program
    {
        private static MessageServer Server;
        private static FileWatcher FileWatcher;
        private static string Target;
        private static string Configuration;
        private static string CsprojRootFolder;
        private static string? CsprojFileFullPath;
        private static string NewAssembleFilePath = string.Empty;

        static async Task Main(string[] args)
        {
        REINPUT:
            if (args == null || args.Length == 0)
            {
                var command = Console.ReadLine();
                args = command.Split(" ");
            }


            string port = "500";
            Target = "net6.0-ios";
            Configuration = "Debug";
            CsprojRootFolder = "";
            CsprojFileFullPath = args.FirstOrDefault();
            var shouldShowHelp = false;

            var options = new OptionSet {

                { "p|Port=", "Port (100,200,300...(Default is 500))", x => port = x },
                { "t|target=", "TargetFramework (net,net6,net6.0-ios,net6.0-android,net6.0-maccatalyst,net6.0-windows,xamarin...) ", x => Target = x },
                { "c|configuration=", "Configuration (Debug, Release)", x => Configuration = x },
                { "f|folder=", "Root folder for the solution (Defaults to the CSProj Folder)", x=> CsprojRootFolder = x },
                { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
            };


            List<string> extra;
            try
            {
                // parse the command line
                extra = options.Parse(args);
                // when copy file path at window, will have \' warp path
                if (CsprojFileFullPath.Contains('\"'))
                {
                    CsprojFileFullPath = CsprojFileFullPath.Substring(1, CsprojFileFullPath.Length - 2);
                }
                // if it is Folder path
                if (!File.Exists(CsprojFileFullPath))
                {
                    if (Directory.Exists(CsprojFileFullPath))
                    {
                        foreach (var file in Directory.EnumerateFiles(CsprojFileFullPath))
                        {
                            if (new FileInfo(file).Extension == ".csproj")
                            {
                                CsprojRootFolder = CsprojFileFullPath;
                                CsprojFileFullPath = file;
                                break;
                            }
                        }
                    }
                    else
                    {
                        //input error
                        Console.Write($"Error Path:{CsprojFileFullPath}");
                    }
                }
                else
                    CsprojRootFolder = GetRootDirectory(CsprojFileFullPath);
            }
            catch (OptionException e)
            {
                // output some error message
                AnsiConsole.WriteException(e);
                ShowHelp(options);
                return;
            }

            if (string.IsNullOrWhiteSpace(CsprojFileFullPath) || string.IsNullOrWhiteSpace(CsprojRootFolder))
            {
                shouldShowHelp = true;
            }

            if (shouldShowHelp)
            {
                ShowHelp(options);
                //return;
                args = null;
                goto REINPUT;
            }


            try
            {
                AnalysisCorrectTarget();

                AnsiConsole.MarkupLine($"[green]{Target} - {Configuration}[/]");
                AnsiConsole.MarkupLine($"[green]Activating HotReload[/]");
                AnsiConsole.MarkupLine($"[green]Watching: {CsprojRootFolder}[/]");

                // Create server
                Server = MessageServer.CreatMessageServer(int.Parse(port));
                Console.Title = $"{Server.MyIp}:{Server.Port}";
                FileWatcher = new FileWatcher(CsprojRootFolder);

                AnsiConsole.MarkupLine($"[green]Hot Reload Is Running![/]");

                AnsiConsole.MarkupLine($"[green]Type exit, To Quit[/]");
                await Task.Run(() =>
                {
                    while (true)
                    {
                        var shouldExit = Console.ReadLine() != "exit";
                        if (shouldExit)
                        {
                            //Shutdown and return;
                            FileWatcher?.Dispose();
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
            finally
            {
                FileWatcher?.Dispose();
            }

            Console.ReadLine();
        }

        internal static void BuildAndSendDll()
        {
            AnsiConsole.MarkupLine("[green]Start MSBuild[/]");
            try
            {
                string tempOutputFolder = String.Empty;
                //net6.0-platform
                if (Target.Contains("-windows"))
                {
                    //need output to bin\Configration\windows\
                    //https://github.com/microsoft/microsoft-ui-xaml/issues/6094
                    tempOutputFolder = Path.Combine(CsprojRootFolder, @$"bin\{Configuration}\windows");
                    if (!Directory.Exists(tempOutputFolder))
                        Directory.CreateDirectory(tempOutputFolder);
                    StartMSBuild(CsprojRootFolder, $"msbuild -t:restore /t:Build /v:m /p:Configuration={Configuration} /p:TargetFramework={Target} /p:OutDir={tempOutputFolder} ");
                    //StartMSBuild(CsprojRootFolder, $"dotnet msbuild {CsprojFileFullPath} /t:Build /p:OutpDir={Path.Combine(CsprojRootFolder, @$"bin\{Configuration}\windows")} /p:Configuration={Configuration} /p:TargetFramework={Target}");
                }
                else if (Target.Contains("-android"))
                {
                    StartMSBuild(CsprojRootFolder, $"msbuild {CsprojFileFullPath} /t:Build /p:Platform=\"AnyCPU\" /p:Configuration={Configuration} /p:TargetFramework={Target} /p:AndroidBuildApplicationPackage=false /p:EmbedAssembliesIntoApk=false");
                    tempOutputFolder = Path.Combine(CsprojRootFolder, @$"bin\{Configuration}\{Target}");
                }
                else if (Target.Contains("-ios") || Target.Contains("-maccatalyst"))
                {
                    StartMSBuild(CsprojRootFolder, $"msbuild {CsprojFileFullPath} /t:Build /p:Platform=\"AnyCPU\" /p:Configuration={Configuration} /p:TargetFramework={Target} ");
                    //StartMSBuild(CsprojRootFolder, $"cmd /k \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Preview\\Common7\\Tools\\VsDevCmd.bat\"");
                    tempOutputFolder = Path.Combine(CsprojRootFolder, @$"bin\{Configuration}\{Target}");
                }
                else// net6,netstandard,xamarin,wpf,uwp,all need change output folder,
                {
                    if (Target.Contains("xamarin"))
                    {
                        tempOutputFolder = Path.Combine(CsprojRootFolder, @$"bin\{Configuration}\xamarin");
                        if (!Directory.Exists(tempOutputFolder))
                            Directory.CreateDirectory(tempOutputFolder);
                        StartMSBuild(CsprojRootFolder, $"msbuild {CsprojFileFullPath} /t:Build /p:Platform=\"AnyCPU\" /p:Configuration={Configuration}  /p:OutDir={tempOutputFolder}");
                    }
                    else
                    {
                        tempOutputFolder = Path.Combine(CsprojRootFolder, @$"bin\{Configuration}\net");
                        if (!Directory.Exists(tempOutputFolder))
                            Directory.CreateDirectory(tempOutputFolder);
                        StartMSBuild(CsprojRootFolder, $"msbuild {CsprojFileFullPath} /t:Build /p:Platform=\"AnyCPU\" /p:Configuration={Configuration} /p:TargetFramework={Target} /p:OutDir={tempOutputFolder} ");
                        //StartMSBuild(CsprojRootFolder, $"cmd /k \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Preview\\Common7\\Tools\\VsDevCmd.bat\"");
                    }
                }
                // get assemble file
                AnalysisNewAssembleFilePath(tempOutputFolder);

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Build Fail[/]");
                AnsiConsole.WriteException(ex);
            }
            AnsiConsole.MarkupLine("[green]Finish MSBuilD[/]");
            // try send
            try
            {
                //send dll to app
                if (Server != null)
                {
                    Server.SendFile(NewAssembleFilePath);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Send Fail[/]");
                AnsiConsole.WriteException(ex);
            }
            finally
            {

            }

        }

        static void AnalysisCorrectTarget()
        {
            if (NewAssembleFilePath == string.Empty)
            {
                var projText = File.ReadAllText(CsprojFileFullPath);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(projText);

                List<string> csprojTargets = new List<string>() { };
                //if net6 multiple targets project, get all target
                foreach (XmlNode elem in xmlDoc.GetElementsByTagName("TargetFrameworks"))
                {
                    var targets = elem.InnerText.Replace(" ", "").Split(";");
                    foreach (var targetStr in targets)
                    {
                        if (targetStr.Contains("net"))
                        {
                            csprojTargets.Add(targetStr);
                        }
                    }
                }

                //find correct target
                if (csprojTargets.Count == 1)
                {
                    //single target
                    Target = csprojTargets.FirstOrDefault();
                }
                else
                {
                    foreach (var targetStr in csprojTargets)
                    {
                        if (targetStr.Contains(Target))
                        {
                            Target = targetStr;
                            break;
                        }
                    }
                }

                //single target
                foreach (XmlNode elem in xmlDoc.GetElementsByTagName("TargetFramework"))
                {
                    Target = elem.InnerText.Replace(" ", "");
                    csprojTargets.Add(Target);
                }

                //如果targets非空,那么就是非xamarin应用项目,有单项目和多项目,单项目可以直接确定Target,多项目需要配合输入,如果查找不到输入匹配,那么直接报错
                //如果为空,就是Xamarin项目,都是单target项目,Target直接定为Xamarin
                if (csprojTargets.Count == 0)
                {
                    Target = "xamarin";
                }
                //if finial multiple target not contain,inputed target is false
                if (csprojTargets.Count > 1 && !csprojTargets.Contains(Target))
                {
                    AnsiConsole.MarkupLine($"[red]Inputed target:{Target} Is Not In Them: {csprojTargets.ToString()},Please Exit[/]");
                    throw new NotImplementedException();
                }
            }
        }

        static void AnalysisNewAssembleFilePath(string outputFolder)
        {
            if (NewAssembleFilePath == string.Empty)
            {
                //切割csproj文件路径获取assembly name
                var csprojFileFullName = new FileInfo(CsprojFileFullPath).Name;
                var csprojFileName = csprojFileFullName.Substring(0, csprojFileFullName.Length - 7);
                //查找是否自定义assembly name
                var projText = File.ReadAllText(CsprojFileFullPath);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(projText);
                var assembleFileFullName = csprojFileName + ".dll";
                foreach (XmlNode elem in xmlDoc.GetElementsByTagName("AssemblyName"))
                {
                    assembleFileFullName = elem.InnerText + ".dll";
                }

                NewAssembleFilePath = FindAssamble(outputFolder, assembleFileFullName);
            }
        }

        static string FindAssamble(string folderPath, string assambelName)
        {
            foreach (var folder in Directory.EnumerateDirectories(folderPath))
            {
                var f = FindAssamble(folder, assambelName);
                if (f != string.Empty)
                    return f;
            }
            foreach (var file in Directory.EnumerateFiles(folderPath))
            {
                if (file.Contains(assambelName))
                {
                    return file;
                }
            }
            return string.Empty;
        }

        static string GetRootDirectory(string csProjPath)
        {
            try
            {
                var root = Path.GetDirectoryName(csProjPath);
                if (!string.IsNullOrWhiteSpace(root))
                    return root;

            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
            return Directory.GetCurrentDirectory();
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage:<Project> [OPTIONS] ");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        /// <summary>
        /// 命令行执行
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        private static void StartProcess(string workingDirectory, string command, string arguments)
        {
            try
            {
                command = Environment.ExpandEnvironmentVariables(command ?? string.Empty);
                arguments = Environment.ExpandEnvironmentVariables(arguments ?? string.Empty);

                var start = new ProcessStartInfo(command, arguments)
                {
                    WorkingDirectory = workingDirectory,
                    LoadUserProfile = true,
                    UseShellExecute = false,
                };
                //ModifyPathVariable(start);
                using (System.Diagnostics.Process.Start(start))
                {
                    // Makes sure the process handle is disposed
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }

        /// <summary>
        /// 参考:https://www.zhangshengrong.com/p/P71MYy31dB/
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="cmdArg"></param>
        /// <param name="command"></param>
        private static void StartMSBuild(string csprojFloder, string cmdArg)
        {
            var p = new System.Diagnostics.Process();
            //设定调用的程序名，不是系统目录的需要完整路径
            p.StartInfo.FileName = "cmd";
            //传入执行参数
            p.StartInfo.Arguments = cmdArg;
            p.StartInfo.UseShellExecute = false;
            //是否重定向标准输入
            p.StartInfo.RedirectStandardInput = true;
            //是否重定向标准转出
            p.StartInfo.RedirectStandardOutput = true;
            //是否重定向错误
            p.StartInfo.RedirectStandardError = false;
            //执行时是不是显示窗口
            p.StartInfo.CreateNoWindow = true;//启动
            p.OutputDataReceived += (s, e) =>
            {
                Console.WriteLine(e.Data);
            };
            p.Start();

            StreamWriter cmdWriter = p.StandardInput;
            p.BeginOutputReadLine();

            cmdWriter.WriteLine("cd /d " + csprojFloder);//进入项目路径
            cmdWriter.WriteLine(cmdArg);
            cmdWriter.Close();

            p.WaitForExit();
            p.Close();
        }
    }
}

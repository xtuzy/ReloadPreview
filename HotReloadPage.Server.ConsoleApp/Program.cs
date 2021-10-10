using HotReload.Message;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HotReloadPage.Server.ConsoleApp
{
    internal class Program
    {
        static ServerProgram Server;
        static CompileClassServer compileClassServer = new CompileClassServer();
        static void Main(string[] args)
        {
            var largestWindowX = Console.LargestWindowWidth;
            var largestWindowY = Console.LargestWindowHeight;
            Console.WindowWidth = 50;
            Console.WindowHeight = 10;


            Console.WriteLine("Hello World!");
            Console.WriteLine("开启重载服务?y/n");
            var command = Console.ReadLine();
            if (!command.Contains("y"))
            {
                return;
            }
            Console.WriteLine("指定服务器端口(300-500)");
            command = Console.ReadLine();
            var port = int.Parse(command);
            Server = ServerProgram.CreatServerProgram(port);
            Console.WriteLine("服务器IP:" + Server.MyIp + " Pot:" + Server.Port);

            Console.WriteLine("--------------现在需要一些配置---------------");
            Console.WriteLine("选择使用VS编译好的Dll(y) Or 编译代码(n) ");
            command = Console.ReadLine();
            if (command.Contains("y"))
            {
                Console.WriteLine("输入生成的Dll路径:");
                command = Console.ReadLine();
                string dllPath = string.Empty;
                //如果复制的路径在""中
                if (command[0] == '\"' && command[command.Length - 1] == '\"')
                {
                    dllPath = command.Substring(1, command.Length - 2);
                }
                else
                    dllPath = command;
                Console.WriteLine("自动监控Dll改变?(y/n):");
                command = Console.ReadLine();
                if (command.Contains("y"))
                {
                    FileSystemWatcher watcher = null;
                    while (true)
                    {
                        if (watcher == null)
                        {
                            var dllFileInfo = new FileInfo(dllPath);
                            var dllDirectory = dllFileInfo.DirectoryName;
                            var dllName = dllFileInfo.Name;

                            // Create a new FileSystemWatcher and set its properties.
                            watcher = new FileSystemWatcher();
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
                                    Thread.Sleep(500);
                                    Task.Run(async () =>
                                    {
                                        try
                                        {
                                            //服务端发送assembly
                                            if (Server != null)
                                            {
                                                Server.SendFile(dllPath);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("错误:" + ex.Message);
                                        }
                                        finally
                                        {
                                            await Task.Delay(3000);
                                            isSingleChange = false;
                                        }
                                    });
                                }
                            };
                            // Begin watching.
                            watcher.EnableRaisingEvents = true;
                        }

                        Console.WriteLine("输入e=退出");
                        command = Console.ReadLine();
                        if (command == "e")
                        {
                            return;
                        }
                    }
                }
                else
                {
                    while (true)
                    {
                        Console.WriteLine("输入h=重载,e=退出");
                        command = Console.ReadLine();

                        if (command == "h")
                        {
                            Server.SendFile(dllPath);
                        }
                        if (command == "e")
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("输入编辑的代码文件路径:");
                command = Console.ReadLine();
                string codeFilePath = string.Empty;
                //如果复制的路径在""中
                if (command[0] == '\"' && command[command.Length - 1] == '\"')
                {
                    codeFilePath = command.Substring(1, command.Length - 2);
                }
                else
                    codeFilePath = command;

                Console.WriteLine("输入编辑的代码文件所属项目文件夹路径:");
                command = Console.ReadLine();
                string codeProjectFolderPath = string.Empty;
                //如果复制的路径在""中
                if (command[0] == '\"' && command[command.Length - 1] == '\"')
                {
                    codeProjectFolderPath = command.Substring(1, command.Length - 2);
                }
                else
                    codeProjectFolderPath = command;
                compileClassServer.ClientAppProjectMainPath = codeProjectFolderPath;

                while (true)
                {
                    Console.WriteLine("输入h=重载,e=退出");
                    command = Console.ReadLine();
                    if (command == "h")
                    {
                        var code = File.ReadAllText(codeFilePath);
                        MemoryStream assemblyStream = compileClassServer.DebugRun(code);

                        //Server.SendMessage(bytes);
                        throw new NotImplementedException();
                    }

                    if (command == "e")
                    {
                        return;
                    }
                }
            }
            Console.WriteLine("意外退出?");
        }
    }
}

using ReloadPreview;
using Spectre.Console;
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
        static MessageServer Server;
        static void Main(string[] args)
        {
            var largestWindowX = Console.LargestWindowWidth;
            var largestWindowY = Console.LargestWindowHeight;
            Console.WindowWidth = 50;
            Console.WindowHeight = 10;
            Console.Title = "ReloadPreview Server";
            string command = string.Empty;

            //Get file path
            AnsiConsole.MarkupLine("[green]Input dll/exe path that you want reload to app: [/]");
            command = Console.ReadLine();
            string dllPath = string.Empty;
            //if path in ""
            if (command[0] == '\"' && command[command.Length - 1] == '\"')
            {
                dllPath = command.Substring(1, command.Length - 2);
            }
            else
                dllPath = command;

            //confirm file exist
            if (!File.Exists(dllPath))
            {
                while (true)
                {
                    AnsiConsole.MarkupLine("[red] File not exist, please restart the server app![/]");
                    command = Console.ReadLine();
                }
            }

            //Get port number
            var port = ChoosePort();

            // Create server
            Server = MessageServer.CreatMessageServer(port);

            if (Server == null)
            {
                while (true)
                {
                    AnsiConsole.MarkupLine("[red] Server start fail, please restart the server app![/]");
                    command = Console.ReadLine();
                }
            }
            else
            {
                // If success, print infor
                Console.WriteLine();
                AnsiConsole.MarkupLine("[green]******************************************[/]");
                AnsiConsole.MarkupLine("[green]*** Server IP:" + Server.MyIp + " Port:" + Server.Port + " ***[/]");
                AnsiConsole.MarkupLine("[green]******************************************[/]");
                Console.WriteLine();
                Console.Title = Server.MyIp + ":" + Server.Port;
            }


            if (AnsiConsole.Confirm("[green]Auto monitor dll/exe change?[/]"))
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
                                Thread.Sleep(150);
                                Task.Run(async () =>
                                {
                                    try
                                    {
                                        //send dll to app
                                        if (Server != null)
                                        {
                                            Server.SendFile(dllPath);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        AnsiConsole.WriteException(ex);
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

                    if (!AnsiConsole.Confirm("[green]Force reload? or Exit[/]"))
                    {
                        return;
                    }
                    else
                    {
                        //send dll to app
                        if (Server != null)
                        {
                            Server.SendFile(dllPath);
                        }
                    }
                }
            }
            else
            {
                while (true)
                {
                    if (AnsiConsole.Confirm("[green]Force reload ? or Exit?[/]"))
                    {
                        //send dll to app
                        if (Server != null)
                        {
                            Server.SendFile(dllPath);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }


            AnsiConsole.MarkupLine("[red]Exit ![/]");
        }


        static int ChoosePort()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<int>("[green]Input a port number for you app ?[/]")
                .Validate(age =>
                {
                    return age switch
                    {
                        < 100 => ValidationResult.Error("[red]Too low[/]"),
                        > 1000 => ValidationResult.Error("[red]Too high[/]"),
                        _ => ValidationResult.Success(),
                    };
                }));
        }
    }
}

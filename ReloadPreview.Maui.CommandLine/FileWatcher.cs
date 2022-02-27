using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ReloadPreview.Maui.CommandLine
{
    public class FileWatcher : IDisposable
    {
        FileSystemWatcher fileWatcher;
        private bool disposedValue;
        string filePath;
        public FileWatcher(string filePath)
        {
            this.filePath = filePath;
            fileWatcher = new FileSystemWatcher(filePath)
            {
                Filter = "*.cs",
                IncludeSubdirectories = true,
            };
            fileWatcher.InternalBufferSize = 64 * 1024;
            fileWatcher.Changed += FileWatcher_Changed;
            fileWatcher.Created += FileWatcher_Changed;
            //fileWatcher.Deleted += FileWatcher_Deleted;
            fileWatcher.Renamed += FileWatcher_Changed;
            fileWatcher.Error += FileWatcher_Error;
            fileWatcher.EnableRaisingEvents = true;

        }

        void FileWatcher_Error(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        //void FileWatcher_Renamed(object sender, RenamedEventArgs e) =>RoslynCodeManager.Shared.Rename(e.OldFullPath, e.FullPath);


        //void FileWatcher_Deleted(object sender, FileSystemEventArgs e) => RoslynCodeManager.Shared.Delete(e.FullPath);

        //void FileWatcher_Created(object sender, FileSystemEventArgs e) => RoslynCodeManager.Shared.NewFiles.Add(e.FullPath);

        static string CleanseFilePath(string filePath)
        {
            //On Mac, it may send // for root paths.
            //MSBuild just has /, so we need to cleanse it
            if (!filePath.StartsWith("//"))
                return filePath;
            return filePath[1..];
        }

        bool isChanged = false;
        void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (isChanged) return;
            else
            {
                isChanged = true;
                Task.Run(async () =>
                {
                    if (e.FullPath.Contains("bin") || e.FullPath.Contains("obj"))
                    {
                        await Task.Delay(2000);
                        isChanged = false;
                    }
                    else
                    {
                        var rule = new Rule("[green]File Changed[/]");
                        rule.Alignment = Justify.Left;
                        AnsiConsole.Write(rule);
                        AnsiConsole.WriteLine();
                        Program.BuildAndSendDll();
                        await Task.Delay(800);
                        isChanged = false;
                    }
                });
            }

            return;
        }


        static bool ShouldExcludePath(string path)
        {
            foreach (var dir in excludedDirs)
                if (path.Contains(dir))
                    return true;
            return false;
        }
        static char slash => Path.DirectorySeparatorChar;
        static List<string> excludedDirs = new()
        {
            $"{slash}obj{slash}",
            $"{slash}bin{slash}"
        };

        static void PrintException(Exception ex)
        {
            if (ex != null)
            {
                AnsiConsole.WriteException(ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    fileWatcher?.Dispose();
                }

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

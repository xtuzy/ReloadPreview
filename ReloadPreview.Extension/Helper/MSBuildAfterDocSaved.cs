using EnvDTE;
using MadsKristensen.OpenCommandLine;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReloadPreview.Extension.Helper
{
    /// <summary>
    /// 参考:https://github.com/Elders/VSE-FormatDocumentOnSave/blob/master/src/Elders.VSE-FormatDocumentOnSave/FormatDocumentOnBeforeSave.cs
    /// </summary>
    internal class MSBuildAfterDocSaved : IVsRunningDocTableEvents
    {
        private readonly DTE dte;
        private readonly RunningDocumentTable _runningDocumentTable;

        public MSBuildAfterDocSaved(DTE dte, RunningDocumentTable runningDocumentTable)
        {
            this.dte = dte;
            this._runningDocumentTable = runningDocumentTable;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            //var document = FindDocument(docCookie);

            //参考:https://github.com/madskristensen/OpenCommandLine/blob/master/src/OpenCommandLine/Options.cs
            // 调用Developer Command Prompt
            string installDir = VsHelpers.GetInstallDirectory();
            string devPromptFile = Path.Combine(installDir, @"..\Tools\VsDevCmd.bat");
            string startBatCommand = "/k \"" + devPromptFile + "\"";
            //SetupProcess(dte,exe, para);
            if (ReloadPreviewToolWindowControl.IsAutoBuild)
            {
                var options = new Options() { OpenSlnLevel = false, OpenProjectLevel = true };
                string folder = VsHelpers.GetFolderPath(options, dte);
                Task.Run(() =>
                {
                    try
                    {
                        StartMSBuild(folder, "cmd " + startBatCommand);
                    }catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Cmd execute msbuild error");
                    }
                    
                });
                
            }
            
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }


        #region 开始CMD
        //参考:https://github.com/madskristensen/OpenCommandLine/blob/master/src/OpenCommandLine/Options.cs
        private void SetupProcess(DTE dte,string command, string arguments)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //var options = GetDialogPage(typeof(Options)) as Options;
            var options = new Options() { OpenSlnLevel = false, OpenProjectLevel = true };
            string folder = VsHelpers.GetFolderPath(options, dte);

            StartProcess(folder, command, arguments);
        }

        //参考:https://www.zhangshengrong.com/p/P71MYy31dB/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="startBatCommand"></param>
        /// <param name="command"></param>
        private void StartMSBuild(string folder,string startBatCommand)
        {
            Debug.WriteLine("Start cmd");
            var p = new System.Diagnostics.Process();
            //设定调用的程序名，不是系统目录的需要完整路径
            p.StartInfo.FileName = "cmd";
            //传入执行参数
            p.StartInfo.Arguments = startBatCommand;
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
                Debug.WriteLine(e.Data);
            };
            p.Start();

            StreamWriter cmdWriter = p.StandardInput;
            p.BeginOutputReadLine();

            
            cmdWriter.WriteLine("cd /d "+folder);//进入项目路径
            cmdWriter.WriteLine("msbuild");//执行build
            cmdWriter.Close();
            
            p.WaitForExit();
            p.Close();
        }

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
                

                ModifyPathVariable(start);

                using (System.Diagnostics.Process.Start(start))
                {
                    // Makes sure the process handle is disposed
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        private static void ModifyPathVariable(ProcessStartInfo start)
        {
            string path = ".\\node_modules\\.bin" + ";" + start.EnvironmentVariables["PATH"];

            var process = System.Diagnostics.Process.GetCurrentProcess();
            string ideDir = Path.GetDirectoryName(process.MainModule.FileName);

            if (Directory.Exists(ideDir))
            {
                string parent = Directory.GetParent(ideDir).Parent.FullName;

                string rc2Preview1Path = new DirectoryInfo(Path.Combine(parent, @"Web\External")).FullName;

                if (Directory.Exists(rc2Preview1Path))
                {
                    path += ";" + rc2Preview1Path;
                    path += ";" + rc2Preview1Path + "\\git";
                }
                else
                {
                    path += ";" + Path.Combine(ideDir, @"Extensions\Microsoft\Web Tools\External");
                    path += ";" + Path.Combine(ideDir, @"Extensions\Microsoft\Web Tools\External\git");
                }
            }

            start.EnvironmentVariables["PATH"] = path;
        }

        #endregion


        private Document FindDocument(uint docCookie)
        {
            var documentInfo = _runningDocumentTable.GetDocumentInfo(docCookie);
            var documentPath = documentInfo.Moniker;

            return dte.Documents.Cast<Document>().FirstOrDefault(doc => doc.FullName == documentPath);
        }

    }
}

using MadsKristensen.OpenCommandLine;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ReloadPreview.Extension
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("645d4cc6-fe9b-4c52-875c-4462116897de")]
    public class ReloadPreviewToolWindow : ToolWindowPane
    {
        private EnvDTE.DTE dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReloadPreviewToolWindow"/> class.
        /// </summary>
        public ReloadPreviewToolWindow() : base(null)
        {
            this.Caption = "ReloadPreviewToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new ReloadPreviewToolWindowControl();
        }

    }
}

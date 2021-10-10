using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace OlegShilo.PropMan
{
    public delegate string AnalysisHandler(string fileText);
    /// <summary>
    /// ������ȡ��ǰ�ļ�ȫ���ı�,��Ҫ��һ���ܵĴ�������б�����
    /// </summary>
    public class FileTextAnalysis
    {
        IVsTextManager txtMgr;
        public event AnalysisHandler Analysis;
        public FileTextAnalysis(IVsTextManager txtMgr)
        {
            this.txtMgr = txtMgr;
        }

        public string Execute()
        {
            //ò���ǵõ���ǰ�ı༭��
            IWpfTextView textView = GetTextView();


            ITextSnapshot snapshot = textView.TextSnapshot;

            if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
                return string.Empty;

            if (!textView.Selection.IsEmpty)
                return string.Empty;

            int caretGlobalPos = textView.Caret.Position.BufferPosition.Position;
            int caretLineGlobalStartPos = textView.GetTextViewLineContainingBufferPosition(textView.Caret.Position.BufferPosition).Start.Position;
            int initialCaretXPosition = caretGlobalPos - caretLineGlobalStartPos;

            int startLineNumber = snapshot.GetLineNumberFromPosition(textView.Caret.Position.BufferPosition);

            string lineText = snapshot.GetLineFromLineNumber(startLineNumber).GetText();
            string resultCode = string.Empty;
            
            resultCode = Analysis.Invoke(snapshot.GetText());
             
            if (resultCode == string.Empty)
                return string.Empty;
            return resultCode;
        }

        IWpfTextView GetTextView()
        {
            return GetViewHost().TextView;
        }

        IWpfTextViewHost GetViewHost()
        {
            object holder;
            Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
            GetUserData().GetData(ref guidViewHost, out holder);
            return (IWpfTextViewHost)holder;
        }

        IVsUserData GetUserData()
        {
            int mustHaveFocus = 1;//means true
            IVsTextView currentTextView;
            txtMgr.GetActiveView(mustHaveFocus, null, out currentTextView);

            if (currentTextView is IVsUserData)
                return currentTextView as IVsUserData;
            else
                throw new ApplicationException("No text view is currently open");
        }
    }
}
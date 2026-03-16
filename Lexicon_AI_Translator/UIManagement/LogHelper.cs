using System;
using System.Windows.Controls;

namespace LexTranslator.UIManagement
{
    public class LogHelper
    {
        private static void SetLog(TextBox Handle, string Msg)
        {
            Handle.Dispatcher.BeginInvoke(new Action(() =>
            {
                Handle.Text = Msg;
                Handle.ScrollToEnd();
            }));
        }

        public static void SetInputLog(string Text)
        {
            if (DeFine.WorkingWin != null)
            {
                SetLog(DeFine.WorkingWin.InputLog, Text);
            }
        }

        public static void SetOutputLog(string Text)
        {
            if (DeFine.WorkingWin != null)
            {
                SetLog(DeFine.WorkingWin.OutputLog, Text);
            }
        }

        public static void SetMainLog(string Text)
        {
            if (DeFine.WorkingWin != null)
            {
                SetLog(DeFine.WorkingWin.MainLog, Text);
            }
        }
    }
}

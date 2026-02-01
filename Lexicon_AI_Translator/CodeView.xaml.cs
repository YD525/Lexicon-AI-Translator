using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit;
using System.Collections.Generic;
using System;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media.TextFormatting;
using System.Diagnostics;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for CodeView.xaml
    /// </summary>
    public partial class CodeView : Window
    {
        public CodeView()
        {
            InitializeComponent();
        }

        private static class Win32
        {
            public static readonly IntPtr HWND_TOP = new IntPtr(0);

            public const uint SWP_NOSIZE = 0x0001;
            public const uint SWP_NOMOVE = 0x0002;
            public const uint SWP_NOACTIVATE = 0x0010;
            public const uint SWP_SHOWWINDOW = 0x0040;

            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(
                IntPtr hWnd,
                IntPtr hWndInsertAfter,
                int X, int Y, int cx, int cy,
                uint uFlags);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string GetName = "LexTranslator" + ".IDERule.Lua.xshd";

            System.Reflection.Assembly Assembly = System.Reflection.Assembly.GetExecutingAssembly();

            using (System.IO.Stream Resource = Assembly.GetManifestResourceStream(GetName))
            {
                using (System.Xml.XmlTextReader Reader = new System.Xml.XmlTextReader(Resource))
                {
                    var Xshd = HighlightingLoader.LoadXshd(Reader);

                    TextEditor.SyntaxHighlighting = HighlightingLoader.Load(Xshd, HighlightingManager.Instance);
                }
            }

            DeFine.ActiveIDE = TextEditor;
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public bool IsLeftMouseDown = false;

        private void WinHead_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsLeftMouseDown = true;
            }

            if (IsLeftMouseDown)
            {
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.DragMove();
                    }));

                    IsLeftMouseDown = false;
                }
                catch { }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                SearchText NSearchText = new SearchText();
                NSearchText.Owner = this;
                NSearchText.Show();
            }
        }

        private void Close_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }

        private readonly Stopwatch SyncWatch = new Stopwatch();
        private readonly object SyncLock = new object();

        public void SyncZIndex()
        {
            lock (SyncLock)
            {
                if (SyncWatch.IsRunning && SyncWatch.ElapsedMilliseconds < 100)
                    return;

                SyncWatch.Restart();
            }

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var ChildHwnd = new WindowInteropHelper(this).Handle;

                Win32.SetWindowPos(
                    ChildHwnd,
                    DeFine.WorkingWin.MainHwnd,
                    0, 0, 0, 0,
                    Win32.SWP_NOMOVE |
                    Win32.SWP_NOSIZE |
                    Win32.SWP_NOACTIVATE |
                    Win32.SWP_SHOWWINDOW);
            }));
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            SyncZIndex();
        }
    }

  
}

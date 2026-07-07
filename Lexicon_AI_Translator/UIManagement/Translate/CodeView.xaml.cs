using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit;
using System;
using System.Runtime.InteropServices;
using LexTranslator.UIManagement;
using LexTranslator.SkyrimManagement;
using System.Threading;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for CodeView.xaml
    /// </summary>
    public partial class CodeView : Window
    {
        private LexGui _Owner;
        public ModFile ModRef = null;
        public CodeView(ModFile Mod,LexGui Owner)
        {
            InitializeComponent();

            _Owner = Owner;

            this.Owner = _Owner;
            this.ModRef = Mod;

            _Owner.LocationChanged += OwnerMainWindow_LocationChanged;
            _Owner.SizeChanged += OwnerMainWindow_SizeChanged;
            _Owner.StateChanged += OwnerMainWindow_StateChanged;
            _Owner.Closed += OwnerMainWindow_Closed;
        }

        private void OwnerMainWindow_Closed(object Sender, EventArgs E)
        {
            MultiWindowController.CodeWin= null;
            this.Close();
        }
        private void UpdateFollowPosition()
        {
            double Gap = 8;

            this.Left = _Owner.Left + _Owner.ActualWidth + Gap;
            this.Top = _Owner.Top;
            this.Height = _Owner.ActualHeight;
        }
        private void RecordTracking_Loaded(object Sender, RoutedEventArgs E)
        {
            UpdateFollowPosition();
        }

        private void OwnerMainWindow_LocationChanged(object Sender, EventArgs E)
        {
            UpdateFollowPosition();
        }

        private void OwnerMainWindow_SizeChanged(object Sender, SizeChangedEventArgs E)
        {
            UpdateFollowPosition();
        }

        private void OwnerMainWindow_StateChanged(object Sender, EventArgs E)
        {
            if (_Owner.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            else
            {
                this.Show();
                UpdateFollowPosition();
            }
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

            UpdateFollowPosition();

            SetText(ModRef.PSCCode);
        }

        public void SyncCode(string SearchText = "")
        {
            SetText(ModRef.PSCCode);
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            _Owner.LocationChanged -= OwnerMainWindow_LocationChanged;
            _Owner.SizeChanged -= OwnerMainWindow_SizeChanged;
            _Owner.StateChanged -= OwnerMainWindow_StateChanged;
            _Owner.Closed -= OwnerMainWindow_Closed;
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
                SearchText NSearchText = new SearchText(TextEditor);
                NSearchText.Owner = this;
                NSearchText.Show();
            }
        }


        private void SetText(string Text)
        {
            this.Dispatcher.Invoke(() =>
            {
               TextEditor.WordWrap = false;
               TextEditor.Document = new TextDocument(Text);
            });
        }

        public Thread AutoSelectIDETrd = null;
        public void SelectLineFromIDE(int LineID, string Value)
        {
            if (AutoSelectIDETrd != null)
            {
                try
                {
                    AutoSelectIDETrd.Abort();
                }
                catch { }
                AutoSelectIDETrd = null;
            }

            Value = "\"" + Value + "\"";

            if (LineID == 0)
            {
                LineID = 1;
            }

            AutoSelectIDETrd = new Thread(() =>
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var Editor = TextEditor;
                        var Doc = Editor.Document;

                        int TotalLines = Doc.LineCount;

                        for (int i = LineID; i <= TotalLines; i++)
                        {
                            var Line = Doc.GetLineByNumber(i);
                            string Text = Doc.GetText(Line);

                            int Index = Text.IndexOf(Value, StringComparison.OrdinalIgnoreCase);

                            if (Index >= 0)
                            {
                                int Offset = Line.Offset + Index;

                                Editor.ScrollToLine(i);
                                Editor.Select(Offset, Value.Length);
                                Editor.CaretOffset = Offset + Value.Length;
                                Editor.Focus();

                                break;
                            }
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                }

                AutoSelectIDETrd = null;
            });


            AutoSelectIDETrd.Start();
        }
    }

  
}

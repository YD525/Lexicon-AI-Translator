using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for MessageBoxExtend.xaml
    /// </summary>
    public partial class MessageBoxExtend : Window
    {
        public MessageBoxExtend()
        {
            InitializeComponent();
        }

        private static int GWL_HWNDPARENT = -8;
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        public static void SetOwnerWindowMultithread(IntPtr windowHandleOwned, IntPtr intPtrOwner)
        {
            if (windowHandleOwned != IntPtr.Zero && intPtrOwner != IntPtr.Zero)
            {
                SetWindowLong(windowHandleOwned, GWL_HWNDPARENT, intPtrOwner.ToInt32());
            }
        }

        public static IntPtr GetHandler(Window window)
        {
            IntPtr Handle = IntPtr.Zero;
            window.Dispatcher.Invoke(new Action(() => {
                var interop = new WindowInteropHelper(window);
                Handle = interop.Handle;
            }));
            return Handle;
        }

        public int SelectState = -1;

        public static int Show(Window Parent, string Msg)
        {
            return MessageBoxExtend.Show(Parent, "Prompt", Msg, MsgAction.Yes, MsgType.Info);
        }
        public static int Show(Window Parent, string Tittle, string Msg, MsgAction OneAction, MsgType OneType)
        {
            MessageBoxExtend NMessageBoxExtend = null;
            double GetCurrentTop = 0;
            double GetCurrentLeft = 0;
            double GetFormWidth = 0;
            double GetFormHeight = 0;
            Parent.Dispatcher.Invoke(new Action(() => {
                GetCurrentTop = Parent.Top;
                GetCurrentLeft = Parent.Left;
                GetFormWidth = Parent.ActualWidth;
                GetFormHeight = Parent.ActualHeight;
            }));

            IntPtr GetTargetHandle = GetHandler(Parent);

            Thread NewCreatForm = new Thread(

             new ThreadStart(
             () =>
             {
                 NMessageBoxExtend = new MessageBoxExtend();
                 NMessageBoxExtend.SelectState = -1;
                 NMessageBoxExtend.Left = GetCurrentLeft;
                 NMessageBoxExtend.Top = GetCurrentTop;
                 NMessageBoxExtend.Width = GetFormWidth;
                 NMessageBoxExtend.Height = GetFormHeight;
                 SetOwnerWindowMultithread(GetHandler(NMessageBoxExtend), GetTargetHandle);

                 NMessageBoxExtend.Dispatcher.Invoke(new Action(() => 
                 {
                     if (OneType == MsgType.Waring)
                     {
                         NMessageBoxExtend.MainBackground.Background = new SolidColorBrush(Color.FromRgb(175,41,10));
                         NMessageBoxExtend.ConfirmBtn.Background = new SolidColorBrush(Color.FromRgb(87,5,5));
                         NMessageBoxExtend.CancelBtn.Background = new SolidColorBrush(Color.FromRgb(157,19,19));
                     }

                     NMessageBoxExtend.Title = Tittle;
                     NMessageBoxExtend.Caption.Content = Tittle;
                     NMessageBoxExtend.CurrentMsg.Text = Msg;

                     if (OneAction == MsgAction.Yes || OneAction == MsgAction.Null)
                     {
                         NMessageBoxExtend.ConfirmBtn.Visibility = Visibility.Visible;
                         NMessageBoxExtend.CancelBtn.Visibility = Visibility.Collapsed;
                     }
                     else
                     if (OneAction == MsgAction.YesNo)
                     {
                         NMessageBoxExtend.ConfirmBtn.Visibility = Visibility.Visible;
                         NMessageBoxExtend.CancelBtn.Visibility = Visibility.Visible;
                     }

                     NMessageBoxExtend.Show();
                     NMessageBoxExtend.Activate();

                     if (OneType == MsgType.Error)
                     {
                         NMessageBoxExtend.Dispatcher.Invoke(new Action(() => {
                             NMessageBoxExtend.GroundColor.Background = new SolidColorBrush(Color.FromRgb(109, 26, 18));
                             NMessageBoxExtend.CancelBtn.Background = new SolidColorBrush(Color.FromRgb(58, 0, 0));
                         }));
                     }
                 }));

                 NMessageBoxExtend.Closed += (Sender, E) =>
                 NMessageBoxExtend.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                 Dispatcher.Run();
             }));

            NewCreatForm.SetApartmentState(ApartmentState.STA);
            NewCreatForm.IsBackground = true;
            NewCreatForm.Start();

            while (NMessageBoxExtend == null)
            {
                Thread.Sleep(10);
            }

            while (NMessageBoxExtend.SelectState == -1)
            {
                Thread.Sleep(10);
            }

            return NMessageBoxExtend.SelectState;
        }


        private void Cancel(object sender, MouseButtonEventArgs e)
        {
            SelectState = 0;
            this.Close();
        }

        private void Confirm(object sender, MouseButtonEventArgs e)
        {
            SelectState = 1;
            this.Close();
        }
    }

    public enum MsgAction
    {
        Null = 0, Yes = 1, YesNo = 2
    }

    public enum MsgType
    {
        Null = 0, Info = 1, Waring = 2, Error = 3
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PhoenixEngine.Engine.ADO;
using PhoenixEngine.Language;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DeFine.CloseAny();
        }

        private void Close_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DeFine.CloseAny();
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

        public Thread LoadingTrd = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingTrd = new Thread(() =>
            {
                DeFine.Init(new Action<int>((Step) =>
                {
                    switch (Step)
                    {
                        case 1:
                            { 
                            
                            }
                        break;
                    } 
                }));

            
                var GetEnCompleter = WordAutoComplete.WordCompleters[Languages.English];//Test
            });
            LoadingTrd.Start();
        }

        public void SetLog(string Msg)
        {
            this.Dispatcher.Invoke(new Action(() => { 
                Log.Content = Msg;
            }));
        }
    }
}

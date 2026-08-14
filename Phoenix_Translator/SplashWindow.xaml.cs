using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using PhoenixEngine;

namespace PhoenixTranslator
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

        public bool CanExit = true;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(CanExit)
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
        public static LexGui Main = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Version.Content = DeFine.CurrentVersion;
            string GetSelfPath = DeFine.GetFullPath(@"\");

            LoadingTrd = new Thread(() =>
            {
                SetLog("Initializing...");

                int SleepMs = 0;

                DeFine.PrepareFileDirectory();

                Phoenix.Init(GetSelfPath, new Action<int>((Step) =>
                {
                    switch (Step)
                    {
                        case 1:
                            {
                                SetLog("Loading master database...");
                                if(SleepMs>0)
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 2:
                            {
                                SetLog("Loading advanced dictionary...");
                                if (SleepMs > 0)
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 3:
                            {
                                SetLog("Loading cache system...");
                                if (SleepMs > 0)
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 5:
                            {
                                SetLog("Loading records for Simplified-Traditional conversion...");
                                if (SleepMs > 0)
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 6:
                            {
                                SetLog("Loading file primary key...");
                                if (SleepMs > 0)
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 7:
                            {
                                SetLog("Loading global configuration file...");
                                if (SleepMs > 0)
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 8:
                            {
                                SetLog("Applying proxy settings...");
                                if (SleepMs > 0)
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 9:
                            {
                                SetLog("Loading vocabulary database...");
                                if (SleepMs > 0)
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 10:
                            {
                                SetLog("Loading API Key management...");
                                if (SleepMs > 0)
                                Thread.Sleep(SleepMs);
                            }
                            break;
                    }
                }));

                SetLog("Launching main program...");

                if (SleepMs > 0)
                Thread.Sleep(SleepMs);
               
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    SplashWindow.Main = new LexGui();
                    SplashWindow.Main.Show();
                    CanExit = false;

                    this.Close();
                }));
               
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

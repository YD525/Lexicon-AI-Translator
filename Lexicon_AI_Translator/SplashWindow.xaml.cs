using System;
using System.Security.RightsManagement;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using PhoenixEngine;
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
        public static MainGui Main = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string GetSelfPath = DeFine.GetFullPath(string.Empty);

            LoadingTrd = new Thread(() =>
            {
                SetLog("Initializing...");

                int SleepMs = 50;

                DeFine.PrepareFileDirectory();

                Phoenix.Init(GetSelfPath, new Action<int>((Step) =>
                {
                    switch (Step)
                    {
                        case 1:
                            {
                                SetLog("Creating the master database.");
                                Thread.Sleep(SleepMs);
                            }
                        break;
                        case 2:
                            {
                                SetLog("Initialize advanced dictionary.");
                                Thread.Sleep(SleepMs);
                            }
                         break;
                        case 3:
                            {
                                SetLog("Initialize Cache System.");
                                Thread.Sleep(SleepMs);
                            }
                          break;
                        case 5:
                            {
                                SetLog("Reading records related to Simplified-Traditional conversion.");
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 6:
                            {
                                SetLog("Initialize file primary key.");
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 7:
                            {
                                SetLog("Read the global configuration file.");
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 8:
                            {
                                SetLog("Apply Proxy Settings.");
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 9:
                            {
                                SetLog("Loading vocabulary database...");
                                Thread.Sleep(SleepMs);
                            }
                            break;
                        case 10:
                            {
                                SetLog("Initialize API Key Management...");
                                Thread.Sleep(SleepMs);
                            }
                            break;
                    }
                }));

                SetLog("Launching main program...");
                Thread.Sleep(SleepMs);

                var GetEnCompleter = WordAutoComplete.WordCompleters[Languages.English];//Test

               
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    SplashWindow.Main = new MainGui();
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

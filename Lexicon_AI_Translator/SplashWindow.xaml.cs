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
                Phoenix.Init(GetSelfPath, new Action<int>((Step) =>
                {
                    switch (Step)
                    {
                        case 1:
                            {
                                SetLog("Creating the master database.");
                            }
                        break;
                        case 2:
                            {
                                SetLog("Initialize advanced dictionary.");
                            }
                         break;
                        case 3:
                            {
                                SetLog("Initialize Cache System.");
                            }
                          break;
                        case 5:
                            {
                                SetLog("Reading records related to Simplified-Traditional conversion.");
                            }
                            break;
                        case 6:
                            {
                                SetLog("Initialize file primary key.");
                            }
                            break;
                        case 7:
                            {
                                SetLog("Read the global configuration file.");
                            }
                            break;
                        case 8:
                            {
                                SetLog("Apply Proxy Settings.");
                            }
                            break;
                        case 9:
                            {
                                SetLog("Loading vocabulary database...");
                            }
                            break;
                        case 10:
                            {
                                SetLog("Initialize API Key Management...");
                            }
                            break;
                    }

                    SetLog("Launching main program...");
                }));
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

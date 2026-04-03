using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PhoenixTranslator.ApplicationLayer;
using PhoenixTranslator.UIManagement.Preview;

namespace PhoenixTranslator
{
    /// <summary>
    /// Interaction logic for ChooseLayout.xaml
    /// </summary>
    public partial class ChooseLayout : Window
    {
        public static bool ModernIsReady = false;
        public static bool ClassicIsReady = true;

        private readonly PreviewDiagnosticService _diagnostics;
        internal ChooseLayout(PreviewDiagnosticService diagnostics)
        {
            DeFine.GlobalLocalSetting.ReadConfig();
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));


            if (DeFine.GlobalLocalSetting.Layout == PhoenixLayout.Null)
            {
                InitializeComponent();
            }
            else
            {
                if (DeFine.GlobalLocalSetting.Layout == PhoenixLayout.Modern)
                {
                    RunModern();
                }
                else
                if (DeFine.GlobalLocalSetting.Layout == PhoenixLayout.Classic)
                {
                    RunClassic();
                }
            }
        }

        private void RunModern()
        {
            DeFine.CurrentLayout = new PreviewShellWindow(_diagnostics);
            DeFine.CurrentLayout.Show();

            this.Close();
        }

        private void RunClassic()
        {
            DeFine.WorkWin = new PhoenixGui();
            DeFine.CurrentLayout = DeFine.WorkWin;
            DeFine.WorkWin.Show();

            this.Close();
        }

        private void Modern_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DeFine.GlobalLocalSetting.Layout = PhoenixLayout.Modern;
            RunModern();
        }
        private void Classic_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DeFine.GlobalLocalSetting.Layout = PhoenixLayout.Classic;
            RunClassic();
        }

        private void Close_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DeFine.CloseAny();
        }

        private void Modern_MouseEnter(object sender, MouseEventArgs e)
        {
            Modern.BorderBrush = new SolidColorBrush(Color.FromRgb(250,227,6));
        }

        private void Modern_MouseLeave(object sender, MouseEventArgs e)
        {
            Modern.BorderBrush = new SolidColorBrush(Color.FromRgb(62, 62, 66));
        }

        private void Classic_MouseEnter(object sender, MouseEventArgs e)
        {
            Classic.BorderBrush = new SolidColorBrush(Color.FromRgb(250, 227, 6));
        }

        private void Classic_MouseLeave(object sender, MouseEventArgs e)
        {
            Classic.BorderBrush = new SolidColorBrush(Color.FromRgb(62, 62, 66));
        }
        public void ChangeState(Grid Parent,bool IsReady)
        {
            this.Dispatcher.Invoke(new Action(() => {
                if (Parent.Children.Count == 2)
                {
                    if (IsReady)
                    {
                        (Parent.Children[0] as Border).Visibility = Visibility.Visible;
                        (Parent.Children[1] as Border).Visibility = Visibility.Collapsed;

                    }
                    else
                    {
                        (Parent.Children[0] as Border).Visibility = Visibility.Collapsed;
                        (Parent.Children[1] as Border).Visibility = Visibility.Visible;
                    }
                }
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeState(ModernState, ModernIsReady);
            ChangeState(ClassicState, ClassicIsReady);
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
    }
}

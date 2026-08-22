using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PhoenixTranslator.UIManagement.Main
{
    /// <summary>
    /// Interaction logic for ModFileDialog.xaml
    /// </summary>
    public partial class ModFileDialog : Window
    {
        public ModFileDialog()
        {
            InitializeComponent();
        }

        private void Path_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SearchStr_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Close_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
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

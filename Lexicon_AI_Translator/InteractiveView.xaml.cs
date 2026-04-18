using System.Collections.Generic;
using System.Windows;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for InteractiveView.xaml
    /// </summary>
    public partial class InteractiveView : Window
    {
        public static void CloseAll()
        {
            foreach (var Get in InteractiveView.Views)
            {
                try
                {
                    Get.CanClose = true;
                    Get.Close();
                }
                catch { }
            }

            InteractiveView.Views.Clear();
        }
       
        public static List<InteractiveView> Views = new List<InteractiveView>();
        public InteractiveView()
        {
            InitializeComponent();
        }

        public bool CanExit = false;
        public string Received = "";
        public void SetSend(string Send)
        { 
            this.SendStr.Text = Send;
            Views.Add(this);
            this.Show();
        }
        private void CopySendStr(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.SendStr.Text);
        }
        private void ApplyStr(object sender, RoutedEventArgs e)
        {
            this.Received = this.SendStr.Text;
            this.CanExit = true;
        }

        public bool CanClose = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Let the translation thread close this window.
            CanExit = true;

            if(!CanClose)
            e.Cancel = true;
        }
    }
}

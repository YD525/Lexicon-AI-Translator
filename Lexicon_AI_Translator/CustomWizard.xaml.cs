using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LexTranslator.ConvertManager;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for CustomWizard.xaml
    /// </summary>
    public partial class CustomWizard : Window
    {
        public CustomWizard()
        {
            InitializeComponent();
        }

        public int Step = 1;

        public void SyncUI()
        {
            StepLab.Content = string.Format("{0}/3", Step);

            switch (Step)
            {
                case 1:
                    {
                        Tittle.Content = "Add Platform";
                    }
                break;
                case 2:
                    {
                        Tittle.Content = "Config Request body";
                    }
                break;
                case 3:
                    {
                        Tittle.Content = "Identify the content returned by the request";
                    }
                break;
            }

            if (Step == 3)
            {
                NextBtn.Visibility = Visibility.Collapsed;
                FinishBtn.Visibility = Visibility.Visible;
            }
            else
            {
                NextBtn.Visibility = Visibility.Visible;
                FinishBtn.Visibility = Visibility.Collapsed;
            }

            if (Step == 1)
            {
                BackBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                BackBtn.Visibility = Visibility.Visible;
            }

            foreach (var GetView in Views.Children)
            {
                if (GetView is Grid)
                {
                    Grid ViewHandle = (Grid)GetView;
                    string GetViewName = ConvertHelper.ObjToStr(ViewHandle.Name);
                    if (GetViewName.Equals(string.Format("View{0}", Step)))
                    {
                        ViewHandle.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ViewHandle.Visibility = Visibility.Hidden;
                    }
                }
            }
        }
        private void Next(object sender, MouseButtonEventArgs e)
        {
            if (Step < 3)
            {
                Step++;
                SyncUI();
            }
        }

        private void Back(object sender, MouseButtonEventArgs e)
        {
            if (Step > 1)
            {
                Step--;
                SyncUI();
            }
        }
    }
}

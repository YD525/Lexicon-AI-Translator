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
using PhoenixEngine.EngineManagement;
using PhoenixEngine.PlatformManagement;

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

                        PlatformType.Items.Clear();
                        PlatformType.Items.Add("Local AI");
                        PlatformType.Items.Add("Cloud AI");
                        PlatformType.Items.Add("Traditional");

                        if (CurrentPlatformType.Length > 0)
                        {
                            PlatformType.SelectedValue = CurrentPlatformType;
                        }
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
        public string CurrentPlatformType = "";
        private CustomPlatformInFo CustomPlatform = null;
        private void Next(object sender, MouseButtonEventArgs e)
        {
            if (Step == 1)
            {
                if (CustomPlatform == null)
                {
                    CustomPlatform = new CustomPlatformInFo();
                    CustomPlatform.CustomID = Phoenix.Config.PlatformConfigs.Count + 1;
                }

                CustomPlatform.Name = PlatformName.Text;

                if (CustomPlatform.Name.Length == 0)
                {
                    MessageBoxExtend.Show(this, "Please set the platform name.");
                    return;
                }
                if (CurrentPlatformType.Length == 0)
                {
                    MessageBoxExtend.Show(this, "Please select the platform type.");
                    return;
                }
            }


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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CustomPlatform = null;
            CurrentPlatformType = string.Empty;
            SyncUI();
        }

        private void PlatformType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var GetSelectValue = ConvertHelper.ObjToStr(PlatformType.SelectedValue);
            if (GetSelectValue.Length > 0)
            {
                CurrentPlatformType = GetSelectValue;
            }
        }
    }
}

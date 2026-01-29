using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
using PhoenixEngine.RequestManagement;
using PhoenixEngine.TranslateManage;
using static PhoenixEngine.EngineManagement.DataTransmission;

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
        private CustomReqCore TestCustomCore = null;
        private void Next(object sender, MouseButtonEventArgs e)
        {
            if (Step == 1)
            {
                if (CustomPlatform == null)
                {
                    CustomPlatform = new CustomPlatformInFo();
                    TestCustomCore = new CustomReqCore();
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

                switch (CurrentPlatformType)
                {
                    case "Local AI":
                        {
                            AutomaticFields.Items.Clear();
                            AutomaticFields.Items.Add("{API_KEY}");
                            AutomaticFields.Items.Add("{AI_Prompt}");

                            CustomPlatform.Type = CustomPlatformType.LocalAI;
                        }
                    break;
                    case "Cloud AI":
                        {
                            AutomaticFields.Items.Clear();
                            AutomaticFields.Items.Add("{API_KEY}");
                            AutomaticFields.Items.Add("{AI_Prompt}");

                            CustomPlatform.Type = CustomPlatformType.CloudAI;
                        }
                    break;
                    case "Traditional":
                        {
                            AutomaticFields.Items.Clear();
                            AutomaticFields.Items.Add("{API_KEY}");
                            AutomaticFields.Items.Add("{From}");
                            AutomaticFields.Items.Add("{To}");

                            CustomPlatform.Type = CustomPlatformType.Traditional;
                        }
                    break;
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

        private void Url_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TestCustomCore != null)
            {
                CustomPlatform.Url = HttpUtility.UrlDecode(Url.Text);
                TestCustomCore.SetUrl(CustomPlatform.Url);
                UrlTags.Items.Clear();

                foreach (var GetTag in TestCustomCore.GetUrlKeyValues())
                {
                    UrlTags.Items.Add(GetTag.Key + "->" + GetTag.Value);
                }
            }
        }

        private void Header_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TestCustomCore != null)
            {
                CustomPlatform.Header = Header.Text;
                TestCustomCore.SetHeader(CustomPlatform.Header);
                HeaderTags.Items.Clear();

                foreach (var GetTag in TestCustomCore.GetHeaderKeyValues())
                {
                    HeaderTags.Items.Add(GetTag.Key + "->" + GetTag.Value);
                }
            }
        }

        private void Payload_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TestCustomCore != null)
            {
                CustomPlatform.PayLoad = Payload.Text;
                TestCustomCore.SetPayLoad(CustomPlatform.PayLoad);
                PayloadTags.Items.Clear();

                foreach (var GetTag in TestCustomCore.GetPayLoadKeyValues())
                {
                    PayloadTags.Items.Add(GetTag.Key + "->" + GetTag.Value);
                }
            }
        }

        private void IsPost_Click(object sender, RoutedEventArgs e)
        {
            if (IsPost.IsChecked == true)
            {
                CustomPlatform.IsPost = true;
            }
            else
            {
                CustomPlatform.IsPost = false;
            }
        }

        public string ApiKey = "";
        private void TestApiKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApiKey = TestApiKey.Text;
        }
        private void TestCall(object sender, MouseButtonEventArgs e)
        {
            PlatformConfig NPlatformConfig = new PlatformConfig();
            NPlatformConfig.Platform = PhoenixEngine.TranslateManage.PlatformType.CustomPlatform;
            NPlatformConfig.Enable = true;

            int TestID = 525;

            switch (CurrentPlatformType)
            {
                case "Local AI":
                    {
                        CustomLocalAIApi NCustomLocalAIApi = new CustomLocalAIApi();
                    }
                    break;
                case "Cloud AI":
                    {
                        NPlatformConfig.CustomInFo = CustomPlatform;

                        if (!Phoenix.Config.PlatformConfigs.ContainsKey(TestID))
                        {
                            Phoenix.Config.PlatformConfigs.Add(TestID, NPlatformConfig);
                        }
                        else
                        {
                            throw (new Exception("Adding more than 500 platforms is not supported."));
                        }
                       

                        AICall GenAICall = new AICall();
                        CustomAIApi NCustomAIApi = new CustomAIApi();
                        NCustomAIApi.Init(TestID, new AITranslationMemory(),Phoenix.Config,ProxyCenter.CurrentProxy);

                        NCustomAIApi.QuickTrans(
                            ApiKey,
                            new List<ReplaceTag>(),
                            "Test Str",
                            Phoenix.From,
                            Phoenix.To,
                            false,
                            0,
                            string.Empty,
                            ref GenAICall,
                            ""
                            );
                    }
                    break;
                case "Traditional":
                    {
                        CustomApi NCustomApi = new CustomApi();
                    }
                    break;
            }

            if (Phoenix.Config.PlatformConfigs.ContainsKey(TestID))
            {
                Phoenix.Config.PlatformConfigs.Remove(TestID);
            }
        }

      
    }
}

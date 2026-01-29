using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        private ReqQueryRuleItem QueryRule = null;

        public string TagType = "";
        public string TagKey = "";
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
                            AutomaticFields.Items.Add("{AI_Model}");

                            CustomPlatform.Type = CustomPlatformType.CloudAI;
                        }
                    break;
                    case "Traditional":
                        {
                            AutomaticFields.Items.Clear();
                            AutomaticFields.Items.Add("{API_KEY}");
                            AutomaticFields.Items.Add("{P_From}");
                            AutomaticFields.Items.Add("{P_To}");

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

            if (Step == 3)
            {
                if (CurrentResponse.Length == 0)
                {
                    MessageBoxExtend.Show(this, "Please click TestCall first to ensure the API returns a normal response.");
                    return;
                }

                P_Response.Text = CurrentResponse;
                var GetKeyValues = CustomPlatformHelper.GetJsonValues(CurrentResponse);

                QueryRule = new ReqQueryRuleItem();

                if (GetKeyValues.Count == 0)
                {
                    QueryRule.ByJson = false;
                    IsJson.IsChecked = false;
                }
                else
                {
                    QueryRule.ByJson = true;
                    IsJson.IsChecked = true;

                    P_ResponseTags.Items.Clear();

                    foreach (var GetItem in GetKeyValues)
                    {
                        if (CustomPlatform.Type == CustomPlatformType.LocalAI || CustomPlatform.Type == CustomPlatformType.CloudAI)
                        {
                            if (MatchTranslationJson(GetItem.Value))
                            {
                                QueryRule.FieldName = GetItem.Key;
                                FieldName.Content = string.Format("FieldName:{0}", GetItem.Key);
                                MessageBoxExtend.Show(this, "The fields have been automatically retrieved; please click Finish to end this wizard.");
                            }
                        }

                        P_ResponseTags.Items.Add(string.Format("{0}->{1}", GetItem.Key, GetItem.Value));
                    }
                }
            }
        }

        private void Back(object sender, MouseButtonEventArgs e)
        {
            if (Step > 1)
            {
                Step--;
                SyncUI();
            }

            if (Step == 2)
            {
                CurrentResponse = string.Empty;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CustomPlatform = null;
            CurrentPlatformType = string.Empty;

            TagType = string.Empty;
            TagKey = string.Empty;

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

                var TagData = TestCustomCore.GetUrlKeyValues();

                foreach (var GetTag in TagData)
                {
                    UrlTags.Items.Add(GetTag.Key + "->" + GetTag.Value);
                }

                 CustomPlatform.Url_Tags = CustomKeyValueToTags(TagData);
            }
        }
        public List<ReqReplaceTag> CustomKeyValueToTags(List<ReqCustomKeyValue>Array)
        {
            List<ReqReplaceTag> ReqTags = new List<ReqReplaceTag>();
            foreach (var Get in Array)
            {
                ReqTags.Add(new ReqReplaceTag(Get.Key,Get.Value));
            }
            return ReqTags;
        }

        private void Header_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TestCustomCore != null)
            {
                CustomPlatform.Header = Header.Text;
                TestCustomCore.SetHeader(CustomPlatform.Header);
                HeaderTags.Items.Clear();

                var TagData = TestCustomCore.GetHeaderKeyValues();

                foreach (var GetTag in TagData)
                {
                    HeaderTags.Items.Add(GetTag.Key + "->" + GetTag.Value);
                }

                CustomPlatform.Header_Tags = CustomKeyValueToTags(TagData);
            }
        }

        private void Payload_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TestCustomCore != null)
            {
                CustomPlatform.PayLoad = Payload.Text;
                TestCustomCore.SetPayLoad(CustomPlatform.PayLoad);
                PayloadTags.Items.Clear();

                var TagData = TestCustomCore.GetPayLoadKeyValues();

                foreach (var GetTag in TagData)
                {
                    PayloadTags.Items.Add(GetTag.Key + "->" + GetTag.Value);
                }

                CustomPlatform.PayLoad_Tags = CustomKeyValueToTags(TagData);
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

        public string CurrentResponse = "";
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

                        Response.Text = GenAICall.ReceiveString;
                        CurrentResponse = GenAICall.ReceiveString;
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

        private void UrlTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetSelectValue = ConvertHelper.ObjToStr(UrlTags.SelectedValue);
            if (GetSelectValue.Trim().Length > 0)
            {
                TagType = "Url";
                TagKey = GetSelectValue.Substring(0,GetSelectValue.IndexOf("->"));

                BindingInFo.Content = string.Format("Select {0},{1}", TagType, TagKey);
            }
        }

        private void HeaderTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetSelectValue = ConvertHelper.ObjToStr(HeaderTags.SelectedValue);
            if (GetSelectValue.Trim().Length > 0)
            {
                TagType = "Header";
                TagKey = GetSelectValue.Substring(0,GetSelectValue.IndexOf("->"));

                BindingInFo.Content = string.Format("Select {0},{1}", TagType, TagKey);
            }
        }

        private void PayloadTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetSelectValue = ConvertHelper.ObjToStr(PayloadTags.SelectedValue);
            if (GetSelectValue.Trim().Length > 0)
            {
                TagType = "Payload";
                TagKey = GetSelectValue.Substring(0,GetSelectValue.IndexOf("->"));

                BindingInFo.Content = string.Format("Select {0},{1}", TagType, TagKey);
            }
        }

        public void ChangeBindingState(string NewValue)
        {
            switch (TagType)
            {
                case "Url":
                    {
                        for (int i = 0; i < UrlTags.Items.Count; i++)
                        {
                            string GetValue = UrlTags.Items[i].ToString();
                            string GetKey = GetValue.Substring(0, GetValue.IndexOf("->"));

                            if (GetKey.Equals(TagKey))
                            {
                                UrlTags.Items[i] = GetKey + "->" + NewValue;
                                break;
                            }
                        }
                    }
                    break;
                case "Header":
                    {
                        for (int i = 0; i < HeaderTags.Items.Count; i++)
                        {
                            string GetValue = HeaderTags.Items[i].ToString();
                            string GetKey = GetValue.Substring(0, GetValue.IndexOf("->"));

                            if (GetKey.Equals(TagKey))
                            {
                                HeaderTags.Items[i] = GetKey + "->" + NewValue;
                                break;
                            }
                        }
                    }
                    break;
                case "Payload":
                    {
                        for (int i = 0; i < PayloadTags.Items.Count; i++)
                        {
                            string GetValue = PayloadTags.Items[i].ToString();
                            string GetKey = GetValue.Substring(0, GetValue.IndexOf("->"));

                            if (GetKey.Equals(TagKey))
                            {
                                PayloadTags.Items[i] = GetKey + "->" + NewValue;
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        private void BindingTag(object sender, SelectionChangedEventArgs e)
        {
            string GetAutomaticField = ConvertHelper.ObjToStr(AutomaticFields.SelectedValue);

            if (GetAutomaticField.Length > 0)
            {
                switch (TagType)
                {
                    case "Url":
                        {
                            for (int i = 0; i < CustomPlatform.Url_Tags.Count; i++)
                            {
                                if (CustomPlatform.Url_Tags[i].Key.Equals(TagKey))
                                {
                                    CustomPlatform.Url_Tags[i].SetValue(GetAutomaticField,ReqEncodeType.Null);
                                    ChangeBindingState(GetAutomaticField);
                                    break;
                                }
                            }
                        }
                        break;
                    case "Header":
                        {
                            for (int i = 0; i < CustomPlatform.Header_Tags.Count; i++)
                            {
                                if (CustomPlatform.Header_Tags[i].Key.Equals(TagKey))
                                {
                                    CustomPlatform.Header_Tags[i].SetValue(GetAutomaticField, ReqEncodeType.Null);
                                    ChangeBindingState(GetAutomaticField);
                                    break;
                                }
                            }
                        }
                        break;
                    case "Payload":
                        {
                            for (int i = 0; i < CustomPlatform.PayLoad_Tags.Count; i++)
                            {
                                if (CustomPlatform.PayLoad_Tags[i].Key.Equals(TagKey))
                                {
                                    CustomPlatform.PayLoad_Tags[i].SetValue(GetAutomaticField, ReqEncodeType.Null);
                                    ChangeBindingState(GetAutomaticField);
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
            
        }

        public bool MatchTranslationJson(string Input)
        {
            return Regex.IsMatch(Input,@"^\s*\{\s*""translation""\s*:\s*""(?:\\.|[^""\\])*""\s*\}\s*$");
        }

        private void P_ResponseTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetSelectValue = ConvertHelper.ObjToStr(P_ResponseTags.SelectedValue);
            if (GetSelectValue.Trim().Length > 0)
            {
                string GetKey = GetSelectValue.Substring(0, GetSelectValue.IndexOf("->"));
                FieldName.Content = string.Format("FieldName:{0}", GetKey);
                QueryRule.FieldName = GetKey;
            }
        }

        private void LeftStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            QueryRule.LeftStr = LeftStr.Text.Trim();
        }

        private void RightStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            QueryRule.RightStr = RightStr.Text.Trim();
        }

        private void SplitStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            QueryRule.SplitStr = SplitStr.Text.Trim();
        }

        private void TestGetResponseBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string TransStr = "";
            if (QueryRule.ByJson)
            {
                var GetTags = CustomPlatformHelper.GetJsonValues(CurrentResponse);

                for (int i = 0; i < GetTags.Count; i++)
                {
                    if (GetTags[i].Key.Equals(QueryRule.FieldName))
                    {
                        TransStr = GetTags[i].Value;
                        break;
                    }
                }
            }
            else
            if (QueryRule.SplitStr.Trim().Length > 0)
            {
                TransStr = CurrentResponse.Substring(CurrentResponse.LastIndexOf(QueryRule.SplitStr) + QueryRule.SplitStr.Length);
            }
            else
            if (QueryRule.LeftStr.Trim().Length > 0)
            {
                TransStr = ConvertHelper.StringDivision(CurrentResponse, QueryRule.LeftStr, QueryRule.RightStr);
            }
            
            MessageBoxExtend.Show(this, TransStr);
        }

        private void FinishBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PlatformConfig NPlatformConfig = new PlatformConfig();
            NPlatformConfig.Platform = PhoenixEngine.TranslateManage.PlatformType.CustomPlatform;
            NPlatformConfig.Enable = false;

            CustomPlatform.QueryRule = QueryRule;
            
            NPlatformConfig.ApiKeys.Add(ApiKey);

            while(Phoenix.Config.PlatformConfigs.ContainsKey(CustomPlatform.CustomID))
            {
                CustomPlatform.CustomID = Phoenix.Config.PlatformConfigs.Count + 1;
            }

            NPlatformConfig.CustomInFo = CustomPlatform;

            Phoenix.Config.PlatformConfigs.Add(CustomPlatform.CustomID, NPlatformConfig);
            Phoenix.SaveConfig();

            CustomPlatform = null;
            CurrentPlatformType = string.Empty;
            QueryRule = null;
            TagType = string.Empty;
            TagKey = string.Empty;

            this.Close();
        }
    }
}

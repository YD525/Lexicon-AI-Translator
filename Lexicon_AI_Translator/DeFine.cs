using ICSharpCode.AvalonEdit;
using System.IO;
using System.Text;
using System.Windows.Media;
using LexTranslator.SkyrimModManager;
using System.Windows.Threading;
using System.Windows;
using LexTranslator.UIManagement;
using System;
using Newtonsoft.Json;
using System.Threading;
using LexTranslator.UIManage;
using PhoenixEngine;
using PhoenixEngine.Language;
using PhoenixEngine.ADO;
using PhoenixEngine.Engine.ADO;
using System.Runtime.CompilerServices;

namespace LexTranslator
{
    public enum GameNames
    {
        Skyrim = 0
    }
    public class DeFine
    {
        public static bool CanUpdateChart = false;
        public static int GlobalRequestTimeOut = 5000;
        public static int ViewMode = 0;

        public static SolidColorBrush DefBackGround = new SolidColorBrush(Color.FromRgb(11, 116, 209));
        public static SolidColorBrush SelectBackGround = new SolidColorBrush(Color.FromRgb(7, 82, 149));

        public static string PapyrusCompilerPath = "";

        public static int DefPageSize = 100;

        public static bool AutoTranslate = true;

        public static string BackupPath = @"\BackUpData\";

        public static string CurrentVersion = "3.8.2.8";
        public static LocalSetting GlobalLocalSetting = new LocalSetting();

        public static TextEditor ActiveIDE = null;

        public static RowStyleWin RowStyleWin = new RowStyleWin();
        public static NodeStyleWin NodeStyleWin = new NodeStyleWin();
        public static PlatformConfigStyleWin PlatformConfigStyleWin = new PlatformConfigStyleWin(null);

        public static DataBaseView DataBaseView = null;

        public static LexGui WorkWin = null;

        public static ExtendWin ExtendWin = null;
        public static CGView CG = null;

        public static ChartData ChartDataRef = null;

        public static WordAutoComplete WordCompleter = null;

        public static void OpenDataBaseView(Window Parent,string SqlOrder = "")
        {
            if (DataBaseView == null)
            {
                DataBaseView = new DataBaseView();
                DataBaseView.Owner = Parent;
                DataBaseView.Show();
                if (SqlOrder.Length > 0)
                {
                    DataBaseView.QueryFirst(SqlOrder);
                }
            }
        }
        public static void CloseDataBaseView()
        {
            if (DataBaseView != null)
            {
                DataBaseView.Close();
                DataBaseView = null;
            }
        }

        public static void CloseAny()
        {
            Phoenix.SaveConfig();
            DeFine.GlobalLocalSetting.SaveConfig();
            Environment.Exit(0);
        }

        public static string GetFullPath(string Path)
        {
            if (Path.Length > 0)
            {
                if (!Path.Trim().StartsWith(@"\"))
                {
                    Path = @"\" + Path;
                }
            }
            string GetShellPath = System.Windows.Forms.Application.StartupPath;
            if (GetShellPath.EndsWith(@"\"))
            {
                if (Path.StartsWith(@"\"))
                {
                    Path = Path.Substring(1);
                }
            }
            return GetShellPath + Path;
        }

        public static void PrepareFileDirectory()
        {
            var HasWriteAccess = new DirectoryInfo(DeFine.GetFullPath(@"\")).GetAccessControl().AreAccessRulesProtected == false;
            if (!HasWriteAccess)
            {
                MessageBox.Show("The current path does not have write permission. Please move to another path Or right-click to run as administrator.");
                DeFine.CloseAny();
            }

            if (!Directory.Exists(DeFine.GetFullPath(@"\Librarys")))
            {
                Directory.CreateDirectory(DeFine.GetFullPath(@"\Librarys"));
            }
            if (!Directory.Exists(DeFine.GetFullPath(@"\Cache")))
            {
                Directory.CreateDirectory(DeFine.GetFullPath(@"\Cache"));
            }
            if (!File.Exists(DeFine.GetFullPath(@"\setting.config")))
            {
                var CreatNewLocalSetting = new LocalSetting();
                CreatNewLocalSetting.SaveConfig();
            }
            if (!Directory.Exists(DeFine.GetFullPath(@"\CorePlugins")))
            {
                Directory.CreateDirectory(DeFine.GetFullPath(@"\CorePlugins"));
            }
        }

        private static object ErrorReportLocker = new object();
        public static void SetSQLErrorReport()
        {
            P_SQLite.OnError += new Action<string>((ErrorMsg) =>
            {
                lock (ErrorReportLocker)
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (DeFine.DataBaseView != null)
                        {
                            MessageBoxExtend.Show(DeFine.DataBaseView, "SQL", ErrorMsg, MsgAction.Null, MsgType.Waring);
                        }
                        else
                        if (DeFine.WorkWin != null)
                        {
                            MessageBoxExtend.Show(DeFine.WorkWin, "SQL", ErrorMsg, MsgAction.Null, MsgType.Waring);
                        }
                    }));
            });
        }
        public static void Init(LexGui Win)
        {
            if (Win != null)
            {
                DeFine.WorkWin = Win;
                ChartDataRef = new ChartData();

                RowStyleWin.Hide();

                //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                ExtendWin = new ExtendWin();

                SetSQLErrorReport();
            }
        }
    }

 
    public class LocalSetting
    {
        public int Style { get; set; } = 1;
        public double FormHeight { get; set; } = 850;
        public double FormWidth { get; set; } = 1200;
        public Languages CurrentUILanguage { get; set; } = Languages.English;
        public string SkyrimPath { get; set; } = "";

        public bool ShowCode { get; set; } = true;
        public GameNames GameType { get; set; } = GameNames.Skyrim;
        public double WritingAreaHeight { get; set; } = 0;
        public string ViewMode { get; set; } = "Normal";

        public Languages SourceLanguage { get; set; } = Languages.English;
        public Languages TargetLanguage { get; set; } = Languages.English;

        public bool CanClearCloudTranslationCache { get; set; } = false;
        public bool CanClearUserInputTranslationCache { get; set; } = false;

        public bool AutoSpeak { get; set; } = false;

        public int ChatGPTTokenUsage { get; set; } = 0;
        public int GeminiTokenUsage { get; set; } = 0;
        public int CohereTokenUsage { get; set; } = 0;
        public int DeepSeekTokenUsage { get; set; } = 0;
        public int BaichuanTokenUsage { get; set; } = 0;
        public int LocalAITokenUsage { get; set; } = 0;

        public bool EnableAnalyzingWords { get; set; } = true;
        public bool AutoUpdateStringsFileToDatabase { get; set; } = false;

        public bool EnableLanguageDetect { get; set; } = true;
        public string P_Placeholders { get; set; } = "<(.*?)>,";

        public bool CanTranslateBook { get; set; } = true;
        public TextLayout TextDisplay { get; set; } = TextLayout.LTR;

        public bool ShowAssembly { get; set; } = false;
        public bool GenCSharp { get; set; } = true;
        public bool UseFullPunctuation { get; set; } = false;


        public bool TableAuto { get; set; } = false;

        public bool WordCompletion { get; set; } = true;

        public string CustomFilterStr { get; set; } = "";

        public void ReadConfig()
        {
            try
            {
                if (File.Exists(DeFine.GetFullPath(@"\setting.config")))
                {
                    var GetStr = Encoding.UTF8.GetString(DataHelper.ReadFile(DeFine.GetFullPath(@"\setting.config")));
                    if (GetStr.Trim().Length > 0)
                    {
                        var GetSetting = JsonConvert.DeserializeObject<LocalSetting>(GetStr);
                        if (GetSetting != null)
                        {
                            this.Style = GetSetting.Style;
                            this.FormHeight = GetSetting.FormHeight;
                            this.FormWidth = GetSetting.FormWidth;
                            this.CurrentUILanguage = GetSetting.CurrentUILanguage;
                            this.SkyrimPath = GetSetting.SkyrimPath;
                            this.ShowCode = GetSetting.ShowCode;
                            this.GameType = GetSetting.GameType;
                            this.WritingAreaHeight = GetSetting.WritingAreaHeight;
                            this.ViewMode = GetSetting.ViewMode;
                            this.SourceLanguage = GetSetting.SourceLanguage;
                            this.TargetLanguage = GetSetting.TargetLanguage;
                            this.CanClearCloudTranslationCache = GetSetting.CanClearCloudTranslationCache;
                            this.CanClearUserInputTranslationCache = GetSetting.CanClearUserInputTranslationCache;
                            this.AutoSpeak = GetSetting.AutoSpeak;

                            this.ChatGPTTokenUsage = GetSetting.ChatGPTTokenUsage;
                            this.GeminiTokenUsage = GetSetting.GeminiTokenUsage;
                            this.CohereTokenUsage = GetSetting.CohereTokenUsage;
                            this.DeepSeekTokenUsage = GetSetting.DeepSeekTokenUsage;
                            this.BaichuanTokenUsage = GetSetting.BaichuanTokenUsage;
                            this.LocalAITokenUsage = GetSetting.LocalAITokenUsage;

                            this.EnableAnalyzingWords = GetSetting.EnableAnalyzingWords;
                            this.AutoUpdateStringsFileToDatabase = GetSetting.AutoUpdateStringsFileToDatabase;

                            this.EnableLanguageDetect = GetSetting.EnableLanguageDetect;
                            this.P_Placeholders = GetSetting.P_Placeholders;
                            this.CanTranslateBook = GetSetting.CanTranslateBook;

                            this.TextDisplay = GetSetting.TextDisplay;

                            this.ShowAssembly = GetSetting.ShowAssembly;
                            this.GenCSharp = GetSetting.GenCSharp;

                            this.UseFullPunctuation = GetSetting.UseFullPunctuation;

                            this.TableAuto = GetSetting.TableAuto;
                            this.WordCompletion = GetSetting.WordCompletion;

                            this.CustomFilterStr = GetSetting.CustomFilterStr;
                        }
                    }
                    else
                    {
                        LocalSetting CopySetting = this;
                        var GetSettingContent = JsonConvert.SerializeObject(CopySetting);
                        DataHelper.WriteFile(DeFine.GetFullPath(@"\setting.config"), Encoding.UTF8.GetBytes(GetSettingContent));
                    }
                }
            }
            catch { }
        }

        public void SaveConfig()
        {
            LocalSetting CopySetting = this;
            var GetSettingContent = JsonConvert.SerializeObject(CopySetting, Formatting.Indented);

            DataHelper.WriteFile(DeFine.GetFullPath(@"\setting.config"), Encoding.UTF8.GetBytes(GetSettingContent));

            Phoenix.SaveConfig();
        }
    }
}

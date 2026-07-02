using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using LexTranslator.SkyrimManagement;
using LexTranslator.TranslateManage;
using LexTranslator.UIManage;
using PhoenixEngine.Common;
using static PexInterface.PexHeuristicAnalysis;
using System.Windows.Media;
using PhoenixEngine.Engine;
using PhoenixEngine.Language;
using LexTranslator.IDEManagement;
using PhoenixEngine.ADO;
using PhoenixEngine.Unit;
using PhoenixEngine.Translate;
using PhoenixEngine.Additional;

namespace LexTranslator.UIManagement
{
    public class SearchData
    {
        public string FristChar = "";
        public Dictionary<string, int> KeyWords = new Dictionary<string, int>();
    }
    /// <summary>
    /// Interaction logic for TranslateView.xaml
    /// </summary>
    public partial class TranslateView : UserControl
    {
        public YDListView TransListView = null;
        public string Path = "";
        public ModFile Mod = null;

        public TranslateView()
        {
            InitializeComponent();
        }

        public void SetFile(string Path)
        {
            if (Mod == null)
            {
                this.Path = Path;

                Mod = new ModFile(Path);
                TransListView = new YDListView(Mod, TransView);
                TransListView.Clear();

                TransListView.LineSelectedEvent += new YDListView.LineSelected((Key) => {
                    this.Dispatcher.Invoke(new Action(() => {
                        SetSelectFromAndToText(Key);
                    }));
                });

                Mod.SetTranslateView(TransListView);
                Mod.Load();

                SyncConfig();

                if (Mod.Type != GameFileType.ESP)
                {
                    ReloadData();
                }
                else
                {
                    ReSetEspTypes(Mod.EspReader.Types);
                }
               
            }
        }

        public void ReSetEspTypes(List<string> Types)
        {
            TypeSelector.Items.Clear();
            if (Types != null)
                if (Types.Count > 0)
                {
                    TypeSelector.Items.Add("ALL");
                    foreach (var Type in Types)
                    {
                        TypeSelector.Items.Add(Type);
                    }
                    TypeSelector.SelectedValue = TypeSelector.Items[0];
                }
        }

        public void Close()
        {
            Mod?.Close();
        }

        public void InitIDE()
        {
            string GetName = "LexTranslator" + ".IDERule.TextStyle.xshd";

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            using (System.IO.Stream s = assembly.GetManifestResourceStream(GetName))
            {
                using (System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(s))
                {
                    var xshd = HighlightingLoader.LoadXshd(reader);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        FromStr.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
                        ToStr.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
                    }));
                }
            }
        }

        public void SyncConfig()
        {
            InitIDE();

            if (DeFine.GlobalLocalSetting.ViewMode == "Normal")
            {
                EnableNormalModel();

            }
            else
            {
                EnableQuickModel();
                EmptyFromAndToText();
            }

            if (DeFine.GlobalLocalSetting.CanClearCloudTranslationCache)
            {
                CloudTranslationCache.IsChecked = true;
            }
            else
            {
                CloudTranslationCache.IsChecked = false;
            }

            if (DeFine.GlobalLocalSetting.CanClearUserInputTranslationCache)
            {
                UserTranslationCache.IsChecked = true;
            }
            else
            {
                UserTranslationCache.IsChecked = false;
            }

            if (DeFine.GlobalLocalSetting.AutoSpeak)
            {
                AutoSpeak.IsChecked = true;
            }
            else
            {
                AutoSpeak.IsChecked = false;
            }

            if (DeFine.GlobalLocalSetting.TableAuto)
            {
                SetHotKeyDot(true);
                NextAutoEnable = 1;
                _AutoLoop = true;
            }

            if (DeFine.GlobalLocalSetting.WordCompletion)
            {
                AutoWordCompletion.IsChecked = true;
            }
            else
            {
                AutoWordCompletion.IsChecked = false;
            }

            if (DeFine.WordCompleter == null)
            {
                HideWordCompletion();
            }
            else
            {
                ShowWordCompletion();
            }
        }

        public void HideWordCompletion()
        {
            AutoWordCompletion.Visibility = Visibility.Collapsed;
            UIAutoWordCompletion.Visibility = Visibility.Collapsed;
        }

        public void ShowWordCompletion()
        {
            AutoWordCompletion.Visibility = Visibility.Visible;
            UIAutoWordCompletion.Visibility = Visibility.Visible;
        }


        public int NextAutoEnable = 0;
        private const string ToolTipNextAutoOff = "Auto Next: OFF — Click to enable. When enabled, pressing Tab will automatically jump to the next untranslated entry.";
        private const string ToolTipNextAutoOn = "Auto Next: ON — Tab key is now redirected to jump to the next untranslated entry. Click to disable.";
        private bool _AutoLoop = false;

        public void SetHotKeyDot(bool Enable)
        {
            if (Enable)
            {
                HotKeyDot.Fill = new SolidColorBrush(Color.FromRgb(11, 116, 209));
                HotKeyArea.ToolTip = ToolTipNextAutoOn;
            }
            else
            {
                HotKeyDot.Fill = new SolidColorBrush(Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF));
                HotKeyArea.ToolTip = ToolTipNextAutoOff;
            }
        }

        public bool CheckDictionary()
        {
            return Mod.Lex_Dictionary.CheckDictionary();
        }

        public void SetLog(string Str)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                CurrentLog.Text = "Log: " + Str;
            }));
        }

        public string LastSetKey = "";

        public void EmptyFromAndToText()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                CurrentKeyBox.Visibility = Visibility.Collapsed;
                FromStr.Text = string.Empty;
                ToStr.Text = string.Empty;
            }));
            LastSetKey = string.Empty;
        }

        public void SetSelectFromAndToText(string Key)
        {
            EmptyFromAndToText();

            LastSetKey = Key;

            if (Mod.Type == GameFileType.ESP)
            {
                if (Mod.EspReader.Records.ContainsKey(LastSetKey))
                {
                    var GetRecord = Mod.EspReader.Records[LastSetKey];
                    SetLog("Select:" + GetRecord.FormID + " | " + GetRecord.ParentSig + " " + GetRecord.ChildSig);
                }
            }
            else
            {
                SetLog("Select:" + LastSetKey);
            }

            if (Mod.EspReader.GameCharacters.ContainsKey(Key))
            {
                NpcView.Visibility = Visibility.Visible;
                NpcName.Text = Mod.EspReader.GameCharacters[Key][0].Name;
                NpcSex.Content = Mod.EspReader.GameCharacters[Key][0].Gender.ToString();
            }
            else
            {
                NpcView.Visibility = Visibility.Collapsed;
            }

            if (Key.Length > 0)
            {
                CurrentKeyBox.Visibility = Visibility.Visible;
            }

            if (TransListView != null)
            {
                var GridHandle = TransListView.KeyToFakeGrid(Key);

                if (GridHandle != null)
                {
                    if (GridHandle.Score < 0)
                    {
                        LastSetKey = string.Empty;
                        return;
                    }

                    bool IsCloud = false;
                    GridHandle.SyncData(ref IsCloud);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (GridHandle.Score < 5)
                        {
                            ToStr.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            ToStr.Foreground = new SolidColorBrush((Color)Application.Current.Resources["DefFontColor"]);
                        }


                        FromStr.Text = TransListView.RealLines[TransListView.SelectLineID].SourceText;
                        ToStr.Text = TransListView.RealLines[TransListView.SelectLineID].TransText;

                        UIHelper.ShowButton(ApplyOTButton, true);

                        if (FromStr.Text.Length > 0)
                        {
                            UIHelper.ShowButton(CancelOTButton, true);
                        }

                        if (DeFine.GlobalLocalSetting.AutoSpeak)
                        {
                            SpeechHelper.TryPlaySound(FromStr.Text, true);
                        }

                        Point MousePos = Mouse.GetPosition(ToStr);
                        if (MousePos.X >= 0 && MousePos.X <= ToStr.ActualWidth &&
                            MousePos.Y >= 0 && MousePos.Y <= ToStr.ActualHeight)
                        {
                            ToStr.Focus();
                        }

                        AutoLoadHistoryList();
                    }));

                    DeFine.ExtendWin.SetOriginal(GridHandle.SourceText, Mod.EspReader.ToStringsFile.QueryData(GridHandle.Key));
                }
            }
        }

        public void AutoLoadHistoryList()
        {
            if (HistoryLayer.Visibility == Visibility.Visible)
            {
                HistoryList.Items.Clear();

                var QueryHistorys = TranslatorInterface.GetTranslatorCache(LastSetKey);
                if (QueryHistorys != null)
                {
                    foreach (var Get in QueryHistorys)
                    {
                        HistoryList.Items.Add(new
                        {
                            ChangeTime = Get.ChangeTime,
                            Translated = Get.Translated
                        });
                    }
                }
            }
        }

        private System.Timers.Timer ReloadDebounceTimer;
        private readonly object ReloadLock = new object();
        private bool UseHotReloadFlag;

        public bool ReadTrdWorkState = false;
        public void ReloadData(bool UseHotReload = false, bool ForceReload = false)
        {
            lock (ReloadLock)
            {
                UseHotReloadFlag = UseHotReload;

                if (ForceReload)
                {
                    ReloadDebounceTimer = null;
                }

                if (ReloadDebounceTimer == null)
                {
                    ReloadDebounceTimer = new System.Timers.Timer(200);
                    ReloadDebounceTimer.AutoReset = false;
                    ReloadDebounceTimer.Elapsed += (s, e) =>
                    {
                        if (!ReadTrdWorkState)
                        {
                            new Thread(() =>
                            {
                                ReloadDataFunc(UseHotReloadFlag);
                            }).Start();
                        }
                    };
                }

                ReloadDebounceTimer.Stop();
                ReloadDebounceTimer.Start();
            }
        }

        public void ReloadStringsFile()
        {
           Mod.EspReader.LoadStringsFile();

            if (Mod.EspReader.FromStringsFile.Strings.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FromStringsFile.Visibility = Visibility.Visible;
                    UIHelper.SyncFromStringsFile(Mod.EspReader, TransListView);
                }));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FromStringsFile.Visibility = Visibility.Collapsed;
                }));
            }
        }

        public object LockerAddTrd = new object();
        public string LastSetSig = "";
        public Thread DataLoadingTrd = null;

        public bool DataLoading = false;

        public string CurrentSig = "";
        public void ReloadDataFunc(bool UseHotReload = false)
        {
            lock (LockerAddTrd)
            {
                DataLoading = true;
                ReadTrdWorkState = true;

                if (LastSetSig != CurrentSig)
                {
                    LastSetSig = CurrentSig;
                    TranslatorInterface.Close();
                }

                if (!UseHotReload)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TransListView.Clear();
                    }));
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TransListView.HotReload();
                    }));
                }

                if (!UseHotReload)
                {
                    if (Mod.Type == GameFileType.ESP)
                    {
                        Mod.EspReader.SelectSig(CurrentSig);

                        if (DataLoadingTrd != null)
                        {
                            try
                            {
                                DataLoadingTrd.Abort();
                            }
                            catch { }

                            DataLoadingTrd = null;

                            TransListView.Parent.Dispatcher.Invoke(new Action(() =>
                            {
                                TransListView.Clear();
                            }));
                        }

                        DataLoadingTrd = new Thread(() =>
                        {
                            UIHelper.TransViewSyncEspRecord(Mod.EspReader, TransListView);

                            ReloadStringsFile();

                            Thread.Sleep(100);

                            DataLoading = false;

                            DataLoadingTrd = null;
                        });

                        DataLoadingTrd.Start();
                    }
                    else
                    if (Mod.Type == GameFileType.MCM)
                    {
                        foreach (var GetItem in Mod.MCMReader.MCMItems)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                TransListView.AddRowR(LineRenderer.CreateLine(GetItem.Type, GetItem.EditorID, GetItem.Key, GetItem.SourceText, GetItem.GetTextIfTransR(TranslatorInterface.Instance), 999));
                            }));
                        }

                        DataLoading = false;
                    }
                    else
                    if (Mod.Type == GameFileType.PEX)
                    {
                        string AutoSig = CurrentSig;
                        if (AutoSig == "ALL")
                        {
                            AutoSig = string.Empty;
                        }
                        Mod.PexReader.Core.GetStrings(out List<PexStringItem> Strings, AutoSig);

                        foreach (var GetItem in Strings)
                        {
                            if (GetItem.FunctionRef != null)
                            {
                                int CalcLineIndex = GetItem.FunctionRef.PscStartLineIndex;

                                if (Mod.PexLinks.ContainsKey(GetItem.UniqueKey))
                                {
                                    Mod.PexLinks[GetItem.UniqueKey] = CalcLineIndex;
                                }
                                else
                                {
                                    Mod.PexLinks.Add(GetItem.UniqueKey, CalcLineIndex);
                                }
                            }

                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                TransListView.AddRowR(LineRenderer.CreateLine("Auto", P_Convert.ObjToStr(GetItem.StringTableID), GetItem.UniqueKey, GetItem.Original, "", GetItem.Score));
                            }));
                        }

                        DataLoading = false;
                    }
                    else
                    if (Mod.Type == GameFileType.JSON)
                    {
                        foreach (var GetItem in Mod.RamCacheReader.RamLines)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                TransListView.AddRowR(LineRenderer.CreateLine(GetItem.Type, "", GetItem.Key, GetItem.SourceText, GetItem.TransText, GetItem.Score));
                            }));
                        }

                        DataLoading = false;
                    }
                    else
                    if (Mod.Type == GameFileType.XML)
                    {
                        foreach (var GetItem in Mod.XmlReader.XmlItems)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                TransListView.AddRowR(LineRenderer.CreateLine(GetItem.Type, "", GetItem.Key, GetItem.SourceText, GetItem.TransText, 999));
                            }));
                        }

                        DataLoading = false;
                    }
                }

                ReadTrdWorkState = false;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    TransListView.UpdateVisibleRows(true);
                }));

                TranslatorInterface.PreparingTranslationUnits();
            }
        }

        private void AutoLoadOrSave(object sender, MouseButtonEventArgs e)
        {

        }

        private void CancelTransEsp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ChangeTransState(object sender, MouseButtonEventArgs e)
        {

        }

        private void ApplyTranslatedText(object sender, MouseButtonEventArgs e)
        {

        }

        private void AutoSpeak_Click(object sender, RoutedEventArgs e)
        {
            if (AutoSpeak.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.AutoSpeak = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.AutoSpeak = false;
            }
        }

        private void ReplaceStr(object sender, MouseButtonEventArgs e)
        {
            DeFine.CurrentReplaceView.Show();
        }


        private void CancelTranslatedText(object sender, MouseButtonEventArgs e)
        {

        }


        private string LastUntranslatedKey = null;
        private void EnableHotKey_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _AutoLoop = !_AutoLoop;

            LastUntranslatedKey = null;

            if (_AutoLoop)
            {
                SetHotKeyDot(true);
                NextAutoEnable = 1;

                DeFine.GlobalLocalSetting.TableAuto = true;
                DeFine.GlobalLocalSetting.SaveConfig();
            }
            else
            {
                SetHotKeyDot(false);
                NextAutoEnable = 0;

                DeFine.GlobalLocalSetting.TableAuto = false;
                DeFine.GlobalLocalSetting.SaveConfig();
            }

            e.Handled = true;
        }

        private void ExportToDsd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportToRamCache_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void AutoWordCompletion_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FindNpc_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ImportRamCache_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProcessBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void RefreshDictionary_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

 

        public SearchData CurrentSearchData = new SearchData();
        public void QuickSearch()
        {
            if (SearchBox.Text.Trim().Length > 0)
            {
                NextSearch:
                string FristChar = SearchBox.Text.Substring(0, 1);

                if (!CurrentSearchData.FristChar.Equals(FristChar))
                {
                    CurrentSearchData.KeyWords.Clear();
                }

                CurrentSearchData.FristChar = FristChar;

                string SearchAny = SearchBox.Text;
                EmptyFromAndToText();

                //If we simply highlight all the matched items, that works well for mods with few items. However, it's not ideal for mods with tens of thousands of lines of data. Therefore, we need to search item by item. When the user presses Enter, the system jumps to the first matched item, and pressing it again jumps to the second. The counter is reset when all items are finally matched.

                int PreOffset = -1;
                int Complete = 0;
                string GetKey = "";

                for (int i = 0; i < TransListView.RealLines.Count; i++)
                {
                    bool IsCloud = false;
                    TransListView.RealLines[i].SyncData(ref IsCloud);

                    if (TransListView.RealLines[i].Key.Contains(SearchAny) ||
                        TransListView.RealLines[i].SourceText.Contains(SearchAny) ||
                        TransListView.RealLines[i].TransText.Contains(SearchAny)
                        )
                    {
                        GetKey = TransListView.RealLines[i].Key;

                        if (CurrentSearchData.KeyWords.ContainsKey(SearchAny))
                        {
                            PreOffset = CurrentSearchData.KeyWords[SearchAny];
                        }
                        else
                        {
                            PreOffset = -1;
                            CurrentSearchData.KeyWords.Add(SearchAny, PreOffset);
                        }

                        if (i > PreOffset)
                        {
                            TransListView.Goto(GetKey);
                            CurrentSearchData.KeyWords[SearchAny] = i;
                            Complete = 1;
                            break;
                        }
                    }
                }
                if (PreOffset != -1 && Complete == 0 && CurrentSearchData.KeyWords.Count > 0)
                {
                    //Reset Counter
                    CurrentSearchData.KeyWords.Remove(SearchAny);
                    //Jump back to the first matching target
                    goto NextSearch;
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    {
                        SearchBox.Focus();
                    }));
                }
            }
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                QuickSearch();
            }
        }

        private void ShowHistorys(object sender, MouseButtonEventArgs e)
        {

        }

        private void ShowLocalEngineSettingView(object sender, MouseButtonEventArgs e)
        {

        }
        private void SyncColumnWidth(object sender, MouseButtonEventArgs e)
        {

        }

        private void TestAll_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ToStr_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void Traditional_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ManageCache_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SpeakFromStr(object sender, MouseButtonEventArgs e)
        {
            SpeechHelper.TryPlaySound(FromStr.Text);
        }

        private void SpeakToStr(object sender, MouseButtonEventArgs e)
        {
            SpeechHelper.TryPlaySound(ToStr.Text);
        }
        private void UPSelecter_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TransListView?.UP();
        }

        private void DownSelecter_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TransListView?.Down();
        }

        private void ClearToStr(object sender, MouseButtonEventArgs e)
        {
            ToStr.Text = string.Empty;
        }

        private void CloneFromStr(object sender, MouseButtonEventArgs e)
        {
            if (ToStr.Text.Length == 0)
            {
                ToStr.Text = FromStr.Text;
            }
        }

        private void NextAuto_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            NextAuto();
        }

        WordCompletionManager CompletionManager = null;
        public void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                if (CompletionManager != null && CompletionManager.IsCompletionActive)
                {
                    return;
                }

                if (TransView.IsHitTestVisible == true)
                {
                    e.Handled = true;

                    if (NextAutoEnable == 0)
                    {
                        TransListView?.Down();
                    }
                    else
                    {
                        NextAuto();
                    }
                }
            }
            if (e.Key == Key.F2)
            {
                if (DeFine.GlobalLocalSetting.ViewMode == "Normal")
                {
                    ApplyTranslatedText();
                }
            }
            if (e.Key == Key.F1)
            {
                if (DeFine.GlobalLocalSetting.ViewMode == "Normal")
                {
                    TranslateCurrent();
                }
            }
        }

        public void ApplyTranslatedText()
        {
            if (TransListView != null)
            {
                if (LastSetKey.Trim().Length > 0)
                {
                    var GetGrid = TransListView.KeyToFakeGrid(LastSetKey);

                    if (GetGrid != null)
                    {
                        bool RefCloud = false;
                        GetGrid.SyncData(ref RefCloud);

                        if (GetGrid.TransText.Length > 0)
                        {
                            TranslatorInterface.SetTranslatorHistoryCache(GetGrid.Key, GetGrid.TransText, false);
                        }

                        GetGrid.TransText = ToStr.Text;

                        try
                        {
                            if (CloudDBCache.FindCache(TranslatorInterface.Instance.GetFileUniqueKey(), GetGrid.Key, TranslatorInterface.Instance.To).Equals(GetGrid.TransText))
                            {
                                LocalDBCache.DeleteCache(TranslatorInterface.Instance.GetFileUniqueKey(), GetGrid.Key, TranslatorInterface.Instance.To);

                                var Link = TranslatorInterface.Instance.GetLink();

                                Link[GetGrid.Key] = GetGrid.TransText;


                            }
                            else
                            {
                                Mod.P_Translator.AutoSetLink(GetGrid.Key, GetGrid.SourceText, GetGrid.TransText);
                            }
                        }
                        catch { }

                        TranslatorInterface.SetTranslatorHistoryCache(GetGrid.Key, GetGrid.TransText, false);

                        GetGrid.SyncData(ref RefCloud);
                        GetGrid.SyncUI(TransListView);
                        //DeFine.ExtendWin.SetOriginal(GetGrid.SourceText, DeFine.WorkingWin.GlobalEspReader.StringsReader.QueryData(GetGrid.Key));
                    }

                    Mod.P_Translator.SyncTranslatedCount(RowStyleWin.DictionaryKeys.Count);

                    UIHelper.ShowButton(ApplyOTButton, false);
                }
            }
        }

        public void CanEditTransView(bool Check)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TransView.IsHitTestVisible = Check;
            }));
        }


        public object TranslateLocker = new object();
        public Thread TranslateTrd = null;
        private CancellationTokenSource TranslateCTS = null;

        public bool SingleTrans = false;
        public void TranslateCurrent()
        {
            TranslatorInterface.MakeReady();

            lock (TranslateLocker)
            {
                if (TransListView != null)
                {
                    if (P_Convert.ObjToStr(TranslateOTButtonFont.Content).Equals(UILanguageHelper.UICache["TranslateOTButtonFont"]))
                    {
                        FakeGrid QueryGrid = TransListView.KeyToFakeGrid(LastSetKey);

                        if (QueryGrid != null && TranslateTrd == null)
                        {
                            bool IsCloud = false;
                            QueryGrid.SyncData(ref IsCloud);

                            if (QueryGrid.TransText.Length > 0)
                            {
                                CloudDBCache.DeleteCache(TranslatorInterface.Instance.GetFileUniqueKey(), QueryGrid.Key, TranslatorInterface.Instance.To);
                            }

                            BaseUnit SetUnit = new BaseUnit(TranslatorInterface.Instance.GetFileUniqueKey(), QueryGrid.Key, QueryGrid.Type, QueryGrid.SourceText, QueryGrid.TransText, 100);

                            CanEditTransView(false);

                            TranslateCTS = new CancellationTokenSource();
                            CancellationToken Token = TranslateCTS.Token;

                            TranslateTrd = new Thread(() =>
                            {
                                try
                                {
                                    SingleTrans = true;

                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        TranslateOTButtonFont.Content =
                                            UILanguageHelper.UICache["TranslateOTButtonFont1"] +
                                            "(Click to cancel)";

                                        //ThreadInFo.Visibility = Visibility.Visible;
                                    }));

                                    SetUnit.Translated = string.Empty;
                                    UnitGroup Result =
                                        TranslatorInterface.Instance.Translate(SetUnit, Token, false);

                                    Token.ThrowIfCancellationRequested();

                                    string GetTranslated =
                                        Result.GetFrist().Translated;

                                    CanEditTransView(true);

                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        TranslateOTButtonFont.Content =
                                            UILanguageHelper.UICache["TranslateOTButtonFont"];

                                        if (TranslatorInterface.TranslationStatus == StateControl.Null ||
                                            TranslatorInterface.TranslationStatus == StateControl.Cancel)
                                        {
                                            //ThreadInFo.Visibility = Visibility.Collapsed;
                                        }

                                        ToStr.Text = GetTranslated;
                                    }));
                                }
                                catch (OperationCanceledException)
                                {
                                }
                                finally
                                {
                                    CanEditTransView(true);

                                    this.Dispatcher.Invoke(() =>
                                    {
                                        TranslateOTButtonFont.Content =
                                            UILanguageHelper.UICache["TranslateOTButtonFont"];

                                        //ThreadInFo.Visibility = Visibility.Collapsed;
                                    });

                                    SingleTrans = false;

                                    TranslateCTS?.Dispose();
                                    TranslateCTS = null;

                                    TranslateTrd = null;
                                }
                            });

                            TranslateTrd.Start();
                        }
                    }
                    else
                    {
                        TranslateCTS?.Cancel();

                        InteractiveView.CloseAll();

                        TranslateOTButtonFont.Content =
                            UILanguageHelper.UICache["TranslateOTButtonFont"];

                        SingleTrans = false;
                        //ThreadInFo.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void NextAuto()
        {
            var Lines = TransListView?.RealLines;
            if (Lines == null) return;

            int StartIndex = 0;
            if (LastUntranslatedKey != null)
            {
                int LastIndex = Lines.FindIndex(l => l.Key == LastUntranslatedKey);
                if (LastIndex >= 0)
                    StartIndex = LastIndex + 1;
            }

            int Total = Lines.Count;
            for (int Offset = 0; Offset < Total; Offset++)
            {
                int i = (StartIndex + Offset) % Total;

                bool IsCloud = false;
                var GetLine = Lines[i];
                Lines[i].SyncData(ref IsCloud);

                if ((GetLine.SourceText + GetLine.RealSource).Trim().Length > 0)
                {
                    if (P_Language.DetectLanguageByLine(GetLine.SourceText) != TranslatorInterface.Instance.To)
                    {
                        if (GetLine.TransText.Length == 0 && (new TranslationPreprocessor().IsOnlySymbolsAndSpaces(GetLine.SourceText + GetLine.RealSource)) == false)
                        {
                            if (Lines[i].Score > 0)
                            {
                                LastUntranslatedKey = Lines[i].Key;
                                TransListView.Goto(Lines[i].Key);
                                return;
                            }
                        }
                    }
                }
            }

            LastUntranslatedKey = null;
        }


        private void TranslateOTButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TransTargetType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetSelectValue = P_Convert.ObjToStr((sender as ComboBox).SelectedValue);
            if (GetSelectValue.Trim().Length > 0)
            {
                CurrentSig = GetSelectValue;
                ReloadData();
            }
        }

        public void EnableNormalModel()
        {
            DeFine.GlobalLocalSetting.ViewMode = "Normal";
            NormalModel.Style = (Style)this.FindResource("ModelSelected");
            QuickModel.Style = (Style)this.FindResource("ModelUnSelected");

            NormalModelBlock.Visibility = Visibility.Visible;
            QuickModelBlock.Visibility = Visibility.Collapsed;

            ReloadData(true, true);

            double AutoHeight = DeFine.GlobalLocalSetting.WritingAreaHeight;

            if (AutoHeight < 100)
            {
                AutoHeight = 300;
            }
            WritingArea.Height = new GridLength(AutoHeight, GridUnitType.Pixel);
            SplictLine.Height = new GridLength(3.5, GridUnitType.Pixel);
        }

        public void EnableQuickModel()
        {
            DeFine.GlobalLocalSetting.ViewMode = "Quick";
            NormalModel.Style = (Style)this.FindResource("ModelUnSelected");
            QuickModel.Style = (Style)this.FindResource("ModelSelected");

            NormalModelBlock.Visibility = Visibility.Collapsed;
            QuickModelBlock.Visibility = Visibility.Visible;

            ReloadData(true, true);

            WritingArea.Height = new GridLength(0, GridUnitType.Pixel);
            SplictLine.Height = new GridLength(0, GridUnitType.Pixel);
        }

        private void NormalModel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnableNormalModel();
        }
        private void QuickModel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnableQuickModel();
        }

        private void ToStr_TextChanged(object sender, EventArgs e)
        {

        }

        private void ShowDataBaseR_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ClearCacheR_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void UserTranslationCache_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloudTranslationCache_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearCacheViewClose_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

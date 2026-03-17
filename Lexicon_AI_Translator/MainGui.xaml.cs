using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using LexTranslator.FileManagement;
using LexTranslator.SkyrimManage;
using LexTranslator.SkyrimManagement;
using LexTranslator.SkyrimModManager;
using LexTranslator.TranslateManage;
using LexTranslator.UIManage;
using LexTranslator.UIManagement;
using Newtonsoft.Json;
using System.Windows.Threading;
using static LexTranslator.SkyrimManagement.DSDConverter;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Interop;
using PexInterface;
using static PexInterface.PexHeuristicAnalysis;
using PhoenixEngine;
using PhoenixEngine.Translate;
using PhoenixEngine.Unit;
using LexTranslator.ConvertManager;
using PhoenixEngine.ADO;
using PhoenixEngine.Platform.LocalAI;
using PhoenixEngine.Language;
using PhoenixEngine.Platform;
using PhoenixEngine.Additional;
using PhoenixEngine.Events;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for MainGui.xaml
    /// </summary>
    public partial class MainGui : Window
    {
        #region Breathing Light
        public Storyboard XTGlowLoopStoryboard = null;
        #endregion

        #region WinControl

        private void Min_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public int SizeChangeState = 0;
        private double OriginalLeft;
        private double OriginalTop;
        private double OriginalWidth;
        private double OriginalHeight;
        private void AutoMax_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var Screen = SystemParameters.WorkArea;

            if (SizeChangeState == 0)
            {
                OriginalLeft = this.Left;
                OriginalTop = this.Top;
                OriginalWidth = this.Width;
                OriginalHeight = this.Height;

                double TargetWidth = Screen.Width - 100;
                double TargetHeight = Screen.Height - 100;

                this.Width = TargetWidth;
                this.Height = TargetHeight;

                this.Left = Screen.Left + (Screen.Width - TargetWidth) / 2;
                this.Top = Screen.Top + (Screen.Height - TargetHeight) / 2;

                SizeChangeState = 1;
            }
            else
            {
                this.Left = OriginalLeft;
                this.Top = OriginalTop;
                this.Width = OriginalWidth;
                this.Height = OriginalHeight;

                SizeChangeState = 0;
            }
        }
        private void Close_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DeFine.CloseAny();
        }

        #endregion
        public MainGui()
        {
            DeFine.GlobalLocalSetting.ReadConfig();

            if (DeFine.GlobalLocalSetting.Style == 1)
            {
                UIHelper.SetGlobalStyle(UIHelper.StyleType.BlueStyle);
            }
            if (DeFine.GlobalLocalSetting.Style == 2)
            {
                UIHelper.SetGlobalStyle(UIHelper.StyleType.RetroStyle);
            }

            InitializeComponent();
        }

        public void AutoShowTraditional()
        {
            bool IsVisible = false;
            if (DeFine.GlobalLocalSetting.SourceLanguage == Languages.SimplifiedChinese || DeFine.GlobalLocalSetting.SourceLanguage == Languages.TraditionalChinese)
            {
                if (DeFine.GlobalLocalSetting.SourceLanguage == Languages.TraditionalChinese || DeFine.GlobalLocalSetting.SourceLanguage == Languages.SimplifiedChinese)
                {
                    Traditional.Visibility = Visibility.Visible;
                    IsVisible = true;
                }
            }

            if (!IsVisible)
            {
                Traditional.Visibility = Visibility.Collapsed;
            }
        }

        public YDListView TransViewList = null;

        private ScanAnimator ScanAnimator = null;

        public IntPtr MainHwnd = IntPtr.Zero;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DeFine.Init(this);

            AutoShowTraditional();

            TranslatorInterface.Init();

            MainHwnd = new WindowInteropHelper(DeFine.WorkingWin).Handle;

            UILanguageHelper.ChangeLanguage(DeFine.GlobalLocalSetting.CurrentUILanguage);

            UILanguages.Items.Clear();

            foreach (var GetLang in UILanguageHelper.GetSupportedLanguages())
            {
                if (GetLang != Languages.Auto)
                    UILanguages.Items.Add(GetLang.ToString());
            }

            UILanguages.SelectedValue = DeFine.GlobalLocalSetting.CurrentUILanguage.ToString();

            EngineEvents.SetBookTranslateCallback += BookTransCallBack;

            SetSelectedNav("TransHub");

            if (TransViewList == null)
            {
                TransViewList = new YDListView(TransView);
                TransViewList.Clear();
            }

            GlobalRamCacheReader = new RamCacheReader();
            //GlobalEspReader = new EspReader();
            GlobalMCMReader = new MCMReader();
            GlobalPexReader = new PexHeuristicAnalysis();
            GlobalXmlReader = new R_XmlReader();

            ScanAnimator = new ScanAnimator(ScanTransform, ProcessBar, 60);

            LastSetLogButton = InputLogButton;

            SyncConfig();

            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    try
                    {
                        GetStatisticsR();
                    }
                    catch { }
                }
            }).Start();

            SyncTransStateUI();

            Phoenix.From = DeFine.GlobalLocalSetting.SourceLanguage;
            Phoenix.To = DeFine.GlobalLocalSetting.TargetLanguage;

            TranslatorInterface.Instance.From = Phoenix.From;
            TranslatorInterface.Instance.To = Phoenix.To;

            SelectFristSettingNav();

            UIHelper.SyncAvalonEditTextLayout();

            //If you like anime, you can place a CG.png in the program's installation directory, making sure the dimensions are correct. It will display an anime character at the top of the software.
            string CheckCGPath = DeFine.GetFullPath(@"\CG.png");
            if (File.Exists(CheckCGPath))
            {
                DeFine.CG = new CGView();
                DeFine.CG.Hide();
                DeFine.CG.CG.Source = new BitmapImage(new Uri(CheckCGPath));

                DeFine.CG.Owner = this;
                DeFine.CG.Show();
                SyncCGLocation();
            }

            UIHelper.SyncNodes();
        }


        #region Drag
        public bool IsDragEnter = false;

        private void ModTransView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                string[] OneFile = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (OneFile == null)
                {
                    return;
                }
                if (OneFile.Length == 0)
                {
                    return;
                }
            }

            ModTransView.Visibility = Visibility.Collapsed;
            DragDropView.Visibility = Visibility.Visible;
            IsDragEnter = true;
        }

        private void ModTransView_DragLeave(object sender, DragEventArgs e)
        {
            if (e != null)
                if (e.Data != null)
                {
                    string[] OneFile = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (OneFile == null)
                    {
                        return;
                    }
                    if (OneFile.Length == 0)
                    {
                        return;
                    }
                }

            ModTransView.Visibility = Visibility.Visible;
            DragDropView.Visibility = Visibility.Collapsed;
            if (IsDragEnter)
            {
                IsDragEnter = false;
            }
        }

        private void ModTransView_Drop(object sender, DragEventArgs e)
        {
            if (e != null)
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] OneFile = (string[])e.Data.GetData(DataFormats.FileDrop);

                    if (OneFile.Length > 0)
                    {
                        //Fix Long Path
                        string GetFilePath = Path.GetFullPath(OneFile[0]);
                        if (GetFilePath.Length >= 260)
                        {
                            GetFilePath = @"\\?\" + GetFilePath;
                        }

                        if (File.Exists(GetFilePath))
                        {
                            new Thread(() =>
                            {
                                this.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    LoadAny(GetFilePath);
                                }), System.Windows.Threading.DispatcherPriority.Background);
                            }).Start();

                            ModTransView_DragLeave(null, null);
                        }
                    }
                }
        }

        private void Traditional_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            new TraditionalConvert().Show();
        }

        #endregion


        public void SyncCGLocation()
        {
            if (DeFine.CG != null)
            {
                DeFine.CG.Top = (this.Top - DeFine.CG.ActualHeight) + 1;
                DeFine.CG.Left = this.Left + 100;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DeFine.CloseAny();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TransViewList != null)
            {
                if (TransViewList.RealLines != null)
                {
                    ReloadData(true);
                }
            }

            AutoSizeHistoryList();
            SyncCGLocation();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            MutiWinHelper.SyncLocation();
            SyncCGLocation();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                if (TransView.IsHitTestVisible == true)
                {
                    e.Handled = true;

                    if (NextAutoEnable == 0)
                    {
                        TransViewList?.Down();
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

        public void ShowMenu(bool Show)
        {
            var Rotate = NavBtn.RenderTransform as RotateTransform;

            if (Show)
            {
                var Storyboard = this.FindResource("ExpandStoryboard") as Storyboard;

                if (Storyboard != null)
                {
                    Storyboard.Begin();
                }

                Mask.Visibility = Visibility.Visible;

                Mask.Tag = 1;

                if (Rotate != null)
                {
                    Rotate.Angle = 180;
                }

                MainNavIsExpanded = true;
            }
            else
            {
                var Storyboard = this.FindResource("CollapseStoryboard") as Storyboard;

                if (Storyboard != null)
                {
                    Storyboard.Begin();
                }

                Mask.Visibility = Visibility.Collapsed;

                Mask.Tag = 0;

                if (Rotate != null)
                {
                    Rotate.Angle = 0;
                }

                MainNavIsExpanded = false;
            }
        }

        public void ShowLeftMenu(bool Show)
        {
            if (DeFine.WorkingWin != null)
            {
                DeFine.WorkingWin.Dispatcher.Invoke(new Action(() =>
                {
                    if (Show)
                    {
                        UIHelper.LeftMenuIsShow = true;
                        Mask.Visibility = Visibility.Visible;
                        Storyboard Storyboard = (Storyboard)this.Resources["ExpandMenu"];
                        Storyboard.Begin();

                        IsExpanded = true;
                        LeftMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        UIHelper.LeftMenuIsShow = false;
                        Mask.Visibility = Visibility.Collapsed;
                        Storyboard Storyboard = (Storyboard)this.Resources["CollapseMenu"];
                        Storyboard.Begin();

                        IsExpanded = false;
                    }
                }));
            }
        }

        public bool IsExpanded = false;
        private void ShowLeftMenu(object sender, MouseButtonEventArgs e)
        {
            if (IsExpanded)
            {
                SyncAnimation();
                ShowLeftMenu(false);
                LogView.Visibility = Visibility.Collapsed;
            }
            else
            {
                SyncAnimation();
                ShowLeftMenu(true);
                LogView.Visibility = Visibility.Visible;
            }
        }


        private void Mask_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ConvertHelper.ObjToInt(Mask.Tag) == 0)
            {
                ShowLeftMenu(false);
                LogView.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowMenu(false);
                LogView.Visibility = Visibility.Collapsed;
            }
        }


        private void ContextGeneration_Click(object sender, RoutedEventArgs e)
        {
            //if (ContextGeneration.IsChecked == true)
            //{
            //    RightContextIndicator.Visibility = Visibility.Visible;
            //    Phoenix.Config.ContextEnable = true;
            //}
            //else
            //{
            //    RightContextIndicator.Visibility = Visibility.Collapsed;
            //    Phoenix.Config.ContextEnable = false;
            //}

            //Phoenix.SaveConfig();
        }


        public void SetLog(string Str)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                CurrentLog.Text = "Log: " + Str;
            }));
        }

        public void GetStatisticsR()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                CalcStatistics();
            }));
        }

        public int GlobalTransCount = 0;

        public void GetGlobalTransCount()
        {
            if (CurrentTransType == 2)
            {
                if (ConvertHelper.ObjToStr(TypeSelector.SelectedValue).Equals("ALL"))
                {
                    if (TransViewList != null)
                        GlobalTransCount = TransViewList.RealLines.Count;
                }
            }
            else
            {
                if (TransViewList != null)
                    GlobalTransCount = TransViewList.RealLines.Count;
            }
        }

        public void CalcStatistics()
        {
            try
            {
                int ModifyCount = 0;
                var GetBatchCore = TranslatorInterface.Instance.GetBatchCore();
                if (GetBatchCore != null)
                {
                    ModifyCount = GetBatchCore.TranslatedCount;
                }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (TranslatorInterface.Instance != null)
                        if (TranslatorInterface.Instance.GetBatchCore() != null)
                        {
                            var BatchCore = TranslatorInterface.Instance.GetBatchCore();
                            if (ScanAnimator != null)
                            {
                                if (ModifyCount > 0 && BatchCore.IsWork && !BatchCore.IsStop)
                                {
                                    ScanAnimator.Start();
                                }
                                else
                                {
                                    ScanAnimator.Stop();
                                }
                            }

                            if ((BatchCore.IsWork && !BatchCore.IsStop) || SingleTrans)
                            {
                                int Current = BatchCore.GetWorkingThreadCount();

                                if (SingleTrans)
                                {
                                    ThreadInFoFont.Content = string.Format("Thread(Current:{0},Max:{1})", Current + 1, Phoenix.Config.MaxThreadCount + 1);
                                }
                                else
                                {
                                    ThreadInFoFont.Content = string.Format("Thread(Current:{0},Max:{1})", Current, Phoenix.Config.MaxThreadCount);
                                }
                            }
                            else
                            if (BatchCore.IsWork && BatchCore.IsStop)
                            {
                                ThreadInFoFont.Content = string.Format("Thread(Current:0,Max:{0})", Phoenix.Config.MaxThreadCount);
                            }
                        }
                        else
                        {
                            if (SingleTrans)
                            {
                                ThreadInFoFont.Content = string.Format("Thread(Current:{0},Max:{1})", 1, Phoenix.Config.MaxThreadCount + 1);
                            }
                        }

                    if (TransViewList != null)
                    {
                        GetGlobalTransCount();

                        if (ReadTrdWorkState)
                        {
                            if (TranslatorInterface.TranslationStatus == StateControl.Cancel || TranslatorInterface.TranslationStatus == StateControl.Null)
                            {
                                TransProcess.Content = string.Format("Loading({0}/{1})", ModifyCount, GlobalTransCount);
                            }

                            TypeSelector.Opacity = 0.5;
                            TypeSelector.IsEnabled = false;

                            ViewModel.Opacity = 0.5;
                            ViewModel.IsHitTestVisible = false;
                        }
                        else
                        {
                            if (TranslatorInterface.Instance != null)
                            {
                                var BatchCore = TranslatorInterface.Instance.GetBatchCore();
                                if (BatchCore != null)
                                {
                                    if (BatchCore.ProcStage < 2)
                                    {
                                        return;
                                    }
                                }
                                
                            }

                            if (TranslatorInterface.TranslationStatus == StateControl.Cancel || TranslatorInterface.TranslationStatus == StateControl.Null)
                            {
                                TransProcess.Content = string.Format("STRINGS({0}/{1})", ModifyCount, GlobalTransCount);
                            }

                            TypeSelector.Opacity = 1;
                            TypeSelector.IsEnabled = true;

                            ViewModel.Opacity = 1;
                            ViewModel.IsHitTestVisible = true;
                        }

                        double GetRate = ((double)ModifyCount / (double)GlobalTransCount);
                        if (GetRate > 0)
                        {
                            try
                            {
                                if (!Double.IsInfinity(GetRate))
                                    ProcessBar.Width = ProcessBarControl.ActualWidth * GetRate;
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                ProcessBar.Width = 0;
                            }
                            catch { }
                        }
                    }
                }));
            }
            catch { }
        }

        public void LoadAny()
        {
            var Dialog = new Microsoft.Win32.OpenFileDialog();
            Dialog.Title = "Please select a file";
            Dialog.Filter = "All files|*.*";
            Dialog.Multiselect = false;

            if (Dialog.ShowDialog() == true)
            {
                string SelectedFile = Dialog.FileName;
                LoadAny(SelectedFile);
            }
        }

        public int CurrentTransType = 0;
        public string FModName = "";
        public string LModName = "";
        string LastSetPath = "";

        public RamCacheReader GlobalRamCacheReader = null;
        public MCMReader GlobalMCMReader = null;
        public PexHeuristicAnalysis GlobalPexReader = null;
        public R_XmlReader GlobalXmlReader = null;
        public Dictionary<string,int>PexLinks = new Dictionary<string,int>();

        //public List<ObjSelect> CanSetSelecter = new List<ObjSelect>();
        //public ObjSelect CurrentSelect = ObjSelect.Null;

        public object LockerAddTrd = new object();
        public bool ReadTrdWorkState = false;

        public void ReloadStringsFile()
        {
            EspReader.LoadStringsFile();

            if (EspReader.FromStringsFile.Strings.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FromStringsFile.Visibility = Visibility.Visible;
                    UIHelper.SyncFromStringsFile(TransViewList);
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

        public string LastSetSig = "";
        public Thread DataLoadingTrd = null;
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
                        TransViewList.Clear();
                    }));
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TransViewList.HotReload();
                    }));
                }

                if (!UseHotReload)
                {
                    if (CurrentTransType == 2)
                    {
                        EspReader.SelectSig(CurrentSig);

                        if (DataLoadingTrd != null)
                        {
                            try
                            {
                                DataLoadingTrd.Abort();
                            }
                            catch { }

                            DataLoadingTrd = null;

                            TransViewList.Parent.Dispatcher.Invoke(new Action(() =>
                            {
                                TransViewList.Clear();
                            }));
                        }

                        DataLoadingTrd = new Thread(() =>
                        {
                            UIHelper.TransViewSyncEspRecord(TransViewList);

                            ReloadStringsFile();

                            Thread.Sleep(100);

                            DataLoading = false;

                            DataLoadingTrd = null;
                        });

                        DataLoadingTrd.Start();

                        // SkyrimDataLoader.Load(CurrentSelect, GlobalEspReader, TransViewList);
                    }
                    else
                    if (CurrentTransType == 1)
                    {
                        foreach (var GetItem in GlobalMCMReader.MCMItems)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                TransViewList.AddRowR(LineRenderer.CreateLine(GetItem.Type, GetItem.EditorID, GetItem.Key, GetItem.SourceText, GetItem.GetTextIfTransR(), 999));
                            }));
                        }

                        DataLoading = false;
                    }
                    else
                    if (CurrentTransType == 3)
                    {
                        string AutoSig = CurrentSig;
                        if (AutoSig == "ALL")
                        {
                            AutoSig = string.Empty;
                        }
                        GlobalPexReader.Core.GetStrings(out List<PexStringItem> Strings, AutoSig);

                        foreach (var GetItem in Strings)
                        {
                            int CalcLineIndex = GetItem.FunctionRef.PscStartLineIndex;

                            if (PexLinks.ContainsKey(GetItem.UniqueKey))
                            {
                                PexLinks[GetItem.UniqueKey] = CalcLineIndex;
                            }
                            else
                            {
                                PexLinks.Add(GetItem.UniqueKey, CalcLineIndex);
                            }

                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                TransViewList.AddRowR(LineRenderer.CreateLine(CurrentSig, ConvertHelper.ObjToStr(GetItem.StringTableID), GetItem.UniqueKey, GetItem.Original, "", GetItem.Score));
                            }));
                        }

                        DataLoading = false;
                    }
                    else
                    if (CurrentTransType == 6)
                    {
                        foreach (var GetItem in GlobalRamCacheReader.RamLines)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                TransViewList.AddRowR(LineRenderer.CreateLine(GetItem.Type, "", GetItem.Key, GetItem.SourceText, GetItem.TransText, GetItem.Score));
                            }));
                        }

                        DataLoading = false;
                    }
                    else
                    if (CurrentTransType == 11)
                    {
                        foreach (var GetItem in GlobalXmlReader.XmlItems)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                TransViewList.AddRowR(LineRenderer.CreateLine(GetItem.Type, "", GetItem.Key, GetItem.SourceText, GetItem.TransText, 999));
                            }));
                        }

                        DataLoading = false;
                    }
                }

                ReadTrdWorkState = false;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    TransViewList.UpdateVisibleRows(true);
                }));

                TranslatorInterface.PreparingTranslationUnits();
            }
        }
        public bool CheckDictionary()
        {
            string SetPath = DeFine.GetFullPath(@"\Librarys\" + Phoenix.LastLoadFileName + ".Json");
            if (File.Exists(SetPath))
            {
                return true;
            }
            return false;
        }

        public void ReSetTransTargetType(List<string> Types)
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

        private System.Timers.Timer ReloadDebounceTimer;
        private readonly object ReloadLock = new object();
        private bool UseHotReloadFlag;

        public void ReloadData(bool UseHotReload = false, bool ForceReload = false)
        {
            if (LoadSaveState != 1)
                return;
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


        public void SetTittle(string Tittle = "")
        {
            if (Tittle.Trim().Length > 0)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    Caption.Content = string.Format("Lex - {0}", Tittle);
                    this.Title = Tittle;
                }));
            }
            else
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    Caption.Content = "Lex";
                    this.Title = "Lex";
                }));
            }
        }

        public void CloseAllPointer()
        {
            GlobalXmlReader.Close();
            GlobalRamCacheReader.Close();
            EspReader.Close();
            GlobalMCMReader.Close();
            GlobalPexReader.Core.Close();
        }

        public bool DataLoading = false;
        public void LoadAny(string FilePath)
        {
            LastSetSig = string.Empty;

            CancelBatchTranslation();

            IsValidFile = false;

            PexLinks.Clear();

            SetLog("Load:" + FilePath);

            if (System.IO.File.Exists(FilePath))
            {
                TranslatorInterface.Close();
                TranslatorInterface.Instance.ClearAICache();
                CloseAllPointer();
                //FromStr.Text = "";
                //ToStr.Text = "";

                string GetFileName = FilePath.Substring(FilePath.LastIndexOf(@"\") + @"\".Length);
                //Caption.Text = GetFileName;

                Phoenix.LoadFile(FilePath);

                string GetModName = GetFileName;
                FModName = LModName = GetModName;
                if (LModName.Contains("."))
                {
                    LModName = LModName.Substring(0, LModName.LastIndexOf("."));
                }

                if (!CheckDictionary())
                {
                    RefreshDictionary.Opacity = 0.5;
                }
                else
                {
                    RefreshDictionary.Opacity = 1;
                }

                CurrentTransType = 0;

                YDDictionaryHelper.ReadDictionary(GetModName);

                LastSetPath = FilePath;

                if (FilePath.ToLower().EndsWith(".xml"))
                {
                    SetTittle(FModName);
                    CurrentTransType = 11;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TransViewList.Clear();
                    }));

                    GlobalXmlReader.Load(LastSetPath);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        CancelBtn.Opacity = 1;
                        CancelBtn.IsEnabled = true;
                        LoadSaveState = 1;
                    }));

                    ReSetTransTargetType(null);
                    ReloadData();

                    IsValidFile = true;
                }

                if (FilePath.ToLower().EndsWith(".json"))
                {
                    SetTittle(FModName);
                    CurrentTransType = 6;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TransViewList.Clear();
                    }));

                    GlobalRamCacheReader.Load(LastSetPath);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        CancelBtn.Opacity = 1;
                        CancelBtn.IsEnabled = true;
                        LoadSaveState = 1;
                    }));

                    ReSetTransTargetType(null);
                    ReloadData();

                    IsValidFile = true;
                }
                if (FilePath.ToLower().EndsWith(".pex"))
                {
                    SetTittle(FModName);
                    CurrentTransType = 3;

                    LastSetPath = FilePath;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TransViewList.Clear();
                    }));

                    string SetPsc = "";

                    CodeGenStyle AutoStyle = CodeGenStyle.Papyrus;

                    if (DeFine.GlobalLocalSetting.GenCSharp)
                    {
                        AutoStyle = CodeGenStyle.CSharp;
                    }

                    GlobalPexReader?.Core.LoadPex(LastSetPath).ReadStrings().GetPsc(out SetPsc, DeFine.GlobalLocalSetting.ShowAssembly, AutoStyle).AnalysisStrings();

                    List<string> Types = new List<string>();
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (var GetType in GlobalPexReader.Core.HeuristicCore.Types)
                        {
                            Types.Add(GetType);
                        }
                    }));

                    DeFine.CurrentCodeView.SetText(SetPsc);

                    double CalcLeft = this.Left + this.ActualWidth + 1;
                    double CalcTop = this.Top;
                    double IDEHeight = this.Height;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        CancelBtn.Opacity = 1;
                        CancelBtn.IsEnabled = true;
                        LoadSaveState = 1;
                    }));

                    ReSetTransTargetType(Types);
                    ReloadData();

                    IsValidFile = true;
                }
                if (FilePath.ToLower().EndsWith(".txt"))
                {
                    SetTittle(FModName);
                    CurrentTransType = 1;

                    LastSetPath = FilePath;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TransViewList.Clear();
                    }));

                    GlobalMCMReader.LoadMCM(LastSetPath);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        CancelBtn.Opacity = 1;
                        CancelBtn.IsEnabled = true;
                        LoadSaveState = 1;
                    }));

                    ReSetTransTargetType(null);
                    ReloadData();

                    IsValidFile = true;
                }
                if (FilePath.ToLower().EndsWith(".esp") || FilePath.ToLower().EndsWith(".esm") || FilePath.ToLower().EndsWith(".esl"))
                {
                    SetTittle(FModName);
                    CurrentTransType = 2;

                    LastSetPath = FilePath;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TransViewList.Clear();
                    }));

                    EspReader.LoadEsp(FilePath);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        CancelBtn.Opacity = 1;
                        CancelBtn.IsEnabled = true;
                        LoadSaveState = 1;
                    }));

                    ReSetTransTargetType(EspReader.Types);
                    //ReloadData();

                    IsValidFile = true;


                    if (DeFine.GlobalLocalSetting.EnableLanguageDetect)
                    {
                        Phoenix.From = DetectLang();
                    }
                }

                if (CurrentTransType != 0)
                {
                    TranslatorInterface.FristInit = false;
                    LoadSaveState = 1;
                    CheckLoadSaveButtonState();
                }
            }
        }

        public void CancelBatchTranslation()
        {
            try
            {
                TranslatorInterface.TranslationStatus = StateControl.Cancel;

                TranslatorInterface.SyncTransState(new Action(() =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        SyncTransStateUI();
                    }));
                }), false);
            }
            catch { }
        }

        private void CancelTransEsp(object sender, MouseButtonEventArgs e)
        {
            CancelAny();
        }

        public void CancelAny()
        {
            GlobalTransCount = 0;

            CancelBatchTranslation();

            EmptyFromAndToText();

            Phoenix.ChangeUniqueKey(0);

            CurrentSearchData = new SearchData();

            DeFine.CurrentCodeView.Dispatcher.Invoke(new Action(() =>
            {
                DeFine.CurrentCodeView.TextEditor.Text = string.Empty;
            }));

            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    SearchBox.Text = string.Empty;

                    LoadFileButton.Content = UILanguageHelper.UICache["LoadFileButton"];

                    TransViewList?.Clear();

                    CloseAllPointer();

                    TranslatorInterface.Instance.GetLink().Clear();
                    TranslatorInterface.Instance.ReInit();
                    LoadSaveState = 0;

                    CancelBtn.Opacity = 0.3;
                    CancelBtn.IsEnabled = false;

                    TypeSelector.Items.Clear();
                    YDDictionaryHelper.Close();

                    FromStringsFile.Visibility = Visibility.Collapsed;
                }
                catch { }
            }));

            SetTittle();

            new Thread(() =>
            {
                Thread.Sleep(500);

                this.Dispatcher.Invoke(new Action(() =>
                {
                    TransViewList.Clear();
                }));

                GlobalTransCount = 0;
            }).Start();

            TranslatorInterface.Close();

            TranslatorInterface.Instance.GetLink().Clear();
        }

        public bool IsValidFile = false;
        public int LoadSaveState = 0;
        private void AutoLoadOrSave(object sender, MouseButtonEventArgs e)
        {
            this.LoadFileButton.Dispatcher.Invoke(new Action(() =>
            {
                if (ConvertHelper.ObjToStr(LoadFileButton.Content).Equals(UILanguageHelper.UICache["LoadFileButton2"]))
                {
                    return;
                }
            }));

            if (LoadSaveState == 0)
            {
                LoadAny();
            }
            else
            {
                SaveFile();
            }

            CheckLoadSaveButtonState();
        }

        public void SaveFile()
        {
            new Thread(() =>
            {
                this.LoadFileButton.Dispatcher.Invoke(new Action(() =>
                {
                    LoadFileButton.Content = UILanguageHelper.UICache["LoadFileButton2"];
                }));

                try
                {
                    CancelBatchTranslation();

                    EmptyFromAndToText();

                    CalcStatistics();

                    Thread.Sleep(100);

                    if (CurrentTransType == 6)
                    {
                        //Set Trans Data
                        UPDateFile(false);
                    }
                    else
                    {
                        //Set Trans Data
                        UPDateFile(true);
                    }

                    LoadSaveState = 0;

                    this.CancelBtn.Dispatcher.Invoke(new Action(() =>
                    {
                        CancelBtn.Opacity = 0.3;
                        CancelBtn.IsEnabled = false;
                    }));

                    string GetFilePath = LastSetPath.Substring(0, LastSetPath.LastIndexOf(@"\")) + @"\";
                    string GetFileFullName = LastSetPath.Substring(LastSetPath.LastIndexOf(@"\") + @"\".Length);
                    string GetFileSuffix = GetFileFullName.Split('.')[1];
                    string GetFileName = GetFileFullName.Split('.')[0];

                    var Link = TranslatorInterface.Instance.GetLink();

                    if (DeFine.GlobalLocalSetting.UseFullPunctuation)
                    {
                        Link.CheckLinks(new Action<string, string,bool>((string Key,string Value,bool Unique) => 
                        {
                            if (Value.Length > 0)
                            {
                                Link[Key] = Value
                               .Replace(",", "，")
                               .Replace(".", "。")
                               .Replace(":", "：")
                               .Replace(";", "；")
                               .Replace("!", "！")
                               .Replace("?", "？");
                            }
                        }));
                    }

                    if (CurrentTransType == 11)
                    {
                        if (Link.Count > 0)
                        {
                            if (GlobalXmlReader.XmlItems.Count > 0)
                            {
                                GlobalXmlReader.Save(LastSetPath);
                            }
                        }
                    }
                    else
                    if (CurrentTransType == 6)
                    {
                        if (Link.Count > 0)
                        {
                            if (GlobalRamCacheReader != null)
                            {
                                if (!GlobalRamCacheReader.Save(LastSetPath))
                                {
                                    MessageBox.Show("Build RamCache Error!");
                                }
                            }
                        }
                    }
                    else
                    if (CurrentTransType == 3)
                    {
                        if (Link.Count > 0)
                        {
                            if (GlobalPexReader != null)
                            {
                                GlobalPexReader.Core.GetStrings(out List<PexStringItem> Strings);

                                int TranslateCount = 0;
                                for (int i = 0; i < Strings.Count; i++)
                                {
                                    var StringItem = Strings[i];
                                    StringItem.Translated = TranslatorInterface.Instance.GetLink(StringItem.UniqueKey);
                                    if (StringItem.Translated.Length > 0)
                                    {
                                        TranslateCount++;
                                    }
                                }

                                if (TranslateCount > 0)
                                {
                                    string GetBackUPPath = GetFilePath + GetFileFullName + ".backup";

                                    if (!File.Exists(GetBackUPPath))
                                    {
                                        File.Copy(LastSetPath, GetBackUPPath);
                                    }

                                    GlobalPexReader.Core.SavePex(LastSetPath, out int SaveState).Close();

                                    if (SaveState > 0 == false)
                                    {
                                        MessageBox.Show("Build Script Error!");
                                    }
                                }
                            }
                        }
                    }
                    else
                    if (CurrentTransType == 2)
                    {
                        string TempFilePath = LastSetPath + ".Temp";

                        int ModifyCount = EspReader.SaveEsp(TempFilePath);

                        if (ModifyCount == 0)
                        {
                            if (File.Exists(TempFilePath))
                            {
                                File.Delete(TempFilePath);
                            }
                        }
                        else
                        {
                            string GetBackUPPath = LastSetPath + ".backup";

                            if (!File.Exists(GetBackUPPath))
                            {
                                File.Copy(LastSetPath, GetBackUPPath);
                            }

                            if (File.Exists(LastSetPath))
                            {
                                File.Delete(LastSetPath);
                            }

                            if (File.Exists(TempFilePath))
                            {
                                File.Move(TempFilePath, LastSetPath);
                            }
                        }
                    }
                    else
                    if (CurrentTransType == 1)
                    {
                        if (Link.Count > 0)
                            if (GlobalMCMReader != null)
                            {
                                string GetBackUPPath = GetFilePath + GetFileFullName + ".backup";

                                if (!File.Exists(GetBackUPPath))
                                {
                                    File.Copy(LastSetPath, GetBackUPPath);
                                }

                                if (File.Exists(LastSetPath))
                                {
                                    File.Delete(LastSetPath);
                                }

                                GlobalMCMReader.SaveMCMConfig(LastSetPath);

                                if (!File.Exists(LastSetPath))
                                {
                                    MessageBox.Show("Save File Error!");
                                    File.Copy(GetBackUPPath, LastSetPath);
                                }
                            }
                    }

                    TranslatorInterface.WriteDictionary();
                    YDDictionaryHelper.CreatDictionary();
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                CancelTransEsp(null, null);
            }).Start();
        }

        public void CheckLoadSaveButtonState()
        {
            if (IsValidFile)
            {
                if (LoadSaveState == 0)
                {
                    LoadFileButton.Content = UILanguageHelper.UICache["LoadFileButton"];
                }
                else
                {
                    LoadFileButton.Content = UILanguageHelper.UICache["LoadFileButton1"];
                }
            }
            else
            {
                LoadFileButton.Content = UILanguageHelper.UICache["LoadFileButton"];
                LoadSaveState = 0;
            }
        }
        public string CurrentSig = "";
        private void TransTargetType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetSelectValue = ConvertHelper.ObjToStr((sender as ComboBox).SelectedValue);
            if (GetSelectValue.Trim().Length > 0)
            {
                CurrentSig = GetSelectValue;
                ReloadData();
            }
        }

        #region Search

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            QuickSearch();
        }


        public class SearchData
        {
            public string FristChar = "";
            public Dictionary<string, int> KeyWords = new Dictionary<string, int>();
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

                for (int i = 0; i < TransViewList.RealLines.Count; i++)
                {
                    bool IsCloud = false;
                    TransViewList.RealLines[i].SyncData(ref IsCloud);

                    if (TransViewList.RealLines[i].Key.Contains(SearchAny) ||
                        TransViewList.RealLines[i].SourceText.Contains(SearchAny) ||
                        TransViewList.RealLines[i].TransText.Contains(SearchAny)
                        )
                    {
                        GetKey = TransViewList.RealLines[i].Key;

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
                            TransViewList.Goto(GetKey);
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

        #endregion

        private void LangFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string GetValue = ConvertHelper.ObjToStr(LangFrom.SelectedValue);

            //if (GetValue.Trim().Length > 0)
            //{
            //    DeFine.GlobalLocalSetting.SourceLanguage = (Languages)Enum.Parse(typeof(Languages), GetValue);
            //    DeFine.LocalConfigView.SFrom.SelectedValue = GetValue;

            //    Engine.From = DeFine.GlobalLocalSetting.SourceLanguage;
            //}
        }

        public void UPDateUI()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                EmptyFromAndToText();
                TranslatorInterface.Instance.GetLink().Clear();
                Phoenix.GetTranslatedCount(Phoenix.GetFileUniqueKey());

                if (TransViewList != null)
                {
                    if (TransViewList.Rows > 0)
                    {
                        TransViewList.QuickRefresh();
                    }
                }
            }));
        }

        private void LangTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string GetValue = ConvertHelper.ObjToStr(LangTo.SelectedValue);

            //if (GetValue.Trim().Length > 0)
            //{
            //    DeFine.GlobalLocalSetting.TargetLanguage = (Languages)Enum.Parse(typeof(Languages), GetValue);
            //    DeFine.LocalConfigView.STo.SelectedValue = GetValue;

            //    if (Engine.To != DeFine.GlobalLocalSetting.TargetLanguage)
            //    {
            //        Engine.To = DeFine.GlobalLocalSetting.TargetLanguage;
            //        UPDateUI();
            //        Translator.ClearAICache();
            //    }
            //}
        }

        public void CanEditTransView(bool Check)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TransView.IsHitTestVisible = Check;
            }));
        }

        public void CheckCanClearCache(out bool Check)
        {
            if ((CloudTranslationCache.IsChecked == true || UserTranslationCache.IsChecked == true) == false)
            {
                Check = false;
                ClearCacheR.Opacity = 0.5;
                ClearCacheR.Cursor = null;
            }
            else
            {
                Check = true;
                ClearCacheR.Opacity = 1;
                ClearCacheR.Cursor = Cursors.Hand;
            }
        }

        public Thread ClearCacheTrd = null;

        private void ClearCache_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LoadSaveState == 0)
            {
                MessageBoxExtend.Show(this, "Only currently open files can have their cache cleared.");
                return;
            }

            CheckCanClearCache(out bool Check);

            ClearCacheView.Visibility = Visibility.Visible;
        }

        private void RefreshDictionary_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MessageBoxExtend.Show(this, "Msg", "Are you sure you want to refresh the original text record? Doing so will lose the mod's original text information.", MsgAction.YesNo, MsgType.Info) <= 0)
            {
                return;
            }
            if (ConvertHelper.ObjToStr(RefreshButton.Content).Equals(UILanguageHelper.UICache["RefreshButton1"]))
            {
                return;
            }

            new Thread(() =>
            {
                RefreshButton.Dispatcher.Invoke(new Action(() =>
                {
                    RefreshButton.Content = UILanguageHelper.UICache["RefreshButton1"];
                }));
                var FileUniqueKey = Phoenix.GetFileUniqueKey();

                if (FileUniqueKey > 0)
                {
                    int CallFuncCount = 0;

                    string SetPath = DeFine.GetFullPath(@"\Librarys\" + Phoenix.LastLoadFileName + ".Json");

                    if (File.Exists(SetPath))
                    {
                        File.Delete(SetPath);

                        CallFuncCount++;
                    }

                    YDDictionaryHelper.Dictionarys.Clear();

                    if (TransViewList != null)
                    {
                        TransViewList.QuickRefresh();
                    }

                    MessageBoxExtend.Show(this, "Original source text has been refreshed from the current file.");
                }

                RefreshButton.Dispatcher.Invoke(new Action(() =>
                {
                    RefreshButton.Content = UILanguageHelper.UICache["RefreshButton"];
                }));
            }).Start();
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
        private void CloudTranslationCache_Click(object sender, RoutedEventArgs e)
        {
            if (CloudTranslationCache.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.CanClearCloudTranslationCache = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.CanClearCloudTranslationCache = false;
            }

            CheckCanClearCache(out bool Check);
        }
        private void UserTranslationCache_Click(object sender, RoutedEventArgs e)
        {
            if (UserTranslationCache.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.CanClearUserInputTranslationCache = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.CanClearUserInputTranslationCache = false;
            }

            CheckCanClearCache(out bool Check);
        }
        private void SpeakFromStr(object sender, MouseButtonEventArgs e)
        {
            SpeechHelper.TryPlaySound(FromStr.Text);
        }

        private void SpeakToStr(object sender, MouseButtonEventArgs e)
        {
            SpeechHelper.TryPlaySound(ToStr.Text);
        }


        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            DeFine.GlobalLocalSetting.WritingAreaHeight = WritingArea.Height.Value;
        }

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

        public string LastSetKey = "";
        public void SetSelectFromAndToText(string Key)
        {
            EmptyFromAndToText();

            LastSetKey = Key;

            if (CurrentTransType == 2)
            {
                if (EspReader.Records.ContainsKey(LastSetKey))
                {
                    var GetRecord = EspReader.Records[LastSetKey];
                    SetLog("Select:" + GetRecord.FormID + " | " + GetRecord.ParentSig + " " + GetRecord.ChildSig);
                }
            }
            else
            {
                SetLog("Select:" + LastSetKey);
            }

            if (EspReader.GameCharacters.ContainsKey(Key))
            {
                NpcView.Visibility = Visibility.Visible;
                NpcName.Text = EspReader.GameCharacters[Key][0].Name;
                NpcSex.Content = EspReader.GameCharacters[Key][0].Gender.ToString();
            }
            else
            {
                NpcView.Visibility = Visibility.Collapsed;
            }

            if (Key.Length > 0)
            {
                CurrentKeyBox.Visibility = Visibility.Visible;
            }

            if (TransViewList != null)
            {
                var GridHandle = TransViewList.KeyToFakeGrid(Key);

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


                        FromStr.Text = TransViewList.RealLines[TransViewList.SelectLineID].SourceText;
                        ToStr.Text = TransViewList.RealLines[TransViewList.SelectLineID].TransText;

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

                    DeFine.ExtendWin.SetOriginal(GridHandle.SourceText, EspReader.ToStringsFile.QueryData(GridHandle.Key));
                }
            }
        }

        private bool MainNavIsExpanded = false;

        private void ShowNav(object sender, MouseButtonEventArgs e)
        {
            if (!MainNavIsExpanded)
            {
                ShowMenu(true);
            }
            else
            {
                ShowMenu(false);
            }
        }

        public Languages DetectLanguage()
        {
            LanguageDetector OneDetect = new LanguageDetector();

            for (int i = 0; i < TransViewList?.RealLines.Count; i++)
            {
                if (i > 999)
                {
                    break;
                }
                bool IsCloud = false;
                TransViewList.RealLines[i].SyncData(ref IsCloud);
                P_Language.DetectLanguage(ref OneDetect, TransViewList.RealLines[i].SourceText);
            }

            return OneDetect.GetMaxLang();
        }

        public Languages DetectLang()
        {
            if (TransViewList.RealLines.Count > 0)
            {
                LanguageDetector OneDetect = new LanguageDetector();

                for (int i = 0; i < TransViewList.RealLines.Count; i++)
                {
                    if (i > 2000)
                    {
                        break;
                    }
                    bool IsCloud = false;
                    TransViewList.RealLines[i].SyncData(ref IsCloud);
                    P_Language.DetectLanguage(ref OneDetect, TransViewList.RealLines[i].SourceText);
                }

                return OneDetect.GetMaxLang();
            }
            else
            {
                return Languages.English;
            }
        }
        private void ShowLocalEngineSettingView(object sender, MouseButtonEventArgs e)
        {
            DeFine.LocalConfigView.Owner = this;
            DeFine.LocalConfigView.Show();
            DeFine.LocalConfigView.SetTypes();

            if (TransViewList.RealLines.Count > 0)
            {
                DeFine.LocalConfigView.SFrom.SelectedValue = Phoenix.From.ToString();
            }
            else
            {
                DeFine.LocalConfigView.SFrom.SelectedValue = Languages.English.ToString();
            }

            DeFine.LocalConfigView.STo.SelectedValue = DeFine.GlobalLocalSetting.TargetLanguage.ToString();
        }

        private void ShowView(object sender, MouseButtonEventArgs e)
        {
            ShowView(ConvertHelper.ObjToStr(((Border)sender).Tag));
        }

        public void SetSelectedNav(string View)
        {
            for (int i = 0; i < MainNav.Children.Count; i++)
            {
                if (MainNav.Children[i] is Grid)
                {
                    Border CurrentNav = (Border)((Grid)MainNav.Children[i]).Children[0];
                    string GetName = ConvertHelper.ObjToStr(CurrentNav.Tag);
                    if (View.Equals(GetName))
                    {
                        CurrentNav.Style = (Style)this.FindResource("MenuBlockSelected");
                    }
                    else
                    {
                        CurrentNav.Style = (Style)this.FindResource("MenuBlockStyle");
                    }
                }
            }
        }

        private void StartXTGlowLoop()
        {
            XTGlowLoopStoryboard = (Storyboard)FindResource("XTGlowLoop");
            XTGlowLoopStoryboard.Begin();
        }

        private void StopXTGlowLoop()
        {
            XTGlowLoopStoryboard?.Stop();
        }
        public void ShowView(string View)
        {
            DeFine.CanUpdateChart = false;

            SetSelectedNav(View);

            if (View == "About")
            {
                StartXTGlowLoop();
            }
            else
            {
                StopXTGlowLoop();
            }

            switch (View)
            {
                case "TransHub":
                    {
                        ModTransView.Visibility = Visibility.Visible;
                        AboutView.Visibility = Visibility.Collapsed;
                        SettingView.Visibility = Visibility.Collapsed;
                        DashBoardView.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "DashBoard":
                    {
                        ModTransView.Visibility = Visibility.Collapsed;
                        AboutView.Visibility = Visibility.Collapsed;
                        SettingView.Visibility = Visibility.Collapsed;
                        DashBoardView.Visibility = Visibility.Visible;
                        DeFine.CanUpdateChart = true;
                    }
                    break;
                case "Settings":
                    {
                        ModTransView.Visibility = Visibility.Collapsed;
                        AboutView.Visibility = Visibility.Collapsed;
                        SettingView.Visibility = Visibility.Visible;
                        DashBoardView.Visibility = Visibility.Collapsed;

                        SyncPlatformConfig();
                    }
                    break;
                case "About":
                    {
                        AboutView.Visibility = Visibility.Visible;
                        ModTransView.Visibility = Visibility.Collapsed;
                        SettingView.Visibility = Visibility.Collapsed;
                        DashBoardView.Visibility = Visibility.Collapsed;

                        Modules.Children.Clear();

                        Modules.Children.Add(UIHelper.CreatModuleItem("LexTranslator", DeFine.CurrentVersion));
                        Modules.Children.Add(UIHelper.CreatModuleItem("Translation Engine", Phoenix.Version));
                        Modules.Children.Add(UIHelper.CreatModuleItem("Pex Analysis", PexHeuristicAnalysis.Version));
                        Modules.Children.Add(UIHelper.CreatModuleItem("Esp Reader", EspInterop.Version));
                        Modules.Children.Add(UIHelper.CreatModuleItem("Pex Reader", PexInterop.Version));
                        Modules.Children.Add(UIHelper.CreatModuleItem("DSD Convert", DSDConverter.Version));
                    }
                    break;
            }
        }

        public void SyncPlatformConfig()
        {
            KeyConfigBlocks.Children.Clear();

            List<PlatformConfig> CloudAIs = new List<PlatformConfig>();
            List<PlatformConfig> LocalAIs = new List<PlatformConfig>();
            List<PlatformConfig> TraditionalPlatforms = new List<PlatformConfig>();

            for (int i = 0; i < Phoenix.Config.PlatformConfigs.Count; i++)
            {
                var Key = Phoenix.Config.PlatformConfigs.ElementAt(i).Key;

                if (Phoenix.Config.PlatformConfigs[Key].CustomInFo != null)
                {
                    if (Phoenix.Config.PlatformConfigs[Key].CustomInFo.Type == CustomPlatformType.CloudAI)
                    {
                        CloudAIs.Add(Phoenix.Config.PlatformConfigs[Key]);
                    }
                    else
                    if (Phoenix.Config.PlatformConfigs[Key].CustomInFo.Type == CustomPlatformType.LocalAI)
                    {
                        LocalAIs.Add(Phoenix.Config.PlatformConfigs[Key]);
                    }
                    else
                    if (Phoenix.Config.PlatformConfigs[Key].CustomInFo.Type == CustomPlatformType.Traditional)
                    {
                        TraditionalPlatforms.Add(Phoenix.Config.PlatformConfigs[Key]);
                    }
                }
            }

            for (int i = 0; i < Phoenix.Config.PlatformConfigs.Count; i++)
            {
                var Key = Phoenix.Config.PlatformConfigs.ElementAt(i).Key;
                var GetPlatform = Phoenix.Config.PlatformConfigs[Key];

                if (GetPlatform.Platform == PlatformType.ChatGpt ||
                   GetPlatform.Platform == PlatformType.Gemini ||
                   GetPlatform.Platform == PlatformType.DeepSeek)
                {
                    if (GetPlatform.CustomInFo == null)
                    {
                        switch (GetPlatform.Platform)
                        {
                            case PlatformType.ChatGpt:
                                {
                                    List<string> Models = new List<string>();
                                    Models.Add("gpt-5-nano");
                                    Models.Add("gpt-5-mini");
                                    Models.Add("gpt-4.1-nano");
                                    Models.Add("gpt-4.1-mini");
                                    Models.Add("gpt-4o-mini");

                                    KeyConfigBlocks.Children.Add(DeFine.PlatformConfigStyleWin.GenCloudAIConfig(0, "ChatGpt", "https://platform.openai.com/api-keys", true, GetPlatform.ApiKeys, GetPlatform.Model, CustomPlatformType.CloudAI, Models));
                                }
                                break;
                            case PlatformType.Gemini:
                                {
                                    List<string> Models = new List<string>();
                                    Models.Add("gemini-2.5-flash");
                                    Models.Add("gemini-2.0-flash");

                                    KeyConfigBlocks.Children.Add(DeFine.PlatformConfigStyleWin.GenCloudAIConfig(0, "Gemini", "https://aistudio.google.com/apikey", true, GetPlatform.ApiKeys, GetPlatform.Model, CustomPlatformType.CloudAI, Models));
                                }
                                break;
                            case PlatformType.DeepSeek:
                                {
                                    List<string> Models = new List<string>();
                                    Models.Add("deepseek-chat");
                                    Models.Add("deepseek-reasoner");

                                    KeyConfigBlocks.Children.Add(DeFine.PlatformConfigStyleWin.GenCloudAIConfig(0, "DeepSeek", "https://platform.deepseek.com/api_keys", true, GetPlatform.ApiKeys, GetPlatform.Model, CustomPlatformType.CloudAI, Models));
                                }
                                break;
                        }
                    }
                }
            }

            foreach (var CustomPlatform in CloudAIs)
            {
                KeyConfigBlocks.Children.Add(DeFine.PlatformConfigStyleWin.GenCloudAIConfig(CustomPlatform.CustomInFo.CustomID, CustomPlatform.CustomInFo.Name, string.Empty, false, CustomPlatform.ApiKeys, CustomPlatform.Model, CustomPlatformType.CloudAI, new List<string>()));
            }

            for (int i = 0; i < Phoenix.Config.PlatformConfigs.Count; i++)
            {
                var Key = Phoenix.Config.PlatformConfigs.ElementAt(i).Key;
                var GetPlatform = Phoenix.Config.PlatformConfigs[Key];

                if (GetPlatform.Platform == PlatformType.LMLocalAI)
                {
                    if (GetPlatform.CustomInFo == null)
                    {
                        switch (GetPlatform.Platform)
                        {
                            case PlatformType.LMLocalAI:
                                {
                                    KeyConfigBlocks.Children.Add(DeFine.PlatformConfigStyleWin.GenLocalAIConfig(0, "LM Studio", "https://lmstudio.ai/docs/developer", true, GetPlatform.LocalPort, LMStudio.CurrentModel, CustomPlatformType.LocalAI));
                                }
                                break;
                        }
                    }
                }
            }

            foreach (var CustomPlatform in LocalAIs)
            {
                KeyConfigBlocks.Children.Add(DeFine.PlatformConfigStyleWin.GenLocalAIConfig(CustomPlatform.CustomInFo.CustomID, CustomPlatform.CustomInFo.Name, string.Empty, false, CustomPlatform.LocalPort, CustomPlatform.Model, CustomPlatformType.LocalAI));
            }

            for (int i = 0; i < Phoenix.Config.PlatformConfigs.Count; i++)
            {
                var Key = Phoenix.Config.PlatformConfigs.ElementAt(i).Key;
                var GetPlatform = Phoenix.Config.PlatformConfigs[Key];

                if (GetPlatform.Platform == PlatformType.DeepL)
                {
                    if (GetPlatform.CustomInFo == null)
                    {
                        switch (GetPlatform.Platform)
                        {
                            case PlatformType.DeepL:
                                {
                                    KeyConfigBlocks.Children.Add(DeFine.PlatformConfigStyleWin.GenTraditionalConfig(0, "DeepL", "https://www.deepl.com/your-account/keys", true, GetPlatform.ApiKeys, CustomPlatformType.Traditional));
                                }
                                break;
                        }
                    }
                }
            }

            foreach (var CustomPlatform in TraditionalPlatforms)
            {
                KeyConfigBlocks.Children.Add(DeFine.PlatformConfigStyleWin.GenTraditionalConfig(CustomPlatform.CustomInFo.CustomID, CustomPlatform.CustomInFo.Name, string.Empty, false, CustomPlatform.ApiKeys, CustomPlatformType.Traditional));
            }
        }
        public void SyncTransStateUI()
        {
            TStop.Opacity = 0.5;

            if (TranslatorInterface.TranslationStatus == StateControl.Run)
            {
                TRun.Visibility = Visibility.Collapsed;
                TStop.Visibility = Visibility.Visible;
                TCancel.Visibility = Visibility.Visible;

                ThreadInFo.Visibility = Visibility.Visible;
            }
            else
            if (TranslatorInterface.TranslationStatus == StateControl.Stop)
            {
                TStop.Opacity = 1;
                TRun.Visibility = Visibility.Collapsed;
                TStop.Visibility = Visibility.Visible;
                TCancel.Visibility = Visibility.Visible;

                ThreadInFo.Visibility = Visibility.Visible;
            }
            else
            if (TranslatorInterface.TranslationStatus == StateControl.Cancel || TranslatorInterface.TranslationStatus == StateControl.Null)
            {
                TRun.Visibility = Visibility.Visible;
                TStop.Visibility = Visibility.Collapsed;
                TCancel.Visibility = Visibility.Collapsed;

                ThreadInFo.Visibility = Visibility.Collapsed;
            }

            if (TranslatorInterface.TranslationStatus == StateControl.Run || TranslatorInterface.TranslationStatus == StateControl.Stop)
            {
                DeFine.LocalConfigView.SFrom.IsEnabled = false;
                DeFine.LocalConfigView.STo.IsEnabled = false;
            }
            else
            {
                DeFine.LocalConfigView.SFrom.IsEnabled = true;
                DeFine.LocalConfigView.STo.IsEnabled = true;
            }
        }

        private void ChangeTransState(object sender, MouseButtonEventArgs e)
        {
            bool IsKeep = false;
            bool CallSucess = false;
            if (TransViewList != null)
            {
                if (TransViewList.Rows > 0)
                {
                    if (sender is Border)
                    {
                        Border ButtonHandle = (Border)sender;

                        string GetButtonName = ButtonHandle.Name;

                        switch (GetButtonName)
                        {
                            case "TRun":
                                {
                                    if (ConvertHelper.ObjToStr(TransProcess.Content).StartsWith("STRINGS("))
                                    {
                                        if (Phoenix.From == Phoenix.To)
                                        {
                                            MessageBoxExtend.Show(this, "The source language and target language cannot be the same!");
                                            CallSucess = false;

                                            ShowLocalEngineSettingView(null, null);
                                            return;
                                        }

                                        if (!Phoenix.CheckAvailableNodes())
                                        {
                                            MessageBoxExtend.Show(this, "Please enable at least one translation platform node.");
                                            CallSucess = false;

                                            if (!IsExpanded)
                                            {
                                                ShowLeftMenu(TRun, null);
                                            }
                                            return;
                                        }

                                        if (Phoenix.Config.GetPlatformData(LMStudio.Type).Enable)
                                        {
                                            LMStudio.CurrentModel = string.Empty;
                                        }

                                        TRun.Visibility = Visibility.Collapsed;

                                        TranslatorInterface.TranslationStatus = StateControl.Run;
                                        CallSucess = true;
                                        IsKeep = false;
                                    }
                                }
                                break;
                            case "TStop":
                                {
                                    if (TStop.Opacity == 0.5)
                                    {
                                        TranslatorInterface.TranslationStatus = StateControl.Stop;
                                        CallSucess = true;
                                    }
                                    else
                                    {
                                        if (TranslatorInterface.TranslationStatus == StateControl.Stop)
                                        {
                                            IsKeep = true;
                                        }

                                        TranslatorInterface.TranslationStatus = StateControl.Run;
                                        CallSucess = true;
                                    }
                                }
                                break;
                            case "TCancel":
                                {
                                    TranslatorInterface.TranslationStatus = StateControl.Cancel;
                                    CallSucess = true;
                                }
                                break;
                        }

                        if (CallSucess)
                        {
                            TranslatorInterface.SyncTransState(new Action(() =>
                            {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    SyncTransStateUI();
                                }));
                            }), IsKeep);
                        }
                    }
                }
            }
            if (!CallSucess)
            {
                MessageBoxExtend.Show(this, "Batch translation is not possible at the current state.\nPlease wait until the file loading is finished.");
            }
        }

        private void ChangeColor(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border ButtonHandle = (Border)sender;
                Color GetColor = ((SolidColorBrush)ButtonHandle.Background).Color;

                if (TransViewList != null)
                {
                    TransViewList.ChangeFontColor(Phoenix.GetFileUniqueKey(), GetColor.R, GetColor.G, GetColor.B);
                }
            }
        }

        private void CancelTranslatedText(object sender, MouseButtonEventArgs e)
        {
            EmptyFromAndToText();

            UIHelper.ShowButton(CancelOTButton, false);
            UIHelper.ShowButton(ApplyOTButton, false);

            ShowClearToStrButton(false);
        }

        public void ApplyTranslatedText()
        {
            if (TransViewList != null)
            {
                if (LastSetKey.Trim().Length > 0)
                {
                    var GetGrid = TransViewList.KeyToFakeGrid(LastSetKey);

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
                            if (CloudDBCache.FindCache(Phoenix.GetFileUniqueKey(), GetGrid.Key, Phoenix.To).Equals(GetGrid.TransText))
                            {
                                LocalDBCache.DeleteCache(Phoenix.GetFileUniqueKey(), GetGrid.Key, Phoenix.To);

                                var Link = TranslatorInterface.Instance.GetLink();

                                Link[GetGrid.Key] = GetGrid.TransText;
                            }
                            else
                            {
                                TranslatorInterface.Instance.AutoSetLink(GetGrid.Key, GetGrid.SourceText, GetGrid.TransText);
                            }
                        }
                        catch { }

                        TranslatorInterface.SetTranslatorHistoryCache(GetGrid.Key, GetGrid.TransText, false);

                        GetGrid.SyncData(ref RefCloud);
                        GetGrid.SyncUI(TransViewList);
                        //DeFine.ExtendWin.SetOriginal(GetGrid.SourceText, DeFine.WorkingWin.GlobalEspReader.StringsReader.QueryData(GetGrid.Key));
                    }

                    UIHelper.ShowButton(ApplyOTButton, false);
                }
            }
        }
        private void ApplyTranslatedText(object sender, MouseButtonEventArgs e)
        {
            ApplyTranslatedText();
        }
        private void ProcessBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ScanAnimator != null)
            {
                if (TranslatorInterface.Instance != null)
                {
                    var GetBatchCore = TranslatorInterface.Instance.GetBatchCore();
                    if(GetBatchCore!=null)
                    if (GetBatchCore.IsWork && !GetBatchCore.IsStop)
                    {
                        ScanAnimator.UpdateAnimationTarget();
                    }
                }
            }
        }

        private void UPSelecter_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TransViewList?.UP();
        }

        private void DownSelecter_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TransViewList?.Down();
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

        public void ShowClearToStrButton(bool Enable)
        {
            UIHelper.ShowButton(ClearToStrButton, Enable);
        }


        private void ToStr_TextChanged(object sender, EventArgs e)
        {
            if (ToStr.Text.Length > 0)
            {
                ShowClearToStrButton(true);
                UIHelper.ShowButton(ApplyOTButton, true);
            }
            else
            {
                ShowClearToStrButton(false);
            }

            UIHelper.ShowButton(ApplyOTButton, true);
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


        private void ToStr_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        public void AutoSizeHistoryList()
        {
            if (HistoryLayer.Visibility == Visibility.Visible)
            {
                ChangeTimeCol.Width = 150;
                double Width = HistoryLayer.ActualWidth - 150;
                if (Width < 0) Width = 300;
                TranslatedCol.Width = Width;
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

        private void ShowHistorys(object sender, MouseButtonEventArgs e)
        {
            if (HistoryLayer.Visibility == Visibility.Collapsed)
            {
                HistoryLayer.Visibility = Visibility.Visible;

                AutoSizeHistoryList();
                HistoryButtonFont.Content = UILanguageHelper.UICache["HistoryButtonFont1"];

                AutoLoadHistoryList();
            }
            else
            {
                HistoryLayer.Visibility = Visibility.Collapsed;
                HistoryButtonFont.Content = UILanguageHelper.UICache["HistoryButtonFont"];
            }
        }

        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var GetItem in HistoryList.SelectedItems)
            {
                var GetCol = HistoryList.SelectedItem.GetType().GetProperty("Translated");
                if (GetCol != null)
                {
                    string Translated = ConvertHelper.ObjToStr(ConvertHelper.ObjToStr(GetCol.GetValue(GetItem, null)));
                    ToStr.Text = Translated;
                }
            }
        }

        private void OpenUrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string GetUrl = "";
            string GetTag = "";

            if (sender is Label)
            {
                GetUrl = "";
                GetTag = ConvertHelper.ObjToStr(((Label)sender).Tag);

                if (GetTag.Length > 0)
                {
                    GetUrl = GetTag;
                }
                else
                {
                    GetUrl = ConvertHelper.ObjToStr(((Label)sender).Content);
                }
            }
            if (sender is Run)
            {
                GetUrl = "";
                GetTag = ConvertHelper.ObjToStr(((Run)sender).Tag);

                if (GetTag.Length > 0)
                {
                    GetUrl = GetTag;
                }
                else
                {
                    GetUrl = ConvertHelper.ObjToStr(((Run)sender).Text);
                }
            }

            if (GetUrl.Length > 0)
            {
                if (MessageBoxExtend.Show(this, "Prompt", "Do you want to open your default browser and visit\n " + GetUrl + "\n?", MsgAction.YesNo, MsgType.Info) > 0)
                {
                    ExplorerHelper.OpenUrl(GetUrl);
                }
            }
        }

        public void BookTransCallBack(string Key, string CurrentText)
        {
            if (Key.Equals(LastSetKey) && CurrentText.Length > 0)
            {
                ToStr.Dispatcher.Invoke(new Action(() =>
                {
                    ToStr.Text = CurrentText;
                }));
            }
        }

        public object TranslateLocker = new object();
        public Thread TranslateTrd = null;

        public bool SingleTrans = false;
        public void TranslateCurrent()
        {
            TranslatorInterface.MakeReady();

            lock (TranslateLocker)
            {
                if (TransViewList != null)
                {
                    if (ConvertHelper.ObjToStr(TranslateOTButtonFont.Content).Equals(UILanguageHelper.UICache["TranslateOTButtonFont"]))
                    {
                        FakeGrid QueryGrid = TransViewList.KeyToFakeGrid(LastSetKey);

                        if (QueryGrid != null && TranslateTrd == null)
                        {
                            bool IsCloud = false;
                            QueryGrid.SyncData(ref IsCloud);

                            if (QueryGrid.TransText.Length > 0)
                            {
                                CloudDBCache.DeleteCache(Phoenix.GetFileUniqueKey(), QueryGrid.Key, Phoenix.To);
                            }

                            BaseUnit SetUnit = new BaseUnit(Phoenix.GetFileUniqueKey(), QueryGrid.Key, QueryGrid.Type, QueryGrid.SourceText, QueryGrid.TransText, 100);

                            bool CanSleep = false;

                            CanEditTransView(false);

                            TranslateTrd = new Thread(() =>
                            {
                                SingleTrans = true;

                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    TranslateOTButtonFont.Content = UILanguageHelper.UICache["TranslateOTButtonFont1"];
                                    ThreadInFo.Visibility = Visibility.Visible;
                                }));


                                string GetTranslated = "";

                                UnitGroup Result = TranslatorInterface.Instance.Translate(SetUnit, false);

                                GetTranslated = Result.GetFrist().Translated;

                                CanEditTransView(true);

                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    TranslateOTButtonFont.Content = UILanguageHelper.UICache["TranslateOTButtonFont"];

                                    if (TranslatorInterface.TranslationStatus == StateControl.Null || TranslatorInterface.TranslationStatus == StateControl.Cancel)
                                    {
                                        ThreadInFo.Visibility = Visibility.Collapsed;
                                    }

                                    ToStr.Text = GetTranslated;
                                }));

                                SingleTrans = false;
                                TranslateTrd = null;
                            });

                            TranslateTrd.Start();
                        }
                    }
                }
            }
        }
        private void TranslateOTButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TranslateCurrent();
        }

        public void UPDateFile(bool CanSetSource)
        {
            if (TransViewList != null)
            {
                for (int i = 0; i < TransViewList.Rows; i++)
                {
                    bool IsCloud = false;

                    TransViewList.RealLines[i].SyncData(ref IsCloud);

                    string GetKey = TransViewList.RealLines[i].Key;

                    string GetTransText = TransViewList.RealLines[i].TransText;

                    if (CanSetSource)
                    {
                        if (string.IsNullOrEmpty(GetTransText))
                        {
                            GetTransText = TransViewList.RealLines[i].SourceText;
                        }
                    }

                    var Link = TranslatorInterface.Instance.GetLink();
                    Link[GetKey] = GetTransText;
                }
            }
        }

        private void ImportRamCache_Click(object sender, RoutedEventArgs e)
        {
            var Dialog = new System.Windows.Forms.OpenFileDialog();
            Dialog.Title = "Please select a file";
            Dialog.Filter = "All files|*.*";
            Dialog.Multiselect = false;

            if (Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string SelectedFile = Dialog.FileName;

                if (File.Exists(SelectedFile))
                {
                    if (TransViewList != null)
                    {
                        if (TransViewList.Rows > 0)
                        {
                            string GetRamCache = Encoding.UTF8.GetString(DataHelper.ReadFile(SelectedFile));
                            List<FakeGrid> RealLines = JsonConvert.DeserializeObject<List<FakeGrid>>(GetRamCache);

                            if (RealLines != null)
                            {
                                for (int i = 0; i < RealLines.Count; i++)
                                {
                                    if (RealLines[i].SourceText != RealLines[i].TransText)
                                    {
                                       TranslatorInterface.Instance.SetLink(RealLines[i].Key, RealLines[i].TransText);
                                    }
                                }

                                for (int i = 0; i < TransViewList.Rows; i++)
                                {
                                    bool IsCloud = false;
                                    TransViewList.RealLines[i].SyncData(ref IsCloud);
                                    TransViewList.RealLines[i].SyncUI(TransViewList);
                                }
                            }
                        }
                        else
                        {
                            LoadAny(SelectedFile);
                        }
                    }
                }
            }
        }

        private void ExportToRamCache_Click(object sender, RoutedEventArgs e)
        {
            if (TransViewList != null)
            {
                if (TransViewList.Rows > 0)
                {
                    var GetWritePath = DataHelper.ShowSaveFileDialog(LModName + "_C.Json", "RamCache (*.Json)|*.Json");

                    UPDateFile(false);

                    string GetJson = JsonConvert.SerializeObject(TransViewList.RealLines, Formatting.Indented);

                    if (GetWritePath != null)
                    {
                        if (GetWritePath.Trim().Length > 0)
                        {
                            if (File.Exists(GetWritePath))
                            {
                                File.Delete(GetWritePath);
                            }
                            DataHelper.WriteFile(GetWritePath, Encoding.UTF8.GetBytes(GetJson));
                        }
                    }
                }
            }
        }
        private void ExportToDsd_Click(object sender, RoutedEventArgs e)
        {
            if (TransViewList != null)
            {
                if (CurrentTransType == 2 && TransViewList.Rows > 0)
                {
                    if (EspReader.Records != null)
                    {
                        var GetWritePath = DataHelper.ShowSaveFileDialog(LModName + ".json", "DSD (*.json)|*.json");

                        var DSDFile = DSDConverter.RecordsToDSDFile();
                        if (DSDFile != null)
                        {
                            if (DSDFile.DSDItems.Count > 0)
                            {
                                List<DSDItem> DSDItems = new List<DSDItem>();
                                DSDItems = DSDFile.DSDItems;
                                string GetJson = JsonConvert.SerializeObject(DSDItems, Formatting.Indented);

                                if (File.Exists(GetWritePath))
                                {
                                    File.Delete(GetWritePath);
                                }
                                DataHelper.WriteFile(GetWritePath, Encoding.UTF8.GetBytes(GetJson));
                            }
                        }
                    }
                }
                else
                {
                    MessageBoxExtend.Show(this, "The current file does not support exporting to DSD format.");
                }
            }
        }

        private void UILanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetValue = ConvertHelper.ObjToStr(UILanguages.SelectedValue);
            if (GetValue.Length > 0)
            {
                DeFine.GlobalLocalSetting.CurrentUILanguage = (Languages)Enum.Parse(typeof(Languages), GetValue);
                UILanguageHelper.ChangeLanguage(DeFine.GlobalLocalSetting.CurrentUILanguage);
            }
        }

        private void ChangeTechDarkBlue(object sender, MouseButtonEventArgs e)
        {
            DeFine.GlobalLocalSetting.Style = 1;
            DeFine.GlobalLocalSetting.SaveConfig();

            MessageBoxExtend.Show(this, "The theme is set successfully and will take effect after restarting the software.");
        }

        private void ChangePurpleStyle(object sender, MouseButtonEventArgs e)
        {
            DeFine.GlobalLocalSetting.Style = 2;
            DeFine.GlobalLocalSetting.SaveConfig();

            MessageBoxExtend.Show(this, "The theme is set successfully and will take effect after restarting the software.");
        }

        private void AutoUpdateStringsFileToDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (AutoUpdateStringsFileToDatabase.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.AutoUpdateStringsFileToDatabase = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.AutoUpdateStringsFileToDatabase = false;
            }

            DeFine.GlobalLocalSetting.SaveConfig();
        }
        private void ForceTranslationConsistency_Click(object sender, RoutedEventArgs e)
        {
            if (ForceTranslationConsistency.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.ForceTranslationConsistency = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.ForceTranslationConsistency = false;
            }

            DeFine.GlobalLocalSetting.SaveConfig();
        }
        private void EnableAnalyzingWords_Click(object sender, RoutedEventArgs e)
        {
            if (EnableAnalyzingWords.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.EnableAnalyzingWords = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.EnableAnalyzingWords = false;
            }

            DeFine.GlobalLocalSetting.SaveConfig();
        }
        private void EnableGlobalSearch_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalSearch.IsChecked == true)
            {
                Phoenix.Config.EnableGlobalSearch = true;
            }
            else
            {
                Phoenix.Config.EnableGlobalSearch = false;
            }

            Phoenix.SaveConfig();
        }

        private void NextAuto_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            NextAuto();
        }

        private string LastUntranslatedKey = null;
        public void NextAuto()
        {
            var Lines = TransViewList?.RealLines;
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
                    var SourceLang = P_Language.DetectLanguageByLine(GetLine.SourceText);
                    if (SourceLang != Phoenix.To)
                    {
                        if (GetLine.TransText.Length == 0 ||
                            SourceLang == P_Language.DetectLanguageByLine(GetLine.TransText))
                        {
                            if (Lines[i].Score > 0)
                            {
                                LastUntranslatedKey = Lines[i].Key;
                                TransViewList.Goto(Lines[i].Key);
                                return;
                            }
                        }
                    }
                }
            }

            LastUntranslatedKey = null;
        }

       

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                QuickSearch();
            }
        }

        private void SEnableLanguageDetect_Click(object sender, RoutedEventArgs e)
        {
            if (SEnableLanguageDetect.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.EnableLanguageDetect = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.EnableLanguageDetect = false;
            }
        }

        private void TestAll_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //var Get = ChineseVariantMap.SimplifiedToTraditionalByReq("测试转换的一段话");
            //MessageBox.Show(Get);

            //new R_XmlReader().Load("C:\\Users\\52508\\Desktop\\Delilah Dress_english_chinese.xml");

            for (int i = 0; i < TransViewList.RealLines.Count; i++)
            {
                TransViewList.RealLines[i].TransText = TransViewList.RealLines[i].SourceText + "(" + i.ToString() + ")";

                var Link = TranslatorInterface.Instance.GetLink();
                Link[TransViewList.RealLines[i].Key] = TransViewList.RealLines[i].TransText;

                TransViewList.RealLines[i].SyncUI(TransViewList);
            }
        }

        private void ClearCacheViewClose_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ClearCacheView.Visibility = Visibility.Collapsed;
        }

        private void ClearCacheR_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckCanClearCache(out bool Check);
            if (Check)
            {
                if (MessageBoxExtend.Show(this, "Waring", "Are you sure you want to clear the database records? Doing so will lose all translated content. (Note: Under no circumstances should you click this button arbitrarily.)", MsgAction.YesNo, MsgType.Waring) <= 0)
                {
                    return;
                }

                if (TransViewList != null)
                {
                    if (TransViewList.Rows > 0)
                    {
                        if (ConvertHelper.ObjToStr(ClearCacheButton.Content).Equals(UILanguageHelper.UICache["ClearCacheButton"]))
                        {
                            if (ClearCacheTrd == null)
                            {
                                bool? GetCloudTranslationCache = CloudTranslationCache.IsChecked;
                                bool? GetUserTranslationCache = UserTranslationCache.IsChecked;

                                ClearCacheTrd = new Thread(() =>
                                {
                                    try
                                    {
                                        ClearCacheButton.Dispatcher.Invoke(new Action(() =>
                                        {
                                            ClearCacheButton.Content = UILanguageHelper.UICache["ClearCacheButton1"];
                                        }));

                                        int CallFuncCount = 0;
                                        if (GetCloudTranslationCache == true)
                                        {
                                            TranslatorInterface.Instance.ClearAICache();

                                            if (CloudDBCache.ClearCloudCache(Phoenix.GetFileUniqueKey()))
                                            {
                                                var GetBatchCore = TranslatorInterface.Instance.GetBatchCore();
                                                if (GetBatchCore != null)
                                                {
                                                    GetBatchCore.TranslatedCount = 0;
                                                }
                                                Phoenix.Vacuum();
                                                CallFuncCount++;
                                            }
                                        }
                                        if (GetUserTranslationCache == true)
                                        {
                                            LocalDBCache.ClearLocalCache(Phoenix.GetFileUniqueKey());
                                            {
                                                Phoenix.Vacuum();
                                                CallFuncCount++;
                                            }

                                            ToStr.Dispatcher.Invoke(new Action(() =>
                                            {
                                                ToStr.Text = "";
                                            }));
                                        }

                                        UPDateUI();
                                    }
                                    catch
                                    {

                                    }

                                    TranslatorInterface.Close();
                                    TranslatorInterface.PreparingTranslationUnits();

                                    while (TranslatorInterface.PreparingTrd != null)
                                    {
                                        Thread.Sleep(100);
                                    }

                                    ClearCacheButton.Dispatcher.Invoke(new Action(() =>
                                    {
                                        ClearCacheButton.Content = UILanguageHelper.UICache["ClearCacheButton"];
                                    }));

                                    ClearCacheTrd = null;
                                });

                                ClearCacheTrd.Start();
                            }
                        }
                    }
                }
            }
        }

        //Control the speed to a fixed 2000 px/s
        private const double ExpandAnimationSpeed = 2000;
        public void SyncAnimation()
        {
            double AutoHeight = CalcLeftMenuHeight();

            var ExpandMenu = (Storyboard)FindResource("ExpandMenu");
            var ExpandAnimation = (DoubleAnimation)ExpandMenu.Children[0];
            ExpandAnimation.To = AutoHeight;
            ExpandAnimation.Duration = TimeSpan.FromSeconds(Math.Abs(0 - AutoHeight) / ExpandAnimationSpeed);

            var CollapseMenu = (Storyboard)FindResource("CollapseMenu");
            var CollapseAnimation = (DoubleAnimation)CollapseMenu.Children[0];
            CollapseAnimation.From = AutoHeight;
            CollapseAnimation.Duration = TimeSpan.FromSeconds(Math.Abs(AutoHeight - 0) / ExpandAnimationSpeed);

            CollapseAnimation.Completed += (_, __) =>
            {
                LeftMenu.Visibility = Visibility.Collapsed;
                LeftMenu.BeginAnimation(HeightProperty, null);
            };
        }


        private double CalcLeftMenuHeight()
        {
            double AutoHeight = 0;
            foreach (FrameworkElement GetRow in Nodes.Children)
            {
                AutoHeight += GetRow.ActualHeight + 1;
            }
            return AutoHeight;
        }


        #region Setting

        public void SelectFristSettingNav()
        {
            if (SettingNavs.Children.Count > 0)
            {
                if (SettingNavs.Children[0] is Border)
                    SelectSettingNav((Border)SettingNavs.Children[0]);
            }
        }

        public Border LastSetBorder = null;
        public void SetSelectSettingNav(Border Nav)
        {
            if (Nav.Child is Grid)
            {
                Grid GetMainGrid = (Grid)Nav.Child;
                if (GetMainGrid.Children.Count == 2)
                {
                    Nav.Style = (Style)this.FindResource("ModelSelected");
                    ((Grid)GetMainGrid.Children[1]).Visibility = Visibility.Visible;
                }
            }

        }

        public void SetUnSelectSettingNav(Border Nav)
        {
            if (Nav.Child is Grid)
            {
                Grid GetMainGrid = (Grid)Nav.Child;
                if (GetMainGrid.Children.Count == 2)
                {
                    Nav.Style = (Style)this.FindResource("ModelUnSelected");
                    ((Grid)GetMainGrid.Children[1]).Visibility = Visibility.Hidden;
                }
            }

        }

        public string GetSettingNavName(Border Nav)
        {
            if (Nav.Child is Grid)
            {
                Grid GetMainGrid = (Grid)Nav.Child;
                if (GetMainGrid.Children.Count == 2)
                {
                    if (GetMainGrid.Children[0] is Label)
                        return ConvertHelper.ObjToStr(((Label)GetMainGrid.Children[0]).Content);
                }
            }
            return string.Empty;
        }

        public void SelectSettingNav(Border Nav)
        {
            if (LastSetBorder != null)
            {
                SetUnSelectSettingNav(LastSetBorder);
            }

            SetSelectSettingNav(Nav);

            string GetName = GetSettingNavName(Nav);
            ShowFrame(GetName);
            SyncSettingUI(GetName);

            LastSetBorder = Nav;
        }

        public void SyncSettingUI(string Name)
        {
            if (Name.Equals("Request And ApiKey Configs"))
            {
                var PhoenixConfig = Phoenix.Config;

                SProxyUrl.Text = Phoenix.Config.ProxyUrl;
                SProxyUserName.Text = Phoenix.Config.ProxyUserName;
                SProxyPassword.Text = Phoenix.Config.ProxyPassword;

                SyncPlatformConfig();
            }
            else
            if (Name.Equals("AI Configs"))
            {
                SContextLimit.Text = Phoenix.Config.ContextLimit.ToString();

                if (Phoenix.Config.ContextEnable)
                {
                    SContextEnable.IsChecked = true;
                }
                else
                {
                    SContextEnable.IsChecked = false;
                }

                SAIKeyword.Text = Phoenix.Config.UserCustomAIPrompt;
            }
            else
            if (Name.Equals("Game Configs"))
            {
                SGame.Items.Clear();
                SGame.Items.Add(GameNames.Skyrim.ToString());

                SGame.SelectedValue = DeFine.GlobalLocalSetting.GameType.ToString();

                if (DeFine.GlobalLocalSetting.ShowAssembly)
                {
                    SShowAssembly.IsChecked = true;
                }
                else
                {
                    SShowAssembly.IsChecked = false;
                }

                SCodeGenStyle.Items.Clear();

                SCodeGenStyle.Items.Add("CSharp");
                SCodeGenStyle.Items.Add("Papyrus");

                if (DeFine.GlobalLocalSetting.GenCSharp)
                {
                    SCodeGenStyle.SelectedValue = SCodeGenStyle.Items[0];
                }
                else
                {
                    SCodeGenStyle.SelectedValue = SCodeGenStyle.Items[1];
                }


                //SGameFileEncoding.Items.Clear();
                //SGameFileEncoding.Items.Add(EncodingTypes.UTF8.ToString());
                //SGameFileEncoding.Items.Add(EncodingTypes.UTF8_1250.ToString());
                //SGameFileEncoding.Items.Add(EncodingTypes.UTF8_1252.ToString());
                //SGameFileEncoding.Items.Add(EncodingTypes.UTF8_1253.ToString());
                //SGameFileEncoding.Items.Add(EncodingTypes.UTF8_1256.ToString());

                //SGameFileEncoding.SelectedValue = DeFine.GlobalLocalSetting.FileEncoding.ToString();
            }
            else
            if (Name.Equals("UI Configs"))
            {
                if (DeFine.GlobalLocalSetting.ShowCode)
                {
                    ShowCodeView.IsChecked = true;
                }
                else
                {
                    ShowCodeView.IsChecked = false;
                }

                if (DeFine.GlobalLocalSetting.TextDisplay == TextLayout.RTL)
                {
                    RTLEnable.IsChecked = true;
                }
                else
                {
                    RTLEnable.IsChecked = false;
                }
            }
            else
            if (Name.Equals("Engine Configs"))
            {
                SThrottlingRatio.Text = Phoenix.Config.ThrottleRatio.ToString();

                SRotationDelay.Text = Phoenix.Config.ThrottleDelayMs.ToString();

                SMaxThread.Text = Phoenix.Config.MaxThreadCount.ToString();

                if (DeFine.GlobalLocalSetting.AutoUpdateStringsFileToDatabase)
                {
                    AutoUpdateStringsFileToDatabase.IsChecked = true;
                }
                else
                {
                    AutoUpdateStringsFileToDatabase.IsChecked = false;
                }

                if (DeFine.GlobalLocalSetting.ForceTranslationConsistency)
                {
                    ForceTranslationConsistency.IsChecked = true;
                }
                else
                {
                    ForceTranslationConsistency.IsChecked = false;
                }

                if (DeFine.GlobalLocalSetting.EnableAnalyzingWords)
                {
                    EnableAnalyzingWords.IsChecked = true;
                }
                else
                {
                    EnableAnalyzingWords.IsChecked = false;
                }

                if (Phoenix.Config.EnableGlobalSearch)
                {
                    GlobalSearch.IsChecked = true;
                }
                else
                {
                    GlobalSearch.IsChecked = false;
                }

                if (DeFine.GlobalLocalSetting.EnableLanguageDetect)
                {
                    SEnableLanguageDetect.IsChecked = true;
                }
                else
                {
                    SEnableLanguageDetect.IsChecked = false;
                }

                P_Placeholders.Text = DeFine.GlobalLocalSetting.P_Placeholders;

                if (DeFine.GlobalLocalSetting.CanTranslateBook)
                {
                    CanTranslateBook.IsChecked = true;
                }
                else
                {
                    CanTranslateBook.IsChecked = false;
                }

                if (DeFine.GlobalLocalSetting.UseFullPunctuation)
                {
                    UseFullPunctuation.IsChecked = true;
                }
                else
                {
                    UseFullPunctuation.IsChecked = false;
                }
            }
        }

        private void P_Placeholders_TextChanged(object sender, TextChangedEventArgs e)
        {
            DeFine.GlobalLocalSetting.P_Placeholders = P_Placeholders.Text;
            DeFine.GlobalLocalSetting.SaveConfig();
        }
        private void ShowCodeView_Click(object sender, RoutedEventArgs e)
        {
            if (ShowCodeView.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.ShowCode = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.ShowCode = false;
            }
        }

        public void ShowFrame(string Name)
        {
            for (int i = 0; i < this.SettingFrames.Children.Count; i++)
            {
                if (this.SettingFrames.Children[i] is Border)
                {
                    Border GetFrame = (Border)this.SettingFrames.Children[i];
                    string GetTag = ConvertHelper.ObjToStr(GetFrame.Tag);
                    if (GetTag.Equals(Name))
                    {
                        GetFrame.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        GetFrame.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void SelectSettingNav(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border GetNav = (Border)sender;

                SelectSettingNav(GetNav);
            }
        }

        private void SCodeGenStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var GetValue = ConvertHelper.ObjToStr(SCodeGenStyle.SelectedValue);
            if(GetValue.Length>0)
            if (GetValue.Equals("CSharp"))
            {
                DeFine.GlobalLocalSetting.GenCSharp = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.GenCSharp = false;
            }
        }

        private void SShowAssembly_Click(object sender, RoutedEventArgs e)
        {
            if (SShowAssembly.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.ShowAssembly = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.ShowAssembly = false;
            }

            DeFine.GlobalLocalSetting.SaveConfig();
        }

        private void SProxyUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.ProxyUrl = SProxyUrl.Text;
        }
        private void SProxyUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.ProxyUserName = SProxyUserName.Text;
        }
        private void SProxyPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.ProxyPassword = SProxyPassword.Text;
        }

        private void SContextLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.ContextLimit = ConvertHelper.ObjToInt(SContextLimit.Text);
        }

        private void SAIKeyword_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.UserCustomAIPrompt = SAIKeyword.Text.Trim();
        }

        private void SContextEnable_Click(object sender, RoutedEventArgs e)
        {
            if (SContextEnable.IsChecked == true)
            {
                Phoenix.Config.ContextEnable = true;

                if (DeFine.NodeStyleWin.ContextCheckBox != null)
                {
                    DeFine.NodeStyleWin.ContextCheckBox.IsChecked = true;
                }
                //ContextGeneration.IsChecked = true;
                //RightContextIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                Phoenix.Config.ContextEnable = false;

                if (DeFine.NodeStyleWin.ContextCheckBox != null)
                {
                    DeFine.NodeStyleWin.ContextCheckBox.IsChecked = false;
                }
                //ContextGeneration.IsChecked = false;
                //RightContextIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetName = ConvertHelper.ObjToStr(SGame.SelectedValue);
            if (GetName.Trim().Length > 0)
            {
                DeFine.GlobalLocalSetting.GameType = (GameNames)Enum.Parse(typeof(GameNames), GetName);
            }
        }

        public void SaveApiKey(PlatformType Type, string KeysStr)
        {
            for (int i = 0; i < Phoenix.Config.PlatformConfigs.Count; i++)
            {
                int GetKey = Phoenix.Config.PlatformConfigs.ElementAt(i).Key;

                if (Phoenix.Config.PlatformConfigs[GetKey].Platform == Type)
                {
                    Phoenix.Config.PlatformConfigs[GetKey].ApiKeys = Phoenix.Config.KeysStrToArray(KeysStr);
                    break;
                }
            }

            Phoenix.SaveConfig();
        }
        private void SThrottlingRatio_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.ThrottleRatio = ConvertHelper.ObjToDouble(SThrottlingRatio.Text);
        }

        private void SRotationDelay_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.ThrottleDelayMs = ConvertHelper.ObjToInt(SRotationDelay.Text);
        }

        private void SMaxThread_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.MaxThreadCount = ConvertHelper.ObjToInt(SMaxThread.Text);
        }

        private void RTLEnable_Click(object sender, RoutedEventArgs e)
        {
            if (RTLEnable.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.TextDisplay = TextLayout.RTL;
            }
            else
            {
                DeFine.GlobalLocalSetting.TextDisplay = TextLayout.LTR;
            }

            UIHelper.SyncAvalonEditTextLayout();
        }

        private void CanTranslateBook_Click(object sender, RoutedEventArgs e)
        {
            if (CanTranslateBook.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.CanTranslateBook = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.CanTranslateBook = false;
            }
        }

        private void UseFullPunctuation_Click(object sender, RoutedEventArgs e)
        {
            if (UseFullPunctuation.IsChecked == true)
            {
                DeFine.GlobalLocalSetting.UseFullPunctuation = true;
            }
            else
            {
                DeFine.GlobalLocalSetting.UseFullPunctuation = false;
            }
        }

        #endregion

        #region LogView

        private Border LastSetLogButton = null;
        private void SelectLogNav(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border GetBorderHandle = (Border)sender;

                if (GetBorderHandle.Child is Label)
                {
                    if (LastSetLogButton != null)
                    {
                        LastSetLogButton.Style = (Style)this.FindResource("LogViewButtonUnSelected");
                    }

                    string GetContent = ConvertHelper.ObjToStr(((Label)GetBorderHandle.Child).Content);

                    if (GetContent == "InputLog")
                    {
                        InputLog.Visibility = Visibility.Visible;
                        OutputLog.Visibility = Visibility.Collapsed;
                        MainLog.Visibility = Visibility.Collapsed;
                    }
                    else
                    if (GetContent == "OutputLog")
                    {
                        InputLog.Visibility = Visibility.Collapsed;
                        OutputLog.Visibility = Visibility.Visible;
                        MainLog.Visibility = Visibility.Collapsed;
                    }
                    else
                    if (GetContent == "Log")
                    {
                        InputLog.Visibility = Visibility.Collapsed;
                        OutputLog.Visibility = Visibility.Collapsed;
                        MainLog.Visibility = Visibility.Visible;
                    }

                    GetBorderHandle.Style = (Style)this.FindResource("LogViewButtonSelected");

                    LastSetLogButton = GetBorderHandle;
                }

            }
        }
        #endregion


        private void SyncColumnWidth(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < this.TransViewList.VisibleRows.Count; i++)
            {
                Grid GetGrid = ((Border)(this.TransViewList.VisibleRows[i].View.Children[0])).Child as Grid;

                for (int ir = 0; ir < GetGrid.ColumnDefinitions.Count; ir++)
                {
                    GetGrid.ColumnDefinitions[ir].Width = TransViewHeader.ColumnDefinitions[ir].Width;
                }
            }
        }

        public bool EnableFocusMode = false;
        public string LastSelectExView = "";

        public int CodeViewShowState = 0;
        private void SelectExView(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border GetBorderHandle = sender as Border;
                TextBlock GetBlock = GetBorderHandle.Child as TextBlock;


                string GetExViewName = ConvertHelper.ObjToStr(GetBlock.Text);
                switch (GetExViewName)
                {
                    case "Focus Mode":
                        {
                            if (EnableFocusMode == false)
                            {
                                NavTag.Visibility = Visibility.Collapsed;

                                HeadLine.Height = new GridLength(0);
                                FooterLine.Height = new GridLength(0);

                                ModeCol.Width = new GridLength(0);
                                BarCol.Width = new GridLength(0);
                                SettingCol.Width = new GridLength(0);

                                FocusModeTag.Style = (Style)this.FindResource("ExWinShow");
                                EnableFocusMode = true;
                            }
                            else
                            {
                                NavTag.Visibility = Visibility.Visible;

                                HeadLine.Height = new GridLength(2);
                                FooterLine.Height = new GridLength(2);

                                ModeCol.Width = new GridLength(120);
                                BarCol.Width = new GridLength(1, GridUnitType.Star);
                                SettingCol.Width = new GridLength(50);

                                FocusModeTag.Style = (Style)this.FindResource("ExWinHide");
                                EnableFocusMode = false;
                            }
                        }
                        break;
                    case "Code View":
                        {
                            if (LastSelectExView != GetExViewName)
                            {
                                CodeViewTag.Style = (Style)this.FindResource("ExWinShow");
                                ExtendViewTag.Style = (Style)this.FindResource("ExWinHide");

                                DeFine.CurrentCodeView.Dispatcher.Invoke(new Action(() =>
                                {
                                    DeFine.CurrentCodeView.Show();
                                }));
                                DeFine.ExtendWin.Hide();
                                MutiWinHelper.SyncLocation();

                                LastSelectExView = GetExViewName;

                                CodeViewShowState = 1;
                            }
                            else
                            {
                                DeFine.CurrentCodeView.Dispatcher.Invoke(new Action(() =>
                                {
                                    DeFine.CurrentCodeView.Hide();
                                }));
                                CodeViewTag.Style = (Style)this.FindResource("ExWinHide");
                                LastSelectExView = string.Empty;

                                CodeViewShowState = 0;
                            }

                            DeFine.CurrentCodeView.SyncZIndex();
                        }
                        break;
                    case "Extend View":
                        {
                            if (LastSelectExView != GetExViewName)
                            {
                                ExtendViewTag.Style = (Style)this.FindResource("ExWinShow");
                                CodeViewTag.Style = (Style)this.FindResource("ExWinHide");

                                DeFine.ExtendWin.ShowUI();
                                DeFine.CurrentCodeView.Dispatcher.Invoke(new Action(() =>
                                {
                                    DeFine.CurrentCodeView.Hide();
                                }));
                                MutiWinHelper.SyncLocation();

                                LastSelectExView = GetExViewName;
                            }
                            else
                            {
                                DeFine.ExtendWin.Hide();
                                ExtendViewTag.Style = (Style)this.FindResource("ExWinHide");
                                LastSelectExView = string.Empty;
                            }
                        }
                        break;
                }
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            DeFine.CurrentCodeView.SyncZIndex();
        }

        private void FindNpc_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            NPCFinder NNPCFinder = new NPCFinder();
            NNPCFinder.Owner = this;
            NNPCFinder.Show();
        }

        public int NextAutoEnable = 0;
        private const string ToolTipNextAutoOff = "Auto Next: OFF — Click to enable. When enabled, pressing Tab will automatically jump to the next untranslated entry.";
        private const string ToolTipNextAutoOn = "Auto Next: ON — Tab key is now redirected to jump to the next untranslated entry. Click to disable.";
        private bool _AutoLoop = false;
        private void EnableHotKey_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _AutoLoop = !_AutoLoop;

            LastUntranslatedKey = null;

            if (_AutoLoop)
            {
                HotKeyDot.Fill = new SolidColorBrush(Color.FromRgb(11, 116, 209));
                HotKeyArea.ToolTip = ToolTipNextAutoOn;
                NextAutoEnable = 1;
            }
            else
            {
                HotKeyDot.Fill = new SolidColorBrush(Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF));
                HotKeyArea.ToolTip = ToolTipNextAutoOff;
                NextAutoEnable = 0;
            }

            e.Handled = true;
        }

      
    }
}

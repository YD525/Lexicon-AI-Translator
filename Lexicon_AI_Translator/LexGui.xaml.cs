using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LexTranslator.FileManagement;
using LexTranslator.SkyrimManagement;
using LexTranslator.TranslateManage;
using LexTranslator.UIManage;
using LexTranslator.UIManagement;
using LexTranslator.YDControls;
using PexInterface;
using PhoenixEngine;
using PhoenixEngine.Common;
using PhoenixEngine.Events;
using PhoenixEngine.Language;
using PhoenixEngine.Platform.LocalAI;
using PhoenixEngine.Platform;
using PhoenixEngine.Translate;
using System.Threading;
using System.Reflection;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for LexGui.xaml
    /// </summary>
    public partial class LexGui : Window
    {
        public LexGui()
        {
            InitializeComponent();
        }

        private PageSwitcher InfoPage = null;
        private PageSwitcher MainPage = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DeFine.Init(this);

            TranslatorInterface.Init();

            InfoPage = new PageSwitcher(this,InFoPages);
            MainPage = new PageSwitcher(this,Views);

            ShowView("InFo");

            UIHelper.SyncNodes(Nodes);

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

            YDChart.SetAction(
              new Action<RealtimeLineChart>((Ref) =>
              {
                  Ref.PushValue(DeFine.ChartDataRef.GetCurrent());
              }),
              new Action<RealtimeLineChart>((Ref) =>
              {
                  Ref.PushValue(DeFine.ChartDataRef.Total);
              }),
              DeFine.ChartDataRef
             );

            EngineEvents.SetBookTranslateCallback += BookTransCallBack;

            SelectFristSettingNav();
            InfoPage.SwitchPageByHorizontal(0);
        }

        public void BookTransCallBack(string Key, string CurrentText)
        {
            //if (Key.Equals(LastSetKey) && CurrentText.Length > 0)
            //{
            //    ToStr.Dispatcher.Invoke(new Action(() =>
            //    {
            //        ToStr.Text = CurrentText;
            //    }));
            //}
        }


        public void SyncCGLocation()
        {
            if (DeFine.CG != null)
            {
                DeFine.CG.Top = (this.Top - DeFine.CG.ActualHeight) + 1;
                DeFine.CG.Left = this.Left + 100;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentNav == "TransHub")
            {
                if (CurrentTranslateView != null)
                {
                    CurrentTranslateView.Window_PreviewKeyDown(sender,e);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {

        }

        private void Window_Activated(object sender, EventArgs e)
        {

        }

        private void ModTransView_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void ModTransView_Drop(object sender, DragEventArgs e)
        {

        }

        private void ModTransView_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void OpenUrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string GetUrl = "";
            string GetTag = "";

            if (sender is Label)
            {
                GetUrl = "";
                GetTag = P_Convert.ObjToStr(((Label)sender).Tag);

                if (GetTag.Length > 0)
                {
                    GetUrl = GetTag;
                }
                else
                {
                    GetUrl = P_Convert.ObjToStr(((Label)sender).Content);
                }
            }
            if (sender is Run)
            {
                GetUrl = "";
                GetTag = P_Convert.ObjToStr(((Run)sender).Tag);

                if (GetTag.Length > 0)
                {
                    GetUrl = GetTag;
                }
                else
                {
                    GetUrl = P_Convert.ObjToStr(((Run)sender).Text);
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
       
        #region Effect

        public Storyboard XTGlowLoopStoryboard = null;
        private void StartLexGlowLoop()
        {
            XTGlowLoopStoryboard = (Storyboard)FindResource("XTGlowLoop");
            XTGlowLoopStoryboard.Begin();
        }
        private void StopLexGlowLoop()
        {
            XTGlowLoopStoryboard?.Stop();
        }

        #endregion

        #region WinControl

        public void UI(Action Action)
        {
            if (Dispatcher.CheckAccess())
                Action();
            else
                Dispatcher.BeginInvoke(Action);
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

        #region ViewSwitch
        private void ChangeInFoPage(object sender, MouseButtonEventArgs e)
        {
            int Index = P_Convert.ObjToInt((sender as Ellipse).Tag);

            if (sender is Ellipse)
            {
                InfoPage.SwitchPageByHorizontal(Index);

                foreach (var Child in ((sender as Ellipse).Parent as StackPanel).Children)
                {
                    if (Child is Ellipse OtherEllipse)
                    {
                        OtherEllipse.Style = (Style)FindResource("PageBtn");
                    }
                }

                (sender as Ellipse).Style = (Style)FindResource("PageBtnSelected");
            }
        }

        private void ShowView(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Border ClickedMenu)) return;

            ShowView(ClickedMenu.Tag?.ToString());
        }

        public string CurrentNav = "";
        public void ShowView(string Name)
        {
            UI(() =>
            {
                int PageIndex = 0;
                CurrentNav = Name;


                switch (Name)
                {
                    case "InFo":
                        {
                            YDChart.Stop();
                            PageIndex = 0;
                            StartLexGlowLoop();
                            LexVer.Content = DeFine.CurrentVersion;
                            EngineVer.Content = Phoenix.Version;
                            PEXAnalysisVer.Content = PexHeuristicAnalysis.Version;
                            PEXReaderVer.Content = PexInterop.Version;
                            ESPReaderVer.Content = EspReader.Version;
                            DSDConvertVer.Content = DSDConverter.Version;
                        }
                        break;
                    case "DashBoard":
                        {
                            PageIndex = 1;
                            StopLexGlowLoop();

                            if (TranslatorInterface.TranslationStatus == StateControl.Run)
                            {
                                YDChart.Start();
                            }
                            else
                            {
                                YDChart.Clear();
                            }
                        }
                        break;
                    case "TransHub":
                        {
                            YDChart.Stop();
                            PageIndex = 2;
                            StopLexGlowLoop();

                            UpdateTabShowState();
                        }
                        break;
                    case "Settings":
                        {
                            YDChart.Stop();
                            PageIndex = 3;
                            StopLexGlowLoop();
                        }
                        break;

                    default: return;
                }

                MainPage.SwitchPageByVertical(PageIndex);

                foreach (var Child in MainNav.Children)
                {
                    if (Child is Grid RowGrid)
                    {
                        foreach (var SubChild in RowGrid.Children)
                        {
                            if (SubChild is Grid MenuContainer)
                            {
                                var Border = MenuContainer.Children.OfType<Border>().FirstOrDefault();
                                if (Border != null)
                                {
                                    SetMenuSelectedState(Border, Border.Tag?.ToString() == Name);
                                }
                            }
                        }
                    }
                }
            });
        }

        private void SetMenuSelectedState(Border MenuBorder, bool IsSelected)
        {
            if (MenuBorder.Child is Grid internalGrid)
            {
                var Grids = internalGrid.Children.OfType<Grid>().ToList();

                if (Grids.Count >= 2)
                {
                    var IndicatorBar = Grids[0];
                    var BGMask = Grids[1];

                    if (IsSelected)
                    {
                        IndicatorBar.Visibility = Visibility.Visible;
                        BGMask.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        IndicatorBar.Visibility = Visibility.Hidden;
                        BGMask.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        #endregion

        private void ApplyTheme(string mode)
        {
            var resources = this.Resources;

            switch (mode)
            {
                case "Dark":
                    Application.Current.Resources["BackgroundColor"] = "#FF282828";
                    Application.Current.Resources["ForegroundColor"] = "White";
                    Application.Current.Resources["BorderColor"] = "#FF555555";
                    Application.Current.Resources["AccentColor"] = "#FF4D8CF7";
                    Application.Current.Resources["PanelBackground"] = "#FF3D3D3D";
                    break;

                case "Light":
                    Application.Current.Resources["BackgroundColor"] = "#FFF5F5F5";
                    Application.Current.Resources["ForegroundColor"] = "Black";
                    Application.Current.Resources["BorderColor"] = "#FFCCCCCC";
                    Application.Current.Resources["AccentColor"] = "#FF4D8CF7";
                    Application.Current.Resources["PanelBackground"] = "White";
                    break;
            }
        }

        #region FileTabs

        public void LoadFile()
        {
            var Dialog = new Microsoft.Win32.OpenFileDialog();
            Dialog.Title = "Please select a file";
            Dialog.Filter = "All files|*.*";
            Dialog.Multiselect = false;

            if (Dialog.ShowDialog() == true)
            {
                string SelectedFile = Dialog.FileName;
                LoadFile(SelectedFile);
            }
        }

        public void LoadFile(string Path)
        {
            AddTab(Path, true);
            UpdateTabShowState();
        }
        private void SelectFile(object sender, MouseButtonEventArgs e)
        {
            LoadFile();
        }

        private void UpdateTabShowState()
        {
            bool IsEmpty = LexTabs.Items.Count == 0;

            EmptyTabView.Visibility = IsEmpty ? Visibility.Visible : Visibility.Collapsed;
            Tab.Visibility = IsEmpty ? Visibility.Collapsed : Visibility.Visible;
        }
        public class FileTabContext
        {
            public string Path { get; set; }
            public TranslateView View { get; set; }
        }


        private TabItem _DraggedTab;
        private Point _DragStartPoint;
        private Border _DraggedTabBg;
        private AdornerLayer _AdornerLayer;
        private DragAdorner _DragAdorner;

        private void LexTabs_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _DragStartPoint = e.GetPosition(null);
            _DraggedTab = FindAncestor<TabItem>(e.OriginalSource as DependencyObject);
        }

        private void LexTabs_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_DraggedTab == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            Point CurrentPos = e.GetPosition(null);
            if (Math.Abs(CurrentPos.X - _DragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(CurrentPos.Y - _DragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            if (_DraggedTabBg == null)
            {
                _DraggedTabBg = FindTabBg(_DraggedTab);
                AnimateOpacity(_DraggedTab, 0.35, 120);
                _DraggedTab.Cursor = Cursors.SizeWE;

                _AdornerLayer = AdornerLayer.GetAdornerLayer(LexTabs);
                _DragAdorner = new DragAdorner(LexTabs, _DraggedTab, e.GetPosition(LexTabs));
                _AdornerLayer.Add(_DragAdorner);
            }

            _DragAdorner.UpdatePosition(e.GetPosition(LexTabs));

            TabItem TargetTab = FindAncestor<TabItem>(e.OriginalSource as DependencyObject);
            if (TargetTab == null || TargetTab == _DraggedTab)
                return;

            int DraggedIndex = LexTabs.Items.IndexOf(_DraggedTab);
            int TargetIndex = LexTabs.Items.IndexOf(TargetTab);
            if (DraggedIndex < 0 || TargetIndex < 0)
                return;

            LexTabs.Items.RemoveAt(DraggedIndex);
            LexTabs.Items.Insert(TargetIndex, _DraggedTab);
            LexTabs.SelectedItem = _DraggedTab;

            FlashSwap(TargetTab);
        }

        private void LexTabs_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_DraggedTab != null)
            {
                AnimateOpacity(_DraggedTab, 1.0, 150);
                _DraggedTab.ClearValue(FrameworkElement.CursorProperty);
            }

            if (_AdornerLayer != null && _DragAdorner != null)
            {
                _AdornerLayer.Remove(_DragAdorner);
            }

            _DraggedTab = null;
            _DraggedTabBg = null;
            _AdornerLayer = null;
            _DragAdorner = null;
        }

        private void AnimateOpacity(TabItem Tab, double ToValue, int DurationMs)
        {
            DoubleAnimation Anim = new DoubleAnimation(ToValue, TimeSpan.FromMilliseconds(DurationMs));
            Tab.BeginAnimation(TabItem.OpacityProperty, Anim);
        }

        private void FlashSwap(TabItem Tab)
        {
            Border TabBg = FindTabBg(Tab);
            if (TabBg == null)
                return;

            SolidColorBrush FlashBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2A2A2A"));
            TabBg.Background = FlashBrush;

            ColorAnimation Anim = new ColorAnimation
            {
                From = (Color)ColorConverter.ConvertFromString("#FFFAE306"),
                To = (Color)ColorConverter.ConvertFromString("#FF2A2A2A"),
                Duration = TimeSpan.FromMilliseconds(280)
            };
            FlashBrush.BeginAnimation(SolidColorBrush.ColorProperty, Anim);
        }

        private Border FindTabBg(TabItem Tab)
        {
            Tab.ApplyTemplate();
            return Tab.Template?.FindName("TabBg", Tab) as Border;
        }

        private static T FindAncestor<T>(DependencyObject Current) where T : DependencyObject
        {
            while (Current != null && !(Current is T))
                Current = VisualTreeHelper.GetParent(Current);
            return Current as T;
        }
        public TranslateView CurrentTranslateView = null;
        private void LexTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Tab = LexTabs.SelectedItem as TabItem;
            if (Tab == null)
                return;

            var CTX = Tab.Tag as FileTabContext;
            if (CTX == null)
                return;

            foreach (UIElement Child in TabViews.Children)
            {
                Child.Visibility = Visibility.Hidden;
            }

            if (CTX.View != null)
            {
                if (!TabViews.Children.Contains(CTX.View))
                {
                    TabViews.Children.Add(CTX.View);
                }

                CTX.View.Visibility = Visibility.Visible;
                CurrentTranslateView = CTX.View;
            }
        }

        private void LexTabs_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var Dep = e.OriginalSource as DependencyObject;

            if (Dep == null)
                return;

            var AnyBtn = FindAncestor<Border>(Dep);

            if (AnyBtn != null && AnyBtn.Tag?.ToString() == "LexTabClose")
            {
                var TabItem = FindAncestor<TabItem>(Dep);

                if (TabItem != null)
                {
                    e.Handled = true;

                    RemoveTab((TabItem.Tag as FileTabContext).Path);
                }
            }

            if (AnyBtn != null && AnyBtn.Tag?.ToString() == "LexTabAdd")
            {
                e.Handled = true;

                LoadFile();

                return;
            }
        }
        public void AddTab(string Path, bool Select = true)
        {
            UI(() =>
            {
                foreach (TabItem Item in LexTabs.Items)
                {
                    if (Item.Tag is FileTabContext CTX && CTX.Path == Path)
                    {
                        if (Select)
                            LexTabs.SelectedItem = Item;

                        return;
                    }
                }

                var View = new TranslateView();
                View.SetFile(this,Path);
                View.Visibility = Visibility.Collapsed;

                var CTXNew = new FileTabContext
                {
                    Path = Path,
                    View = View
                };

                var Tab = new TabItem
                {
                    Header = System.IO.Path.GetFileName(Path),
                    Tag = CTXNew
                };

                LexTabs.Items.Add(Tab);

                TabViews.Children.Add(View);

                if (Select)
                    LexTabs.SelectedItem = Tab;

                UpdateTabShowState();
            });
        }
        public void RemoveTab(string Path)
        {
            UI(() =>
            {
                TabItem Target = null;
                FileTabContext CTX = null;

                foreach (TabItem Item in LexTabs.Items)
                {
                    if (Item.Tag is FileTabContext c && c.Path == Path)
                    {
                        Target = Item;
                        CTX = c;
                        break;
                    }
                }

                if (Target == null)
                    return;

                if (CTX != null && CTX.View != null)
                {
                    CTX.View.Close();
                    CTX.View.Visibility = Visibility.Collapsed;
                }

                if (CurrentTranslateView == CTX.View)
                {
                    CurrentTranslateView = null;
                }

                LexTabs.Items.Remove(Target);

                UpdateTabShowState();
            });
        }
        #endregion

        #region Nodes
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


     
        private double CalcLeftMenuHeight()
        {
            double AutoHeight = 0;
            foreach (FrameworkElement GetRow in Nodes.Children)
            {
                AutoHeight += GetRow.ActualHeight + 1;
            }
            return AutoHeight;
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

        public void ShowLeftMenu(bool Show)
        {
            this.Dispatcher.Invoke(new Action(() =>
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

        #endregion

        private void Mask_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }


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

                    string GetContent = P_Convert.ObjToStr(((Label)GetBorderHandle.Child).Content);

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

        #region Setting
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

                // EspFilterStr.Text = GlobalEspReader.GetFilterByStr(); //！
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
        public void ShowFrame(string Name)
        {
            for (int i = 0; i < this.SettingFrames.Children.Count; i++)
            {
                if (this.SettingFrames.Children[i] is Border)
                {
                    Border GetFrame = (Border)this.SettingFrames.Children[i];
                    string GetTag = P_Convert.ObjToStr(GetFrame.Tag);
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
        private void SelectSettingNav(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border GetNav = (Border)sender;

                SelectSettingNav(GetNav);
            }
        }
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
                        return P_Convert.ObjToStr(((Label)GetMainGrid.Children[0]).Content);
                }
            }
            return string.Empty;
        }

        private void UILanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetValue = P_Convert.ObjToStr(UILanguages.SelectedValue);
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
        private void SCodeGenStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var GetValue = P_Convert.ObjToStr(SCodeGenStyle.SelectedValue);
            if (GetValue.Length > 0)
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
            Phoenix.Config.ContextLimit = P_Convert.ObjToInt(SContextLimit.Text);
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
            }
            else
            {
                Phoenix.Config.ContextEnable = false;

                if (DeFine.NodeStyleWin.ContextCheckBox != null)
                {
                    DeFine.NodeStyleWin.ContextCheckBox.IsChecked = false;
                }
            }
        }

        private void SGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GetName = P_Convert.ObjToStr(SGame.SelectedValue);
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
            Phoenix.Config.ThrottleRatio = P_Convert.ObjToDouble(SThrottlingRatio.Text);
        }
        private void SRotationDelay_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.ThrottleDelayMs = P_Convert.ObjToInt(SRotationDelay.Text);
        }

        private void SMaxThread_TextChanged(object sender, TextChangedEventArgs e)
        {
            Phoenix.Config.MaxThreadCount = P_Convert.ObjToInt(SMaxThread.Text);

            if (TranslatorInterface.Instance != null)
            {
                if (TranslatorInterface.Instance.GetBatchCore() != null)
                {
                    TranslatorInterface.Instance.GetBatchCore().AutoThreadLimit = Phoenix.Config.MaxThreadCount;
                }
            }
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

        private void ReSetFilter(object sender, MouseButtonEventArgs e)
        {
            //GlobalEspReader.ResetToSkyrimFilter();
            //EspFilterStr.Text = GlobalEspReader.GetFilterByStr();

            //DeFine.GlobalLocalSetting.CustomFilterStr = string.Empty;
            //DeFine.GlobalLocalSetting.SaveConfig();
        }

        private void SetFilter(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    var FilterDict = GlobalEspReader.ParseFilterString(EspFilterStr.Text);

            //    var SourceFilterStr = GlobalEspReader.GetFilterByStr();

            //    if (SourceFilterStr.ToUpper() != EspFilterStr.Text.ToUpper())
            //    {
            //        if (FilterDict.Count > 0)
            //        {
            //            GlobalEspReader.SetFilter(FilterDict);
            //            DeFine.GlobalLocalSetting.CustomFilterStr = EspFilterStr.Text;
            //            DeFine.GlobalLocalSetting.SaveConfig();
            //        }
            //    }
            //}
            //catch
            //{
            //    MessageBoxExtend.Show(this, "The string used to set the filter is incorrect.");
            //}
        }

        #endregion

        #region Drag
        //public bool IsDragEnter = false;

        //private void ModTransView_DragEnter(object sender, DragEventArgs e)
        //{
        //    if (e.Data != null)
        //    {
        //        string[] OneFile = (string[])e.Data.GetData(DataFormats.FileDrop);
        //        if (OneFile == null)
        //        {
        //            return;
        //        }
        //        if (OneFile.Length == 0)
        //        {
        //            return;
        //        }
        //    }

        //    ModTransView.Visibility = Visibility.Collapsed;
        //    DragDropView.Visibility = Visibility.Visible;
        //    IsDragEnter = true;
        //}

        //private void ModTransView_DragLeave(object sender, DragEventArgs e)
        //{
        //    if (e != null)
        //        if (e.Data != null)
        //        {
        //            string[] OneFile = (string[])e.Data.GetData(DataFormats.FileDrop);
        //            if (OneFile == null)
        //            {
        //                return;
        //            }
        //            if (OneFile.Length == 0)
        //            {
        //                return;
        //            }
        //        }

        //    ModTransView.Visibility = Visibility.Visible;
        //    DragDropView.Visibility = Visibility.Collapsed;
        //    if (IsDragEnter)
        //    {
        //        IsDragEnter = false;
        //    }
        //}

        //private void ModTransView_Drop(object sender, DragEventArgs e)
        //{
        //    if (e != null)
        //        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        //        {
        //            string[] OneFile = (string[])e.Data.GetData(DataFormats.FileDrop);

        //            if (OneFile.Length > 0)
        //            {
        //                //Fix Long Path
        //                string GetFilePath = Path.GetFullPath(OneFile[0]);
        //                if (GetFilePath.Length >= 260)
        //                {
        //                    GetFilePath = @"\\?\" + GetFilePath;
        //                }

        //                if (File.Exists(GetFilePath))
        //                {
        //                    new Thread(() =>
        //                    {
        //                        this.Dispatcher.BeginInvoke(new Action(() =>
        //                        {
        //                            LoadAny(GetFilePath);
        //                        }), System.Windows.Threading.DispatcherPriority.Background);
        //                    }).Start();

        //                    ModTransView_DragLeave(null, null);
        //                }
        //            }
        //        }
        //}

        //private void Traditional_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    new TraditionalConvert(new TranslateView()).Show();
        //}

        #endregion


    }
}

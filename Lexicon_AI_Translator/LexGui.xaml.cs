using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using LexTranslator.FileManagement;
using LexTranslator.SkyrimManagement;
using LexTranslator.UIManagement;
using PexInterface;
using PhoenixEngine;
using PhoenixEngine.Common;

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
            InfoPage = new PageSwitcher(this,InFoPages);
            MainPage = new PageSwitcher(this,Views);
            ShowView("InFo");
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
                        }
                        break;
                    case "TransHub":
                        {
                            PageIndex = 2;
                            StopLexGlowLoop();
                        }
                        break;
                    case "Settings":
                        {
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
            AddTab(Path,true);
        }



        #region FileTabs
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
            });
        }
        #endregion
    }
}

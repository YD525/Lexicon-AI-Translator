using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using LexTranslator.FileManagement;
using LexTranslator.SkyrimManagement;
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

        public void ShowView(string Name)
        {
            this.Dispatcher.Invoke(new Action(() => 
            {
                int PageIndex = 0;

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
            }));
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
    }
}

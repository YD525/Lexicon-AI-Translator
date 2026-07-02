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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LexTranslator.UIManagement;
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InfoPage = new PageSwitcher(this,InFoPages);
            StartXTGlowLoop();
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

        private void ShowView(object sender, MouseButtonEventArgs e)
        {

        }

        public Storyboard XTGlowLoopStoryboard = null;
        private void StartXTGlowLoop()
        {
            XTGlowLoopStoryboard = (Storyboard)FindResource("XTGlowLoop");
            XTGlowLoopStoryboard.Begin();
        }

        private void StopXTGlowLoop()
        {
            XTGlowLoopStoryboard?.Stop();
        }

        private void OpenUrl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ChangeInFoPage(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse)
            {
                int Page = P_Convert.ObjToInt((sender as Ellipse).Tag);
                InfoPage.SwitchPageByHorizontal(Page);

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
    }
}

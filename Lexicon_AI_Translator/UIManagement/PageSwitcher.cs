using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;

namespace LexTranslator.UIManagement
{
    public class PageSwitcher
    {
        private const double EdgeBuffer = 70.0;
        private const int AnimationDurationMs = 350;

        public Window WorkingWin { get; set; }
        public Grid Views { get; set; }
        private int _CurrentPage = 0; 

        public PageSwitcher(Window WorkingWin, Grid Views)
        {
            this.WorkingWin = WorkingWin;
            this.Views = Views;
        }

        private double GetViewH() => WorkingWin.ActualHeight + EdgeBuffer;
        private double GetViewW() => WorkingWin.ActualWidth + EdgeBuffer;

  
        public void SwitchPageByVertical(int Index) 
        {
            if (_CurrentPage == Index) return;

            var Pages = Views.Children.OfType<Grid>().ToList(); 
            double ViewH = GetViewH();                          

            var Ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
            var Dur = new Duration(TimeSpan.FromMilliseconds(AnimationDurationMs));

            Pages[Index].Visibility = Visibility.Visible;

            for (int I = 0; I < Pages.Count; I++)
            {
                var Page = Pages[I];
                if (!(Page.RenderTransform is TranslateTransform))
                    Page.RenderTransform = new TranslateTransform();

                var T = (TranslateTransform)Page.RenderTransform;

                T.BeginAnimation(TranslateTransform.XProperty,
                    new DoubleAnimation { To = 0, Duration = TimeSpan.Zero });

                double TargetY = I == Index ? 0
                               : I < Index ? -ViewH
                               : ViewH;

                var YAnimation = new DoubleAnimation { To = TargetY, Duration = Dur, EasingFunction = Ease };
                if (I == _CurrentPage)
                {
                    YAnimation.Completed += (Sender, Args) => Page.Visibility = Visibility.Collapsed;
                }

                T.BeginAnimation(TranslateTransform.YProperty, YAnimation);
            }

            _CurrentPage = Index;
        }

        public void SwitchPageByHorizontal(int Index)
        {
            if (_CurrentPage == Index) return;

            var Pages = Views.Children.OfType<Grid>().ToList();
            double ViewW = GetViewW();                         

            var Ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
            var Dur = new Duration(TimeSpan.FromMilliseconds(AnimationDurationMs));

            Pages[Index].Visibility = Visibility.Visible;

            for (int I = 0; I < Pages.Count; I++)
            {
                var Page = Pages[I];
                if (!(Page.RenderTransform is TranslateTransform))
                    Page.RenderTransform = new TranslateTransform();

                var T = (TranslateTransform)Page.RenderTransform;

                T.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation { To = 0, Duration = TimeSpan.Zero });

                double TargetX = I == Index ? 0
                               : I < Index ? -ViewW
                               : ViewW;

                var XAnimation = new DoubleAnimation { To = TargetX, Duration = Dur, EasingFunction = Ease };
                if (I == _CurrentPage)
                {
                    XAnimation.Completed += (Sender, Args) => Page.Visibility = Visibility.Collapsed;
                }

                T.BeginAnimation(TranslateTransform.XProperty, XAnimation);
            }

            _CurrentPage = Index;
        }
    }
}

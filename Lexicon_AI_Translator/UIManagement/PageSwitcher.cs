using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using System;
using System.Linq;

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
        if (Index < 0 || Index >= Pages.Count) return;

        double ViewH = GetViewH();
        var Ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
        var Dur = new Duration(TimeSpan.FromMilliseconds(AnimationDurationMs));

        int OldPage = _CurrentPage;
        _CurrentPage = Index;

        foreach (var Page in Pages)
        {
            if (Page.RenderTransform is TranslateTransform T)
            {
                T.BeginAnimation(TranslateTransform.YProperty, null);
                T.BeginAnimation(TranslateTransform.XProperty, null);
            }
        }

        Pages[Index].Visibility = Visibility.Visible;
        Pages[OldPage].Visibility = Visibility.Visible;

        for (int I = 0; I < Pages.Count; I++)
        {
            var Page = Pages[I];
            if (!(Page.RenderTransform is TranslateTransform))
                Page.RenderTransform = new TranslateTransform();

            var T = (TranslateTransform)Page.RenderTransform;

            T.BeginAnimation(TranslateTransform.XProperty, null);
            T.X = 0;

            double TargetY = I == Index ? 0
                           : I < Index ? -ViewH
                           : ViewH;

            var YAnimation = new DoubleAnimation { To = TargetY, Duration = Dur, EasingFunction = Ease };

            if (I == Index)
            {
                YAnimation.From = Index > OldPage ? ViewH : -ViewH;
            }

            if (I == OldPage)
            {
                var PageToHide = Page;
                YAnimation.Completed += (Sender, Args) =>
                {
                    if (Pages.IndexOf(PageToHide) != _CurrentPage)
                    {
                        PageToHide.Visibility = Visibility.Collapsed;
                    }
                };
            }
            else if (I != Index)
            {
                Page.Visibility = Visibility.Collapsed;
                T.Y = TargetY;
                continue;
            }

            T.BeginAnimation(TranslateTransform.YProperty, YAnimation);
        }
    }

    public void SwitchPageByHorizontal(int Index)
    {
        if (_CurrentPage == Index) return;

        var Pages = Views.Children.OfType<Grid>().ToList();
        if (Index < 0 || Index >= Pages.Count) return;

        double ViewW = GetViewW();
        var Ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
        var Dur = new Duration(TimeSpan.FromMilliseconds(AnimationDurationMs));

        int OldPage = _CurrentPage;
        _CurrentPage = Index;

        foreach (var Page in Pages)
        {
            if (Page.RenderTransform is TranslateTransform T)
            {
                T.BeginAnimation(TranslateTransform.YProperty, null);
                T.BeginAnimation(TranslateTransform.XProperty, null);
            }
        }

        Pages[Index].Visibility = Visibility.Visible;
        Pages[OldPage].Visibility = Visibility.Visible;

        for (int I = 0; I < Pages.Count; I++)
        {
            var Page = Pages[I];
            if (!(Page.RenderTransform is TranslateTransform))
                Page.RenderTransform = new TranslateTransform();

            var T = (TranslateTransform)Page.RenderTransform;

            T.BeginAnimation(TranslateTransform.YProperty, null);
            T.Y = 0;

            double TargetX = I == Index ? 0
                           : I < Index ? -ViewW
                           : ViewW;

            var XAnimation = new DoubleAnimation { To = TargetX, Duration = Dur, EasingFunction = Ease };

            if (I == Index)
            {
                XAnimation.From = Index > OldPage ? ViewW : -ViewW;
            }

            if (I == OldPage)
            {
                var PageToHide = Page;
                XAnimation.Completed += (Sender, Args) =>
                {
                    if (Pages.IndexOf(PageToHide) != _CurrentPage)
                    {
                        PageToHide.Visibility = Visibility.Collapsed;
                    }
                };
            }
            else if (I != Index)
            {
                Page.Visibility = Visibility.Collapsed;
                T.X = TargetX;
                continue;
            }

            T.BeginAnimation(TranslateTransform.XProperty, XAnimation);
        }
    }
}
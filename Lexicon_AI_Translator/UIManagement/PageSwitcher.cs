using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using System;
using System.Linq;
using System.Collections.Generic;

public class PageSwitcher
{
    private const double EdgeBuffer = 70.0;
    private const int AnimationDurationMs = 350;

    public Window WorkingWin { get; set; }
    public Grid Views { get; set; }
    private int _CurrentPage = -1;

    public PageSwitcher(Window WorkingWin, Grid Views)
    {
        this.WorkingWin = WorkingWin;
        this.Views = Views;
    }

    private double GetViewH() => WorkingWin.ActualHeight + EdgeBuffer;
    private double GetViewW() => WorkingWin.ActualWidth + EdgeBuffer;

    private void FadeInPage(Grid page)
    {
        page.Opacity = 0;
        page.Visibility = Visibility.Visible;
        var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(2));
        page.BeginAnimation(UIElement.OpacityProperty, anim);
    }

    public void SwitchPageByVertical(int Index)
    {
        var Pages = Views.Children.OfType<Grid>().ToList();
        if (Index < 0 || Index >= Pages.Count) return;

        if (_CurrentPage == -1)
        {
            _CurrentPage = Index;
            FadeInPage(Pages[Index]);
            return;
        }

        if (_CurrentPage == Index) return;

        double ViewH = GetViewH();
        int OldPage = _CurrentPage;
        _CurrentPage = Index;

        PrepareTransform(Pages);

        for (int I = 0; I < Pages.Count; I++)
        {
            var Page = Pages[I];
            var T = GetOrCreateTransform(Page);

            if (I == Index || I == OldPage) Page.Visibility = Visibility.Visible;
            else Page.Visibility = Visibility.Collapsed;

            T.BeginAnimation(TranslateTransform.XProperty, null);
            T.X = 0;

            double TargetY = I == Index ? 0 : (I < Index ? -ViewH : ViewH);
            var YAnimation = new DoubleAnimation(TargetY, TimeSpan.FromMilliseconds(AnimationDurationMs))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            if (I == Index) YAnimation.From = Index > OldPage ? ViewH : -ViewH;

            YAnimation.Completed += (s, e) => {
                if (I == OldPage) Page.Visibility = Visibility.Collapsed;
            };

            T.BeginAnimation(TranslateTransform.YProperty, YAnimation);
        }
    }

    public void SwitchPageByHorizontal(int Index)
    {
        var Pages = Views.Children.OfType<Grid>().ToList();
        if (Index < 0 || Index >= Pages.Count) return;

        if (_CurrentPage == -1)
        {
            _CurrentPage = Index;
            FadeInPage(Pages[Index]);
            return;
        }

        if (_CurrentPage == Index) return;

        double ViewW = GetViewW();
        int OldPage = _CurrentPage;
        _CurrentPage = Index;

        PrepareTransform(Pages);

        for (int I = 0; I < Pages.Count; I++)
        {
            var Page = Pages[I];
            var T = GetOrCreateTransform(Page);

            if (I == Index || I == OldPage) Page.Visibility = Visibility.Visible;
            else Page.Visibility = Visibility.Collapsed;

            T.BeginAnimation(TranslateTransform.YProperty, null);
            T.Y = 0;

            double TargetX = I == Index ? 0 : (I < Index ? -ViewW : ViewW);
            var XAnimation = new DoubleAnimation(TargetX, TimeSpan.FromMilliseconds(AnimationDurationMs))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            if (I == Index) XAnimation.From = Index > OldPage ? ViewW : -ViewW;

            XAnimation.Completed += (s, e) => {
                if (I == OldPage) Page.Visibility = Visibility.Collapsed;
            };

            T.BeginAnimation(TranslateTransform.XProperty, XAnimation);
        }
    }

    private void PrepareTransform(List<Grid> pages)
    {
        foreach (var Page in pages)
        {
            if (Page.RenderTransform is TranslateTransform T)
            {
                T.BeginAnimation(TranslateTransform.YProperty, null);
                T.BeginAnimation(TranslateTransform.XProperty, null);
            }
        }
    }

    private TranslateTransform GetOrCreateTransform(Grid page)
    {
        if (!(page.RenderTransform is TranslateTransform))
            page.RenderTransform = new TranslateTransform();
        return (TranslateTransform)page.RenderTransform;
    }
}
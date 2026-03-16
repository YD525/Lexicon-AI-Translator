using System;
using System.Windows;

namespace LexTranslator.UIManagement
{
    public class MutiWinHelper
    {
        public static void SyncLocation()
        {
            if (DeFine.WorkingWin == null) return;

            double ExtendWinWidth = 300;

            double TittleHeight = 30;
            double FooterHeight = 10;

            double CalcLeft = DeFine.WorkingWin.Left + DeFine.WorkingWin.ActualWidth;
            double CalcTop = DeFine.WorkingWin.Top + TittleHeight;
            double CalcHeight = DeFine.WorkingWin.ActualHeight - (TittleHeight + FooterHeight);

            if (DeFine.ExtendWin.Visibility == Visibility.Visible)
            {
                DeFine.ExtendWin.Left = CalcLeft;
                DeFine.ExtendWin.Top = CalcTop;
                DeFine.ExtendWin.Height = CalcHeight;

                DeFine.ExtendWin.Width = ExtendWinWidth;

                if (DeFine.ExtendWin.WindowState == WindowState.Minimized)
                {
                    DeFine.ExtendWin.WindowState = WindowState.Normal;
                }
            }
            else
            {
                ExtendWinWidth = (DeFine.WorkingWin.ActualWidth * 0.7);
                DeFine.CurrentCodeView.Dispatcher.Invoke(new Action(() => 
                {
                    if (DeFine.CurrentCodeView.Visibility == Visibility.Visible)
                    {
                        DeFine.CurrentCodeView.Left = CalcLeft;
                        DeFine.CurrentCodeView.Top = CalcTop;
                        DeFine.CurrentCodeView.Height = CalcHeight;

                        DeFine.CurrentCodeView.Width = ExtendWinWidth;

                        if (DeFine.CurrentCodeView.WindowState == WindowState.Minimized)
                        {
                            DeFine.CurrentCodeView.WindowState = WindowState.Normal;
                        }

                        DeFine.CurrentCodeView.SyncZIndex();
                    }
                }));
            }
        }
    }
}

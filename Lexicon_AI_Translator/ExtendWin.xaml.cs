using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using LexTranslator.SkyrimManagement;
using LexTranslator.UIManage;
using System.Collections.Generic;
using PhoenixEngine.ADO;
using PhoenixEngine;
using PhoenixEngine.Engine;
using LexTranslator.TranslateManage;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for ExtendWin.xaml
    /// </summary>
    public partial class ExtendWin : Window
    {
        public ExtendWin()
        {
            InitializeComponent();
        }

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
            this.Hide();
        }

        #endregion

        public bool IsShow = false;
        public void CloseUI()
        {
            IsShow = false;

            MatchView.Children.Clear();
            this.Hide();
        }

        public bool CanShow = true;

        public void ShowUI()
        {
            IsShow = true;

            if (CanShow)
                this.Show();
        }

        private CancellationTokenSource _CancellationTokenSource;
        public Thread SearchTrd = null;

        public void CancelSearch()
        {
            _CancellationTokenSource?.Cancel();
        }

        public void SetOriginal(string Original, StringItem StringItem)
        {
            uint StringKey = 0;
            if (IsShow)
            {
                if (StringItem != null)
                {
                    StringKey = StringItem.ID;
                }

                if (SearchTrd != null)
                {
                    CancelSearch();

                    SearchTrd = null;
                }

                _CancellationTokenSource = new CancellationTokenSource();


                SearchTrd = new Thread(() =>
                {
                    MatchTransItem(Original, StringKey, _CancellationTokenSource.Token);
                    SearchTrd = null;
                });

                SearchTrd.Start();

            }
        }

        public EspReader EspInstance = null;
        public void MatchTransItem(string Original, uint StringKey, CancellationToken CancellationToken)
        {
            //MatchView.Dispatcher.Invoke(new Action(() =>
            //{
            //    MatchView.Children.Clear();
            //}));

            //List<string> UniqueResult = new List<string>();
            //List<string> UniqueKeys = new List<string>();

            //var MatchCloudItems = LocalDBCache.MatchLocalItem((int)TranslatorInterface.Instance.To, Original);

            //foreach (var GetMatch in MatchCloudItems)
            //{
            //    if (!UniqueResult.Contains(GetMatch.Result))
            //    {
            //        UniqueResult.Add(GetMatch.Result);
            //        if (!UniqueKeys.Contains(GetMatch.Key))
            //        {
            //            UniqueKeys.Add(GetMatch.Key);
            //            MatchView.Dispatcher.Invoke(new Action(() =>
            //            {
            //                MatchView.Children.Add(UIHelper.CreatMatchLine(
            //                  UniqueKeyHelper.RowidToOriginalKey(GetMatch.FileUniqueKey),//Get Original File Name
            //                  GetMatch.Key,
            //                  GetMatch.Result
            //                  ));
            //            }));
            //        }
            //    }
            //}

            //foreach (var GetMatch in CloudDBCache.MatchCloudItem((int)TranslatorInterface.Instance.To, Original))
            //{
            //    if (!UniqueResult.Contains(GetMatch.Result))
            //    {
            //        UniqueResult.Add(GetMatch.Result);
            //        if (!UniqueKeys.Contains(GetMatch.Key))
            //        {
            //            UniqueKeys.Add(GetMatch.Key);
            //            MatchView.Dispatcher.Invoke(new Action(() =>
            //            {
            //                MatchView.Children.Add(UIHelper.CreatMatchLine(
            //                  UniqueKeyHelper.RowidToOriginalKey(GetMatch.FileUniqueKey),//Get Original File Name
            //                  GetMatch.Key,
            //                  GetMatch.Result
            //                  ));
            //            }));
            //        }
            //    }
            //}
            

            //Find DL IL Strings
            //if (StringKey != 0)
            //    if (DeFine.WorkingWin.CurrentTransType == 2)
            //    {
            //        if (EspInstance.ToStringsFile != null)
            //        {
            //            if (EspInstance.ToStringsFile.Strings.ContainsKey(StringKey) == true)
            //            {
            //                string AutoFileName = "Strings";
            //                var FindItem = EspInstance.ToStringsFile.Strings[StringKey];

            //                if (FindItem.Type == StringsFileType.DL)
            //                {
            //                    AutoFileName += ".dlstrings";
            //                }
            //                else
            //                if (FindItem.Type == StringsFileType.IL)
            //                {
            //                    AutoFileName += ".ilstrings";
            //                }
            //                else
            //                {
            //                    AutoFileName += ".strings";
            //                }
            //                MatchView.Dispatcher.Invoke(new Action(() =>
            //                {
            //                    MatchView.Children.Add(UIHelper.CreatMatchLine(
            //                    FindItem.Type.ToString(),
            //                    FindItem.ID.ToString(),
            //                    FindItem.Value
            //                    ));
            //                }));
            //            }
            //        }
            //    }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}

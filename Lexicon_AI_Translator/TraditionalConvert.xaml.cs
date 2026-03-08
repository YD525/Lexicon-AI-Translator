using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using LexTranslator.TranslateManage;
using PhoenixEngine;
using PhoenixEngine.ADO;
using PhoenixEngine.Language;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for TraditionalConvert.xaml
    /// </summary>
    public partial class TraditionalConvert : Window
    {
        public TraditionalConvert()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Owner = DeFine.WorkingWin;
        }

        public Thread ConvertTrd = null;
        private void ConvertCurrent_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string GetOriginal = DeFine.WorkingWin.FromStr.Text;
            ConvertTrd = new Thread(() =>
            {
                string Result = ChineseVariantMap.SimplifiedToTraditionalByReq(GetOriginal);

                if (Result.Length > 0)
                {
                    DeFine.WorkingWin.Dispatcher.Invoke(new Action(() => {
                        DeFine.WorkingWin.ToStr.Text = Result;
                    }));
                }

                ConvertTrd = null;
            });
            ConvertTrd.Start();
        }

        private void ConvertAll_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ConvertTrd = new Thread(() =>
            {
                int Total = DeFine.WorkingWin.TransViewList.RealLines.Count;

                for (int i = 0; i < DeFine.WorkingWin.TransViewList.RealLines.Count; i++)
                {
                    if (DeFine.WorkingWin.TransViewList.RealLines[i].Score <= 0)
                    {
                        continue;
                    }
                    string Source = DeFine.WorkingWin.TransViewList.RealLines[i].SourceText;
                    var Result = ChineseVariantMap.SimplifiedToTraditionalByReq(Source);

                    if (Source.ToLower().Replace(" ","") != Result.ToLower().Replace(" ", ""))
                    {
                        DeFine.WorkingWin.TransViewList.RealLines[i].TransText = Result;

                        var Key = DeFine.WorkingWin.TransViewList.RealLines[i].Key;

                        var Link = TranslatorInterface.Instance.GetLink();

                        Link[Key] = Result;

                        DeFine.WorkingWin.TransViewList.RealLines[i].SyncUI(DeFine.WorkingWin.TransViewList);

                        CloudDBCache.AddCache(Phoenix.GetFileUniqueKey(), Key, (int)Phoenix.To, Source, Result);
                    }

                    ConvertAllBtn.Dispatcher.Invoke(new Action(() => {
                        ConvertAllBtn.Content = string.Format("Converting ({0}/{1})", i, Total);
                    }));
                }

                ConvertAllBtn.Dispatcher.Invoke(new Action(() => {
                    ConvertAllBtn.Content = "All to Traditional";
                }));

                ConvertTrd = null;
            });
            ConvertTrd.Start();
        }
    }
}

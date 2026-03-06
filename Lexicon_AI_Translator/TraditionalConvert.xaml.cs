using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LexTranslator.TranslateManage;
using PhoenixEngine.EngineManagement;
using PhoenixEngine.LanguageManagement;
using PhoenixEngine.TranslateCore;

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

                        if (TranslatorInterface.Instance.TranslatedLink.ContainsKey(Key))
                        {
                            TranslatorInterface.Instance.TranslatedLink[Key] = Result;
                        }
                        else
                        {
                            TranslatorInterface.Instance.TranslatedLink.Add(Key, Result);
                        }

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

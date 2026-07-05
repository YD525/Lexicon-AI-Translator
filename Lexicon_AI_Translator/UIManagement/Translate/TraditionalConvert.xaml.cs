using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using LexTranslator.TranslateManage;
using LexTranslator.UIManagement;
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
        private TranslateView _Owner;
        public TraditionalConvert(TranslateView Owner)
        {
            InitializeComponent();
            this._Owner = Owner;
        }
     
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Owner = _Owner.Parent;
        }

        public Thread ConvertTrd = null;
        private void ConvertCurrent_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string GetOriginal = _Owner.FromStr.Text;
            ConvertTrd = new Thread(() =>
            {
                string Result = ChineseVariantMap.SimplifiedToTraditionalByReq(GetOriginal);

                if (Result.Length > 0)
                {
                    _Owner.Dispatcher.Invoke(new Action(() => {
                        _Owner.ToStr.Text = Result;
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
                int Total = _Owner.TransListView.RealLines.Count;

                for (int i = 0; i < _Owner.TransListView.RealLines.Count; i++)
                {
                    if (_Owner.TransListView.RealLines[i].Score <= 0)
                    {
                        continue;
                    }
                    string Source = _Owner.TransListView.RealLines[i].SourceText;
                    var Result = ChineseVariantMap.SimplifiedToTraditionalByReq(Source);

                    if (Source.ToLower().Replace(" ","") != Result.ToLower().Replace(" ", ""))
                    {
                        _Owner.TransListView.RealLines[i].TransText = Result;

                        var Key = _Owner.TransListView.RealLines[i].Key;

                        var Link = TranslatorInterface.Instance.GetLink();

                        Link[Key] = Result;

                        _Owner.TransListView.RealLines[i].SyncUI(_Owner.TransListView);

                        CloudDBCache.AddCache(TranslatorInterface.Instance.GetFileUniqueKey(), Key, (int)TranslatorInterface.Instance.To, Source, Result);
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

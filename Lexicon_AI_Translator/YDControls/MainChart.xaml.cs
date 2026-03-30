using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LexTranslator.YDControls
{
    /// <summary>
    /// Interaction logic for MainChart.xaml
    /// </summary>
    public partial class MainChart : UserControl
    {
        private bool _Paused = false;

        public MainChart()
        {
            InitializeComponent();
        }

        private void BtnPause_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _Paused = !_Paused;
            BtnPause.Content = _Paused ? "▶  RESUME" : "⏸  PAUSE";
        }

        private void BtnClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TokenChart.Clear();
        }

        public long TotalToken = 0;
        public double CurrentToken = 0;
        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //TokenChart.OnTick += new Action<RealtimeLineChart>((Ref) =>
            //{
            //    if (!_Paused)
            //    {
            //        var GenValue = new Random(Guid.NewGuid().GetHashCode()).Next(100, 99000);
            //        CurrentToken = GenValue;
            //        TotalToken += Convert.ToInt64(GenValue);
            //        Ref.PushValue(GenValue);
            //    }
           
            //});
            //TokenChart.Start();

            //TotalTokenChart.OnTick += new Action<RealtimeLineChart>((Ref) =>
            //{
            //    Ref.PushValue(TotalToken);

            //    Tokens.Content = string.Format("Current:{0}", CurrentToken);

            //    if (CurrentToken > 99000 * 0.7)
            //    {
            //        Tokens.Foreground = new SolidColorBrush(Colors.Red);
            //    }
            //    else
            //    {
            //        Tokens.Foreground = new SolidColorBrush(Colors.White);
            //    }
               
            //});
            //TotalTokenChart.Start();

        }
    }
}

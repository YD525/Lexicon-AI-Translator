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

        public void SetAction(Action<RealtimeLineChart> Current, Action<RealtimeLineChart> Total)
        {
            if (Total != null)
            {
                TotalTokenChart.OnTick += Total;
                TotalTokenChart.Start();
            }

            if (Current != null)
            {
                TokenChart.OnTick += Current;
                TokenChart.Start();
            }
        }
    }
}

using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LexTranslator.UIManagement;

namespace LexTranslator.YDControls
{
    /// <summary>
    /// Interaction logic for MainChart.xaml
    /// </summary>
    public partial class MainChart : UserControl
    {
        public MainChart()
        {
            InitializeComponent();
        }

        private void BtnPause_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DataRef.Paused = !DataRef.Paused;
            BtnPause.Content = DataRef.Paused ? "▶  RESUME" : "⏸  PAUSE";
        }

        private void BtnClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TokenChart.Clear();
            TotalTokenChart.Clear();
        }

        public ChartData DataRef = null;

        public void SetAction(Action<RealtimeLineChart> Current, Action<RealtimeLineChart> Total,ChartData DataRef)
        {
            if (Current != null)
            {
                TokenChart.OnTick += new Action<RealtimeLineChart>((Ref) =>
                {
                    if(!DataRef.Paused)
                    Current.Invoke(Ref);
                });
                TokenChart.Start();
            }

            if (Total != null)
            {
                TotalTokenChart.OnTick += new Action<RealtimeLineChart>((Ref) =>
                {
                    if (!DataRef.Paused)
                    Total.Invoke(Ref);
                });
                TotalTokenChart.Start();
            }

            this.DataRef = DataRef;
        }
    }
}

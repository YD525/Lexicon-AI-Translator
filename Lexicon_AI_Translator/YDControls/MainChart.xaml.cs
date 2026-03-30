using System;
using System.Windows.Controls;

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

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TokenChart.OnTick += new System.Action<RealtimeLineChart>((Ref) =>
            {
                if(!_Paused)
                Ref.PushValue(new Random(Guid.NewGuid().GetHashCode()).Next(100,99000));
            });
            TokenChart.Start();
        }
    }
}

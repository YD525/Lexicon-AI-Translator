using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LexTranslator.UIManagement;
using Newtonsoft.Json.Linq;

namespace LexTranslator.YDControls
{
    /// <summary>
    /// Interaction logic for TestWin.xaml
    /// </summary>
    public partial class TestWin : Window
    {
        public TestWin()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChartData DataRef = new ChartData();

            YDChart.SetAction(
                new Action<RealtimeLineChart>((Ref) =>
                {
                    Ref.PushValue(DataRef.SetCurrent(
                        new Random(Guid.NewGuid().GetHashCode()).Next(1000, 9999)
                        ));
                }),
                new Action<RealtimeLineChart>((Ref) =>
                {
                    Ref.PushValue(DataRef.Total);
                }),
                DataRef
                );
        }
    }
}

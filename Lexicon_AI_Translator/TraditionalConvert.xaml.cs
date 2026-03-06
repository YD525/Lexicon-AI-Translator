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
    }
}

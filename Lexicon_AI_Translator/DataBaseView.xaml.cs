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
using PhoenixEngine;
using PhoenixEngine.ADO;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for DataBaseView.xaml
    /// </summary>
    public partial class DataBaseView : Window
    {
        public DataBaseView()
        {
            InitializeComponent();
        }

        private void QueryDataBase(object sender, RoutedEventArgs e)
        {
            try
            { 
                var Result = Phoenix.LocalDB.P_ExecuteQuery(SqlOrder.Text);
            }
            catch(Exception Ex) 
            {
                MessageBoxExtend.Show(this, Ex.Message);
            }
        }
    }
}

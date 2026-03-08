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
using LexTranslator.SkyrimManagement;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for NPCFinder.xaml
    /// </summary>
    public partial class NPCFinder : Window
    {
        public NPCFinder()
        {
            InitializeComponent();
        }

        public string SearchName = "";

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < DeFine.WorkingWin.TransViewList.RealLines.Count; i++)
            {
                var Key = DeFine.WorkingWin.TransViewList.RealLines[i].Key;
                if (EspReader.GameCharacters.ContainsKey(Key))
                {
                    if (EspReader.GameCharacters[Key].Equals(SearchName))
                    {
                        DeFine.WorkingWin.TransViewList.Goto(Key);
                        return;
                    }
                }
            }
        }
    }
}

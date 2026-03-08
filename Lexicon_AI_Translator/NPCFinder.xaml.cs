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
using LexTranslator.ConvertManager;
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
                    if (EspReader.GameCharacters[Key][0].Name.Equals(SearchName))
                    {
                        DeFine.WorkingWin.TransViewList.Goto(Key);
                        return;
                    }
                }
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            for (int i = 0; i < NameCache.Count; i++)
            {
                if (NameCache[i].StartsWith(NpcName.Text))
                {
                    NpcNames.SelectedValue = NameCache[i];
                    break;
                }
                else
                if (NameCache[i].Equals(NpcName.Text))
                {
                    NpcNames.SelectedValue = NameCache[i];
                    break;
                }
            }
        }
        public List<string> NameCache = new List<string>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NpcNames.Items.Clear();

            foreach (var GetNpc in EspReader.GameCharacters.ToList())
            {
                var Name = GetNpc.Value[0].Name;

                if (!NameCache.Contains(Name))
                {
                    NameCache.Add(Name);
                    NpcNames.Items.Add(Name);
                }
            }
        }

        private void NpcNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchName = ConvertHelper.ObjToStr(NpcNames.SelectedValue);
        }
    }
}

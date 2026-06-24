using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LexTranslator.SkyrimManagement;
using PhoenixEngine.Common;

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
        public EspReader EspInstance = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < DeFine.WorkingWin.TransViewList.RealLines.Count; i++)
            {
                var Key = DeFine.WorkingWin.TransViewList.RealLines[i].Key;
                if (EspInstance.GameCharacters.ContainsKey(Key))
                {
                    if (EspInstance.GameCharacters[Key][0].Name.Equals(SearchName))
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

            foreach (var GetNpc in EspInstance.GameCharacters.ToList())
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
            SearchName = P_Convert.ObjToStr(NpcNames.SelectedValue);
        }
    }
}

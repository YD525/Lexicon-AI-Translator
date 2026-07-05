using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LexTranslator
{
    /// <summary>
    /// Interaction logic for SearchText.xaml
    /// </summary>
    public partial class SearchText : Window
    {
        public SearchText()
        {
            InitializeComponent();
        }


        int Line = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool IsFind = false;
            for (int i = Line; i < DeFine.ActiveIDE.LineCount; i++)
            {
                var GetLineStr = DeFine.ActiveIDE.Text.Substring(DeFine.ActiveIDE.Document.Lines[i].Offset,DeFine.ActiveIDE.Document.Lines[i].Length);
                if (GetLineStr.Contains(Search.Text))
                {
                    DeFine.ActiveIDE.ScrollToLine(i);
                    int GetOffset = GetLineStr.IndexOf(Search.Text);
                    DeFine.ActiveIDE.Select(DeFine.ActiveIDE.Document.Lines[i].Offset + GetOffset, Search.Text.Length);

                    Line = i + 1;
                    IsFind = true;
                    break;
                }
            }
            if (!IsFind)
            {
                Line = 0;
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            Line = 0;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(null,null);
            }
        }
    }
}

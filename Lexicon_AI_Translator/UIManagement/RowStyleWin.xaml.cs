using System.Windows;
using System.Windows.Controls;
using LexTranslator.SkyrimManage;
using System.Windows.Input;
using System.Windows.Media;
using PhoenixEngine.TranslateManagement;
using PhoenixEngine.ConvertManager;
using PhoenixEngine.EngineManagement;
using System.Windows.Shapes;
using LexTranslator.TranslateManage;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using LexTranslator.SkyrimManagement;
using LexTranslator.UIManage;
using Newtonsoft.Json.Linq;

namespace LexTranslator.UIManagement
{
    /// <summary>
    /// Interaction logic for RowStyleWin.xaml
    /// </summary>
    public partial class RowStyleWin : Window
    {
        public RowStyleWin()
        {
            InitializeComponent();
            this.Hide();
        }

    
        public static void SetColor(Grid Grid,int R,int G,int B)
        {
            Color FontColor = Color.FromRgb((byte)R, (byte)G, (byte)B);

            Grid MainGrid = Grid;

            Border MainBorder = (Border)MainGrid.Children[0];

            Grid GetChildGrid = (Grid)MainBorder.Child;

            StackPanel GetStackPanel = (StackPanel)((Grid)GetChildGrid.Children[0]).Children[0];

            Label GetType = (Label)GetStackPanel.Children[1];
            GetType.Foreground = new SolidColorBrush(FontColor);

            Grid GetKeyGrid = (Grid)GetChildGrid.Children[1];
            TextBox GetKey = (TextBox)GetKeyGrid.Children[0];
            TextBox GetFakeKey = (TextBox)(GetKeyGrid.Children[1] as StackPanel).Children[0];
            GetKey.Foreground = new SolidColorBrush(FontColor);
            GetFakeKey.Foreground = new SolidColorBrush(FontColor);

            Grid GetOriginalGrid = (Grid)GetChildGrid.Children[2];
            TextBox GetOriginal = (TextBox)GetOriginalGrid.Children[0];
            GetOriginal.Foreground = new SolidColorBrush(FontColor);

            Grid GetTranslatedGrid = (Grid)GetChildGrid.Children[3];
            Border GetTranslatedBorder = (Border)GetTranslatedGrid.Children[0];

            TextEditor GetTranslated = (TextEditor)(GetTranslatedBorder.Child);
            GetTranslated.Foreground = new SolidColorBrush(FontColor);
        }

        public static string GetType(Grid Grid)
        {
            Grid GetDataGrid = ((Grid)((Border)Grid.Children[0]).Child);
            StackPanel GetStackPanel = (StackPanel)(((Grid)GetDataGrid.Children[0]).Children[0]);
            Label GetType = (Label)GetStackPanel.Children[1];

            return ConvertHelper.ObjToStr(GetType.Content);
        }

        public static string GetKey(Grid Grid)
        {
            string GetKey = "";

            Grid.Dispatcher.Invoke(new Action(() => {
                GetKey = ConvertHelper.ObjToStr(((Border)Grid.Children[0]).Tag);
            }));

            return GetKey;
        }

        public static void MarkLeader(Grid Grid,bool Visible = true)
        {
            Grid GetDataGrid = ((Grid)((Border)Grid.Children[0]).Child);
            Grid GetKeyGrid = (Grid)GetDataGrid.Children[1];
            StackPanel GetKeyPanel = GetKeyGrid.Children[1] as StackPanel;

            if (GetKeyPanel != null && GetKeyPanel.Children.Count > 1)
            {
                Grid GetLeader = (Grid)(GetKeyPanel).Children[1];
                GetLeader.Visibility = Visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static void SetOriginal(Grid Grid,string Text)
        {
            Grid GetDataGrid = ((Grid)((Border)Grid.Children[0]).Child);

            Grid GetOriginalGrid = (Grid)GetDataGrid.Children[2];

            TextBox GetOriginal = (TextBox)GetOriginalGrid.Children[0];

            GetOriginal.Text = Text;
        }

        public static string GetOriginal(Grid Grid)
        {
            Grid GetDataGrid = ((Grid)((Border)Grid.Children[0]).Child);

            Grid GetOriginalGrid = (Grid)GetDataGrid.Children[2];

            TextBox GetOriginal = (TextBox)GetOriginalGrid.Children[0];

            return GetOriginal.Text;
        }

        public static string GetTranslated(Grid Grid)
        {
            Grid GetDataGrid = ((Grid)((Border)Grid.Children[0]).Child);

            Grid GetTranslatedGrid = (Grid)GetDataGrid.Children[3];

            Border GetTranslatedBorder = (Border)GetTranslatedGrid.Children[0];

            TextEditor GetTranslated = (TextEditor)(GetTranslatedBorder.Child);

            return GetTranslated.Text;
        }

        public static void SetTranslated(Grid Grid,string Translated)
        {
            Grid SetDataGrid = ((Grid)((Border)Grid.Children[0]).Child);

            Grid SetTranslatedGrid = (Grid)SetDataGrid.Children[3];

            Border SetTranslatedBorder = (Border)SetTranslatedGrid.Children[0];

            TextEditor SetTranslated = (TextEditor)(SetTranslatedBorder.Child);

            SetTranslated.Text = Translated;
        }

        public ColumnDefinition GetColorCol(Grid Grid)
        {
            Grid SetDataGrid = ((Grid)((Border)Grid.Children[0]).Child);

            Grid SetTranslatedGrid = (Grid)SetDataGrid.Children[3];

            return SetTranslatedGrid.ColumnDefinitions[1];
        }

        public static List<string> RecordModifyStates = new List<string>();

        public Grid CreateLine(bool IsModify,double Height, BaseUnit Item)
        {

            var QueryTranslated = TranslatorInterface.Instance.QueryTransData(Item.Key);

            if (QueryTranslated != null)
            {
                Item.Translated = QueryTranslated.TransText;
            }

            var FindDictionary = YDDictionaryHelper.CheckDictionary(Item.Key);

            if (FindDictionary != null)
            {
                if (FindDictionary.OriginalText.Trim().Length > 0)
                {
                    if (Item.Original != FindDictionary.OriginalText)
                    {
                        Item.Original = FindDictionary.OriginalText;

                        //11 116 209
                        IsModify = true;

                        if (!RecordModifyStates.Contains(Item.Key))
                        {
                            RecordModifyStates.Add(Item.Key);
                        }
                    }
                }
            }

            Color FontColor = Colors.White;

            var QueryColor = FontColorFinder.FindColor(Phoenix.GetFileUniqueKey(), Item.Key);

            if (QueryColor != null)
            {
                FontColor = Color.FromRgb((byte)QueryColor.R, (byte)QueryColor.G, (byte)QueryColor.B);
            }

            Grid MainGrid = UIHelper.CloneElement(LineGrid);

            if (MainGrid == null)
            {
                return new Grid();
            }

            MainGrid.Height = Height;

            Border MainBorder = (Border)MainGrid.Children[0];
            MainBorder.Tag = Item.Key;

            Grid GetChildGrid = (Grid)MainBorder.Child;

            GetChildGrid.ColumnDefinitions[0].Width = DeFine.WorkingWin.TransViewHeader.ColumnDefinitions[0].Width;
            GetChildGrid.ColumnDefinitions[1].Width = DeFine.WorkingWin.TransViewHeader.ColumnDefinitions[1].Width;
            GetChildGrid.ColumnDefinitions[2].Width = DeFine.WorkingWin.TransViewHeader.ColumnDefinitions[2].Width;
            GetChildGrid.ColumnDefinitions[3].Width = DeFine.WorkingWin.TransViewHeader.ColumnDefinitions[3].Width;

            StackPanel GetStackPanel = (StackPanel)((Grid)GetChildGrid.Children[0]).Children[0];

            Ellipse State = (Ellipse)GetStackPanel.Children[0];

            if (IsModify || RecordModifyStates.Contains(Item.Key))
            {
                State.Fill = new SolidColorBrush(Color.FromRgb(11, 116, 209));
            }

            Label GetType = (Label)GetStackPanel.Children[1];
            GetType.Content = Item.Type;

            if (FontColor == Colors.White)
            { 
               FontColor = (Color)Application.Current.Resources["DefFontColor"];
            }
            GetType.Foreground = new SolidColorBrush(FontColor);

            Grid GetKeyGrid = (Grid)GetChildGrid.Children[1];
            TextBox GetKey = (TextBox)GetKeyGrid.Children[0];
            GetKey.Text = Item.Key;

            if (FontColor == Colors.White)
            { 
               FontColor = (Color)Application.Current.Resources["DefFontColor"];
            }
            GetKey.Foreground = new SolidColorBrush(FontColor);

            GetKey.PreviewMouseWheel += OnePreviewMouseWheel;

            StackPanel GetKeyPanel = GetKeyGrid.Children[1] as StackPanel;
            TextBox GetFakeKey = (TextBox)(GetKeyPanel).Children[0];

            if (DeFine.WorkingWin.CurrentTransType == 2 && EspReader.Records.ContainsKey(Item.Key))
            {
                GetFakeKey.Text = EspReader.Records[Item.Key].FormID + " " + EspReader.Records[Item.Key].ChildSig;
            }
            else
            {
                GetFakeKey.Text = Item.Key;
            }

            if (TranslatorInterface.Instance != null)
            {
                var BatchCore = TranslatorInterface.Instance.GetBatchCore();
                if (BatchCore != null)
                {
                    if (BatchCore.Content != null)
                    {
                        if (BatchCore.Content.UnionData.Leaders.ContainsKey(Item.Key))
                        {
                            Grid GetLeader = (Grid)(GetKeyPanel).Children[1];
                            GetLeader.Visibility = Visibility.Visible;
                        }
                    }
                }
               
            }

            GetFakeKey.Foreground = new SolidColorBrush(FontColor);
            
            GetFakeKey.PreviewMouseWheel += OnePreviewMouseWheel;

            Grid GetOriginalGrid = (Grid)GetChildGrid.Children[2];
            TextBox GetOriginal = (TextBox)GetOriginalGrid.Children[0];
            GetOriginal.Text = Item.Original;

            if (FontColor == Colors.White)
            {
                FontColor = (Color)Application.Current.Resources["DefFontColor"];
            }
            GetOriginal.Foreground = new SolidColorBrush(FontColor);

            GetOriginal.PreviewMouseWheel += OnePreviewMouseWheel;

            Grid GetTranslatedGrid = (Grid)GetChildGrid.Children[3];
            Border GetTranslatedBorder = (Border)GetTranslatedGrid.Children[0];

            Grid GetColorGrid = (Grid)GetTranslatedGrid.Children[1];

            ((Border)GetColorGrid.Children[0]).PreviewMouseDown += ChangeColor;
            ((Border)GetColorGrid.Children[1]).PreviewMouseDown += ChangeColor;
            ((Border)GetColorGrid.Children[2]).PreviewMouseDown += ChangeColor;

            TextEditor GetTranslated = (TextEditor)(GetTranslatedBorder.Child);

            GetTranslated.TextArea.LeftMargins.Clear();

            GetTranslated.Text = Item.Translated;

            if (FontColor == Colors.White)
            { 
               FontColor = (Color)Application.Current.Resources["DefFontColor"];
            }
            GetTranslated.Foreground = new SolidColorBrush(FontColor);

            GetTranslated.PreviewMouseWheel += TextEditorPreviewMouseWheel;

            GetTranslated.MouseLeave += GetTranslated_MouseLeave;
            GetTranslated.LostFocus += GetTranslated_LostFocus;

            GetTranslated.Tag = Item.Key;

            GetTranslated.TextArea.Caret.CaretBrush = Brushes.Orange;

            if (DeFine.GlobalLocalSetting.ViewMode == "Normal")
            {
                MainGrid.Cursor = Cursors.Hand;
                GetKey.Cursor = Cursors.Hand;
                GetOriginal.Cursor = Cursors.Hand;
                GetTranslated.Cursor = Cursors.Hand;
                GetTranslatedBorder.Background = null;
                GetTranslatedBorder.Cursor = null;

                GetTranslated.IsReadOnly = true;
                GetTranslated.VerticalContentAlignment = VerticalAlignment.Center;

                GetTranslated.TextArea.Caret.Hide();
                GetTranslated.IsHitTestVisible = false;

                ApplyLTROrRtl(GetTranslated);
            }
            else
            {
                MainGrid.Cursor = Cursors.Hand;
                GetKey.Cursor = Cursors.Hand;
                GetOriginal.Cursor = Cursors.Hand;
                GetTranslated.Cursor = Cursors.IBeam;
                GetTranslated.IsReadOnly = false;              
                GetTranslated.VerticalContentAlignment = VerticalAlignment.Center;

                ApplyLTROrRtl(GetTranslated);
            }

            if (Item.Score < 0)
            {
                GetKey.Foreground = new SolidColorBrush(Colors.Red);
                GetOriginal.Foreground = new SolidColorBrush(Colors.Red);
                GetTranslated.Foreground = new SolidColorBrush(Colors.Red);
                GetTranslatedBorder.Visibility = Visibility.Collapsed;
                GetFakeKey.Foreground = new SolidColorBrush(Colors.Red);

                GetTranslated.IsReadOnly = true;
            }

            return MainGrid;
        }

        private void GetTranslated_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveText((TextEditor)sender);
        }

        void ApplyLTROrRtl(TextEditor Box)
        {
            Box.HorizontalAlignment = HorizontalAlignment.Stretch;
            Box.VerticalContentAlignment = VerticalAlignment.Center;

            if (DeFine.GlobalLocalSetting.TextDisplay == UIManage.TextLayout.RTL)
            {
                Box.FlowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                Box.FlowDirection = FlowDirection.LeftToRight;
            }
        }

        private void GetTranslated_MouseLeave(object sender, MouseEventArgs e)
        {
            SaveText((TextEditor)sender);
        }

        public void SaveText(TextEditor RTB)
        {
            // Skip if In Normal View Mode Or Working Window / TransViewList Is Null
            if (DeFine.GlobalLocalSetting.ViewMode == "Normal" ||
                DeFine.WorkingWin == null ||
                DeFine.WorkingWin.TransViewList == null) return;
            try
            {
                string OriginalText = RTB.Text;
               
                // Get Key And Target Grid
                string Key = ConvertHelper.ObjToStr(RTB.Tag);
                var Target = DeFine.WorkingWin.TransViewList.KeyToFakeGrid(Key);

                // Update Translation Data And History Cache
                if (Target != null)
                {
                    TranslatorInterface.Instance.AutoSetLink(Key, Target.SourceText, OriginalText);
                    bool IsCloud = false;
                    Target.SyncData(ref IsCloud);
                    TranslatorInterface.SetTranslatorHistoryCache(Key, OriginalText, IsCloud);
                }

                // Apply LTR Or RTL Layout
                ApplyLTROrRtl(RTB);
            }
            finally
            {
                
            }
        }

        public static Thread AutoSelectIDETrd = null;
     
        public static void SelectLineFromIDE(int LineID, string Value)
        {
            if (DeFine.ActiveIDE == null)
            {
                return;
            }
            if (DeFine.WorkingWin.CodeViewShowState!=1)
            {
                return;
            }

            if (AutoSelectIDETrd != null)
            {
                try 
                {
                    AutoSelectIDETrd.Abort();
                }
                catch { }
                AutoSelectIDETrd = null;
            }

            Value = "\"" + Value + "\"";

            AutoSelectIDETrd = new Thread(() =>
            {
                try
                {
                    DeFine.ActiveIDE.Dispatcher.Invoke(() =>
                    {
                        var Editor = DeFine.ActiveIDE;
                        var Doc = Editor.Document;

                        int TotalLines = Doc.LineCount;

                        for (int i = LineID; i <= TotalLines; i++)
                        {
                            var Line = Doc.GetLineByNumber(i);
                            string Text = Doc.GetText(Line);

                            int Index = Text.IndexOf(Value, StringComparison.OrdinalIgnoreCase);

                            if (Index >= 0)
                            {
                                int Offset = Line.Offset + Index;

                                Editor.ScrollToLine(i);
                                Editor.Select(Offset, Value.Length);
                                Editor.CaretOffset = Offset + Value.Length;
                                Editor.Focus();

                                break;
                            }
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                }

                AutoSelectIDETrd = null;
            });


            AutoSelectIDETrd.Start();
        }

        public void OnePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var TextBox = sender as TextBox;
            var Parent = VisualTreeHelper.GetParent(TextBox);

            while (Parent != null && !(Parent is ScrollViewer))
            {
                Parent = VisualTreeHelper.GetParent(Parent);
            }

            if (Parent is ScrollViewer ScrollViewer)
            {
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true; 
            }
        }

        public void TextEditorPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var TextBox = sender as TextEditor;
            var Parent = VisualTreeHelper.GetParent(TextBox);

            while (Parent != null && !(Parent is ScrollViewer))
            {
                Parent = VisualTreeHelper.GetParent(Parent);
            }

            if (Parent is ScrollViewer ScrollViewer)
            {
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        public void ChangeColor(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border ButtonHandle = (Border)sender;
                Color GetColor = ((SolidColorBrush)ButtonHandle.Background).Color;

                if (DeFine.WorkingWin.TransViewList != null)
                {
                    DeFine.WorkingWin.TransViewList.ChangeFontColor(Phoenix.GetFileUniqueKey(), GetColor.R, GetColor.G, GetColor.B);
                }
            }
        }
    }
}

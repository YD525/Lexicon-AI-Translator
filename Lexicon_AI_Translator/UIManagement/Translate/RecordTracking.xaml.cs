using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LexTranslator.SkyrimManage;
using LexTranslator.SkyrimManagement;
using LexTranslator.UIManagement;
using PhoenixEngine.Translate;

namespace LexTranslator
{
    public class TrackingItem
    {
        public ModFile ModRef;
        public string Key = "";
        public TrackingItem(ModFile ModRef, string Key)
        {
            this.ModRef = ModRef;
            this.Key = Key;
        }
    }
    public partial class RecordTracking : Window
    {
        private Window _Owner;

        private bool _NpcExpanded = true;
        private bool _RelatedTextExpanded = true;
        private bool _DialogueExpanded = true;

        public ModFile ModRef = null;

        public RecordTracking(ModFile Mod,Window Owner)
        {
            InitializeComponent();

            _Owner = Owner;
            this.ModRef = Mod;

            this.Owner = _Owner;
            this.Loaded += RecordTracking_Loaded;
            this.Closed += RecordTracking_Closed;

            _Owner.LocationChanged += OwnerMainWindow_LocationChanged;
            _Owner.SizeChanged += OwnerMainWindow_SizeChanged;
            _Owner.StateChanged += OwnerMainWindow_StateChanged;
            _Owner.Closed += OwnerMainWindow_Closed;
        }

        private void RecordTracking_Loaded(object Sender, RoutedEventArgs E)
        {
            UpdateFollowPosition();
        }

        private void OwnerMainWindow_LocationChanged(object Sender, EventArgs E)
        {
            UpdateFollowPosition();
        }

        private void OwnerMainWindow_SizeChanged(object Sender, SizeChangedEventArgs E)
        {
            UpdateFollowPosition();
        }

        public void UpdateAllSectionHeights()
        {
            NpcRow.BeginAnimation(RowDefinition.HeightProperty, null);
            NpcChevronRotate.BeginAnimation(RotateTransform.AngleProperty, null);
            bool hasNpc = NpcListPanel.Children.Count > 0;
            NpcRow.Height = new GridLength(hasNpc ? 1 : 0, GridUnitType.Star);
            NpcChevronRotate.Angle = hasNpc ? 0 : 180;
            _NpcExpanded = hasNpc;

            RelatedTextRow.BeginAnimation(RowDefinition.HeightProperty, null);
            RelatedTextChevronRotate.BeginAnimation(RotateTransform.AngleProperty, null);
            bool hasRelated = RelatedTextListPanel.Children.Count > 0;
            RelatedTextRow.Height = new GridLength(hasRelated ? 1 : 0, GridUnitType.Star);
            RelatedTextChevronRotate.Angle = hasRelated ? 0 : 180;
            _RelatedTextExpanded = hasRelated;

            DialogueRow.BeginAnimation(RowDefinition.HeightProperty, null);
            DialogueChevronRotate.BeginAnimation(RotateTransform.AngleProperty, null);
            bool hasDialogue = DialogueListPanel.Children.Count > 0;
            DialogueRow.Height = new GridLength(hasDialogue ? 1 : 0, GridUnitType.Star);
            DialogueChevronRotate.Angle = hasDialogue ? 0 : 180;
            _DialogueExpanded = hasDialogue;
        }
        private void OwnerMainWindow_StateChanged(object Sender, EventArgs E)
        {
            if (_Owner.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            else
            {
                this.Show();
                UpdateFollowPosition();
            }
        }

        private void OwnerMainWindow_Closed(object Sender, EventArgs E)
        {
            MultiWindowController.TrackingWin = null;
            this.Close();
        }

        private void RecordTracking_Closed(object Sender, EventArgs E)
        {
            _Owner.LocationChanged -= OwnerMainWindow_LocationChanged;
            _Owner.SizeChanged -= OwnerMainWindow_SizeChanged;
            _Owner.StateChanged -= OwnerMainWindow_StateChanged;
            _Owner.Closed -= OwnerMainWindow_Closed;
        }

        private void UpdateFollowPosition()
        {
            double Gap = 8;

            this.Left = _Owner.Left + _Owner.ActualWidth + Gap;
            this.Top = _Owner.Top;
            this.Height = _Owner.ActualHeight;
        }

        //Section collapse / expand

        private void NpcHeader_PreviewMouseDown(object Sender, MouseButtonEventArgs E)
        {
            ToggleSection(NpcContentHost, NpcRow, NpcChevronRotate, ref _NpcExpanded);
        }

        private void RelatedTextHeader_PreviewMouseDown(object Sender, MouseButtonEventArgs E)
        {
            ToggleSection(RelatedTextContentHost, RelatedTextRow, RelatedTextChevronRotate, ref _RelatedTextExpanded);
        }

        private void DialogueHeader_PreviewMouseDown(object Sender, MouseButtonEventArgs E)
        {
            ToggleSection(DialogueContentHost, DialogueRow, DialogueChevronRotate, ref _DialogueExpanded);
        }

        private void ToggleSection(Border ContentHost, RowDefinition Row, RotateTransform ChevronRotate, ref bool IsExpanded)
        {
            if (IsExpanded)
            {
                CollapseSection(Row, ChevronRotate);
            }
            else
            {
                ExpandSection(Row, ChevronRotate);
            }

            IsExpanded = !IsExpanded;
        }

        private void CollapseSection(RowDefinition Row, RotateTransform ChevronRotate)
        {
            Row.BeginAnimation(RowDefinition.HeightProperty, null);
            ChevronRotate.BeginAnimation(RotateTransform.AngleProperty, null);

            GridLengthAnimation HeightAnimation = new GridLengthAnimation();
            HeightAnimation.From = Row.Height;
            HeightAnimation.To = new GridLength(0, GridUnitType.Star);
            HeightAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(220));
            HeightAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
            HeightAnimation.FillBehavior = FillBehavior.HoldEnd;

            DoubleAnimation RotateAnimation = new DoubleAnimation();
            RotateAnimation.To = 180;
            RotateAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(220));
            RotateAnimation.FillBehavior = FillBehavior.HoldEnd;

            Row.BeginAnimation(RowDefinition.HeightProperty, HeightAnimation);
            ChevronRotate.BeginAnimation(RotateTransform.AngleProperty, RotateAnimation);
        }
        private void ExpandSection(RowDefinition Row, RotateTransform ChevronRotate)
        {
            Row.BeginAnimation(RowDefinition.HeightProperty, null);
            ChevronRotate.BeginAnimation(RotateTransform.AngleProperty, null);

            GridLengthAnimation HeightAnimation = new GridLengthAnimation();
            HeightAnimation.From = Row.Height;
            HeightAnimation.To = new GridLength(1, GridUnitType.Star);
            HeightAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(220));
            HeightAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
            HeightAnimation.FillBehavior = FillBehavior.HoldEnd;

            DoubleAnimation RotateAnimation = new DoubleAnimation();
            RotateAnimation.To = 0;
            RotateAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(220));
            RotateAnimation.FillBehavior = FillBehavior.HoldEnd;

            Row.BeginAnimation(RowDefinition.HeightProperty, HeightAnimation);
            ChevronRotate.BeginAnimation(RotateTransform.AngleProperty, RotateAnimation);
        }

        public void LoadNpcRecord(RecordItem Record, string NpcName, string Gender)
        {
            NpcListPanel.Children.Clear();

            bool HasData = Record != null;

            if (HasData)
            {
                NpcListPanel.Children.Add(BuildNpcCard(Record, NpcName, Gender));
            }
        }

        public void LoadRelatedTextRecords(List<RecordItem> Records)
        {
            RelatedTextListPanel.Children.Clear();

            int Count = Records != null ? Records.Count : 0;
            bool HasData = Count > 0;

            if (HasData)
            {
                for (int i = 0; i < Records.Count; i++)
                {
                    RelatedTextListPanel.Children.Add(BuildRelatedTextCard(Records[i]));
                }
            }
        }

        public void LoadDialogueRecords(ModFile ModRef, List<ManagedDialNode> Records)
        {
            DialogueListPanel.Children.Clear();

            int Count = Records != null ? Records.Count : 0;
            bool HasData = Count > 0;

            if (HasData)
            {
                for (int i = 0; i < Records.Count; i++)
                {
                    var GetLine = BuildDialogueCard(ModRef, Records[i]);
                    if (GetLine != null)
                    {
                        DialogueListPanel.Children.Add(GetLine);
                    }
                }
            }
        }

        //Card builders

        private Border BuildNpcCard(RecordItem Item, string NpcName, string Gender)
        {
            Border CardBorder = new Border();
            CardBorder.Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            CardBorder.CornerRadius = new CornerRadius(6);
            CardBorder.Margin = new Thickness(0, 0, 0, 6);
            CardBorder.Padding = new Thickness(8, 6, 8, 6);

            StackPanel ContentPanel = new StackPanel();
            ContentPanel.Orientation = Orientation.Vertical;

            TextBlock TextLine = new TextBlock();
            TextLine.Text = Item.String;
            TextLine.Foreground = Brushes.White;
            TextLine.FontSize = 13;
            TextLine.TextWrapping = TextWrapping.Wrap;
            ContentPanel.Children.Add(TextLine);

            StackPanel InfoLine = new StackPanel();
            InfoLine.Orientation = Orientation.Horizontal;
            InfoLine.Margin = new Thickness(0, 4, 0, 0);

            TextBlock NameText = new TextBlock();
            NameText.Text = NpcName;
            NameText.Foreground = new SolidColorBrush(Color.FromRgb(0xFA, 0xE3, 0x06));
            NameText.FontSize = 12;
            NameText.FontWeight = FontWeights.DemiBold;
            InfoLine.Children.Add(NameText);

            TextBlock GenderText = new TextBlock();
            GenderText.Text = "  (" + Gender + ")";
            GenderText.Foreground = new SolidColorBrush(Color.FromRgb(0xBF, 0xBF, 0xBF));
            GenderText.FontSize = 12;
            InfoLine.Children.Add(GenderText);

            ContentPanel.Children.Add(InfoLine);
            CardBorder.Child = ContentPanel;

            return CardBorder;
        }

        public string FindTranslated(string Key,string Type,string SourceText,ModFile Mod)
        {
            var FindDictionary = new LexDictionary().CheckDictionary(Key);

            if (FindDictionary != null)
            {
                if (!string.IsNullOrEmpty(FindDictionary.OriginalText))
                {
                    SourceText = FindDictionary.OriginalText;
                }
            }

            var QueryResult = Mod.P_Translator.QueryTransData(Key,Type,SourceText, true);

            if (QueryResult != null)
            {
                return QueryResult.TransText;
            }

            return string.Empty;
        }


        private Border BuildRelatedTextCard(RecordItem Item)
        {
            var GetTranslated = FindTranslated(Item.UniqueKey,Item.ParentSig,Item.String,ModRef);

            Border CardBorder = new Border();
            CardBorder.Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            CardBorder.CornerRadius = new CornerRadius(6);
            CardBorder.Margin = new Thickness(0, 0, 0, 6);
            CardBorder.Padding = new Thickness(8, 6, 8, 6);
            CardBorder.Tag = new TrackingItem(ModRef, Item.UniqueKey);
            CardBorder.PreviewMouseDown += AnyCard_PreviewMouseDown;

            StackPanel ContentPanel = new StackPanel();
            ContentPanel.Orientation = Orientation.Vertical;

            TextBox TextLine = new TextBox();
            TextLine.Background = null;
            TextLine.BorderBrush = null;
            TextLine.BorderThickness = new Thickness(0);
            TextLine.IsReadOnly = true;
            TextLine.Foreground = Brushes.White;
            TextLine.FontSize = 13;
            TextLine.TextWrapping = TextWrapping.Wrap;
            TextLine.Cursor = Cursors.Hand;

            if (GetTranslated.Length == 0)
            {
                TextLine.Text = Item.String;
            }
            else
            {
                TextLine.Text = Item.String + " -> " + GetTranslated;
            }

            ContentPanel.Children.Add(TextLine);

            StackPanel InfoLine = new StackPanel();
            InfoLine.Orientation = Orientation.Horizontal;
            InfoLine.Margin = new Thickness(0, 4, 0, 0);

            TextBlock InFoText = new TextBlock();
            InFoText.Text = Item.ParentSig + " " + Item.ChildSig;
            InFoText.Foreground = new SolidColorBrush(Color.FromRgb(0xFA, 0xE3, 0x06));
            InFoText.FontSize = 12;
            InFoText.FontWeight = FontWeights.DemiBold;
            InfoLine.Children.Add(InFoText);

            TextBlock ResponseIdText = new TextBlock();
            ResponseIdText.Text = "  #" + Item.UniqueKey;
            ResponseIdText.Foreground = new SolidColorBrush(Color.FromRgb(0xBF, 0xBF, 0xBF));
            ResponseIdText.FontSize = 12;
            InfoLine.Children.Add(ResponseIdText);

            ContentPanel.Children.Add(InfoLine);
            CardBorder.Child = ContentPanel;

            return CardBorder;
        }

        private Border BuildDialogueCard(ModFile ModRef, ManagedDialNode Item)
        {
            var GetRecord = ModRef.EspReader.GetRecordItemByOffsets(Item.RecordOffset, Item.SubOffset);
            if (GetRecord != null)
            {
                var GetTranslated = FindTranslated(GetRecord.UniqueKey, GetRecord.ParentSig, GetRecord.String, ModRef);

                Border CardBorder = new Border();
                CardBorder.Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
                CardBorder.CornerRadius = new CornerRadius(6);
                CardBorder.Margin = new Thickness(0, 0, 0, 6);
                CardBorder.Padding = new Thickness(8, 6, 8, 6);
                CardBorder.Tag = new TrackingItem(ModRef,GetRecord.UniqueKey);         

                StackPanel ContentPanel = new StackPanel();
                ContentPanel.Orientation = Orientation.Vertical;

                TextBox TextLine = new TextBox();
                TextLine.Background = null;
                TextLine.BorderBrush = null;
                TextLine.BorderThickness = new Thickness(0);
                TextLine.IsReadOnly = true;
                TextLine.Foreground = Brushes.White;
                TextLine.FontSize = 13;
                TextLine.TextWrapping = TextWrapping.Wrap;


                if (GetTranslated.Length == 0)
                {
                    TextLine.Text = GetRecord.String;
                }
                else
                {
                    TextLine.Text = GetRecord.String + " -> " + GetTranslated;
                }

                ContentPanel.Children.Add(TextLine);

                StackPanel InfoLine = new StackPanel();
                InfoLine.Orientation = Orientation.Horizontal;
                InfoLine.Margin = new Thickness(0, 4, 0, 0);

                TextBlock EmotionText = new TextBlock();
                if (Item.EmotionType != 999)
                {
                    EmotionText.Text = EmotionTypeHelper.FromRaw(Item.EmotionType).ToString();
                    CardBorder.Cursor = Cursors.Hand;
                    TextLine.Cursor = Cursors.Hand;

                    CardBorder.PreviewMouseDown += AnyCard_PreviewMouseDown;
                }
                else
                {
                    //Double checking prevents display errors; I'm unsure if the emoji value in ESP will be exactly 999.
                    if (Item.SubOffset == 0)
                    {
                        TextLine.Foreground = new SolidColorBrush(Color.FromRgb(0xFA, 0xE3, 0x06));
                        EmotionText.Text = "Tittle";
                    }
                }
               
                EmotionText.Foreground = new SolidColorBrush(Color.FromRgb(0xFA, 0xE3, 0x06));
                EmotionText.FontSize = 12;
                EmotionText.FontWeight = FontWeights.DemiBold;
                InfoLine.Children.Add(EmotionText);


                TextBlock ResponseIdText = new TextBlock();
                ResponseIdText.Text = "  #" + GetRecord.UniqueKey;
                ResponseIdText.Foreground = new SolidColorBrush(Color.FromRgb(0xBF, 0xBF, 0xBF));
                ResponseIdText.FontSize = 12;
                InfoLine.Children.Add(ResponseIdText);

                ContentPanel.Children.Add(InfoLine);
                CardBorder.Child = ContentPanel;

                return CardBorder;
            }

            return null;
        }

        private void AnyCard_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                TrackingItem GetTrack = (TrackingItem)((sender as Border).Tag);
                GetTrack.ModRef.ListView.Goto(GetTrack.Key);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MultiWindowController.TrackingWin = null;
        }

        public void MatchTransItem(string Original, uint StringKey, CancellationToken CancellationToken)
        {
            //MatchView.Dispatcher.Invoke(new Action(() =>
            //{
            //    MatchView.Children.Clear();
            //}));

            //List<string> UniqueResult = new List<string>();
            //List<string> UniqueKeys = new List<string>();

            //var MatchCloudItems = LocalDBCache.MatchLocalItem((int)TranslatorInterface.Instance.To, Original);

            //foreach (var GetMatch in MatchCloudItems)
            //{
            //    if (!UniqueResult.Contains(GetMatch.Result))
            //    {
            //        UniqueResult.Add(GetMatch.Result);
            //        if (!UniqueKeys.Contains(GetMatch.Key))
            //        {
            //            UniqueKeys.Add(GetMatch.Key);
            //            MatchView.Dispatcher.Invoke(new Action(() =>
            //            {
            //                MatchView.Children.Add(UIHelper.CreatMatchLine(
            //                  UniqueKeyHelper.RowidToOriginalKey(GetMatch.FileUniqueKey),//Get Original File Name
            //                  GetMatch.Key,
            //                  GetMatch.Result
            //                  ));
            //            }));
            //        }
            //    }
            //}

            //foreach (var GetMatch in CloudDBCache.MatchCloudItem((int)TranslatorInterface.Instance.To, Original))
            //{
            //    if (!UniqueResult.Contains(GetMatch.Result))
            //    {
            //        UniqueResult.Add(GetMatch.Result);
            //        if (!UniqueKeys.Contains(GetMatch.Key))
            //        {
            //            UniqueKeys.Add(GetMatch.Key);
            //            MatchView.Dispatcher.Invoke(new Action(() =>
            //            {
            //                MatchView.Children.Add(UIHelper.CreatMatchLine(
            //                  UniqueKeyHelper.RowidToOriginalKey(GetMatch.FileUniqueKey),//Get Original File Name
            //                  GetMatch.Key,
            //                  GetMatch.Result
            //                  ));
            //            }));
            //        }
            //    }
            //}


            //Find DL IL Strings
            //if (StringKey != 0)
            //    if (DeFine.WorkingWin.CurrentTransType == 2)
            //    {
            //        if (EspInstance.ToStringsFile != null)
            //        {
            //            if (EspInstance.ToStringsFile.Strings.ContainsKey(StringKey) == true)
            //            {
            //                string AutoFileName = "Strings";
            //                var FindItem = EspInstance.ToStringsFile.Strings[StringKey];

            //                if (FindItem.Type == StringsFileType.DL)
            //                {
            //                    AutoFileName += ".dlstrings";
            //                }
            //                else
            //                if (FindItem.Type == StringsFileType.IL)
            //                {
            //                    AutoFileName += ".ilstrings";
            //                }
            //                else
            //                {
            //                    AutoFileName += ".strings";
            //                }
            //                MatchView.Dispatcher.Invoke(new Action(() =>
            //                {
            //                    MatchView.Children.Add(UIHelper.CreatMatchLine(
            //                    FindItem.Type.ToString(),
            //                    FindItem.ID.ToString(),
            //                    FindItem.Value
            //                    ));
            //                }));
            //            }
            //        }
            //    }
        }

    }
}
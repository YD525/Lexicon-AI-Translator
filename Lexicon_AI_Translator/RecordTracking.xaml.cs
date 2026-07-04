using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LexTranslator.SkyrimManagement;
using LexTranslator.UIManagement;

namespace LexTranslator
{
    public partial class RecordTracking : Window
    {
        private Window _Owner;

        public RecordTracking(Window Owner)
        {
            InitializeComponent();

            _Owner = Owner;

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
            ToggleSection(NpcContentHost, NpcChevronRotate);
        }

        private void RelatedTextHeader_PreviewMouseDown(object Sender, MouseButtonEventArgs E)
        {
            ToggleSection(RelatedTextContentHost, RelatedTextChevronRotate);
        }

        private void DialogueHeader_PreviewMouseDown(object Sender, MouseButtonEventArgs E)
        {
            ToggleSection(DialogueContentHost, DialogueChevronRotate);
        }

        private void ToggleSection(Border ContentHost, RotateTransform ChevronRotate)
        {
            bool IsExpanded = double.IsNaN(ContentHost.Height) || ContentHost.Height > 0;

            if (IsExpanded)
            {
                CollapseSection(ContentHost, ChevronRotate);
            }
            else
            {
                ExpandSection(ContentHost, ChevronRotate);
            }
        }
        private void CollapseSection(Border ContentHost, RotateTransform ChevronRotate)
        {
            ContentHost.BeginAnimation(FrameworkElement.HeightProperty, null);

            double StartHeight = double.IsNaN(ContentHost.Height) ? ContentHost.ActualHeight : ContentHost.Height;

            DoubleAnimation HeightAnimation = new DoubleAnimation();
            HeightAnimation.From = StartHeight;
            HeightAnimation.To = 0;
            HeightAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(220));
            HeightAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
            HeightAnimation.Completed += delegate
            {
                ContentHost.BeginAnimation(FrameworkElement.HeightProperty, null);
                ContentHost.Height = 0;
            };

            DoubleAnimation RotateAnimation = new DoubleAnimation();
            RotateAnimation.To = 180;
            RotateAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(220));
            RotateAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

            ContentHost.BeginAnimation(FrameworkElement.HeightProperty, HeightAnimation);
            ChevronRotate.BeginAnimation(RotateTransform.AngleProperty, RotateAnimation);
        }

        private void ExpandSection(Border ContentHost, RotateTransform ChevronRotate)
        {
            ContentHost.BeginAnimation(FrameworkElement.HeightProperty, null);

            double CollapsedHeight = ContentHost.Height;
            if (double.IsNaN(CollapsedHeight))
            {
                CollapsedHeight = 0;
            }

            ContentHost.Height = double.NaN;
            double AvailableWidth = ContentHost.ActualWidth > 0 ? ContentHost.ActualWidth : double.PositiveInfinity;
            ContentHost.Measure(new Size(AvailableWidth, double.PositiveInfinity));
            double TargetHeight = ContentHost.DesiredSize.Height;
            ContentHost.Height = CollapsedHeight;

            DoubleAnimation HeightAnimation = new DoubleAnimation();
            HeightAnimation.From = CollapsedHeight;
            HeightAnimation.To = TargetHeight;
            HeightAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(220));
            HeightAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
            HeightAnimation.Completed += delegate
            {
                ContentHost.BeginAnimation(FrameworkElement.HeightProperty, null);
                ContentHost.Height = double.NaN;
            };

            DoubleAnimation RotateAnimation = new DoubleAnimation();
            RotateAnimation.To = 0;
            RotateAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(220));
            RotateAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

            ContentHost.BeginAnimation(FrameworkElement.HeightProperty, HeightAnimation);
            ChevronRotate.BeginAnimation(RotateTransform.AngleProperty, RotateAnimation);
        }

        //Data loading

        public void LoadNpcRecord(RecordItem Record,string NpcName,string Gender)
        {
            NpcListPanel.Children.Clear();
            NpcListPanel.Children.Add(BuildNpcCard(Record, NpcName, Gender));
        }

        public void LoadRelatedTextRecords(List<RecordItem> Records)
        {
            RelatedTextListPanel.Children.Clear();

            if (Records == null)
            {
                return;
            }

            for (int i = 0; i < Records.Count; i++)
            {
                RelatedTextListPanel.Children.Add(BuildRelatedTextCard(Records[i]));
            }
        }

        public void LoadDialogueRecords(ModFile ModRef, List<ManagedDialNode> Records)
        {
            DialogueListPanel.Children.Clear();

            if (Records == null)
            {
                return;
            }

            for (int i = 0; i < Records.Count; i++)
            {
                DialogueListPanel.Children.Add(BuildDialogueCard(ModRef, Records[i]));
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

        private Border BuildRelatedTextCard(RecordItem Item)
        {
            Border CardBorder = new Border();
            CardBorder.Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            CardBorder.CornerRadius = new CornerRadius(6);
            CardBorder.Margin = new Thickness(0, 0, 0, 6);
            CardBorder.Padding = new Thickness(8, 6, 8, 6);

            TextBlock TextLine = new TextBlock();
            TextLine.Text = Item.String;
            TextLine.Foreground = Brushes.White;
            TextLine.FontSize = 13;
            TextLine.TextWrapping = TextWrapping.Wrap;

            CardBorder.Child = TextLine;

            return CardBorder;
        }

        private Border BuildDialogueCard(ModFile ModRef, ManagedDialNode Item)
        {
            Border CardBorder = new Border();
            CardBorder.Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            CardBorder.CornerRadius = new CornerRadius(6);
            CardBorder.Margin = new Thickness(0, 0, 0, 6);
            CardBorder.Padding = new Thickness(8, 6, 8, 6);

            StackPanel ContentPanel = new StackPanel();
            ContentPanel.Orientation = Orientation.Vertical;

            TextBlock TextLine = new TextBlock();
            TextLine.Foreground = Brushes.White;
            TextLine.FontSize = 13;
            TextLine.TextWrapping = TextWrapping.Wrap;
            TextLine.Text = ModRef.EspReader.GetRecordItemByOffsets(0, Item.RecordOffset, Item.SubOffset).String;
            ContentPanel.Children.Add(TextLine);

            StackPanel InfoLine = new StackPanel();
            InfoLine.Orientation = Orientation.Horizontal;
            InfoLine.Margin = new Thickness(0, 4, 0, 0);

            TextBlock EmotionText = new TextBlock();
            EmotionText.Text = EmotionTypeHelper.FromRaw(Item.EmotionType).ToString();
            EmotionText.Foreground = new SolidColorBrush(Color.FromRgb(0xFA, 0xE3, 0x06));
            EmotionText.FontSize = 12;
            EmotionText.FontWeight = FontWeights.DemiBold;
            InfoLine.Children.Add(EmotionText);
           

            TextBlock ResponseIdText = new TextBlock();
            ResponseIdText.Text = "  #" + Item.ResponseID;
            ResponseIdText.Foreground = new SolidColorBrush(Color.FromRgb(0xBF, 0xBF, 0xBF));
            ResponseIdText.FontSize = 12;
            InfoLine.Children.Add(ResponseIdText);

            ContentPanel.Children.Add(InfoLine);
            CardBorder.Child = ContentPanel;

            return CardBorder;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MultiWindowController.TrackingWin = null;
        }
    }
}
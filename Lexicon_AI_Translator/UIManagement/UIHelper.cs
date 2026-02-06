using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using LexTranslator.ConvertManager;
using System.Windows.Media.Animation;
using System.Windows.Input;
using PhoenixEngine.EngineManagement;
using PhoenixEngine.TranslateManage;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using LexTranslator.SkyrimManagement;
using LexTranslator.UIManagement;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Xml;
using System.Windows.Markup;
using PhoenixEngine.PlatformManagement;
using static LexTranslator.UIManagement.NodeStyleWin;
using System.Runtime.CompilerServices;
using PhoenixEngine.TranslateManagement;

namespace LexTranslator.UIManage
{
    public class ScanAnimator
    {
        private readonly TranslateTransform _ScanTransform;
        private readonly FrameworkElement _ProcessBar;
        private DoubleAnimation _Animation;

        private readonly double _Speed = 120;
        private double _PendingTo;

        public ScanAnimator(TranslateTransform scanTransform, FrameworkElement processBar, double speed = 120)
        {
            _ScanTransform = scanTransform;
            _ProcessBar = processBar;
            _Speed = speed;
        }

        public void UpdateAnimationTarget()
        {
            _PendingTo = _ProcessBar.ActualWidth;
        }

        public void Start()
        {
            if (_Animation != null) return;
            StartNewCycle();
        }

        public void Stop()
        {
            _ScanTransform.BeginAnimation(TranslateTransform.XProperty, null);
            _Animation = null;
        }

        private void StartNewCycle()
        {
            double from = -30;
            double to = _PendingTo > 0 ? _PendingTo : _ProcessBar.ActualWidth;
            if (to <= 0) return;

            double distance = to - from;
            double durationSeconds = distance / _Speed;

            _Animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                AutoReverse = false,
                RepeatBehavior = new RepeatBehavior(1),
                FillBehavior = FillBehavior.Stop
            };

            Timeline.SetDesiredFrameRate(_Animation, 15);

            _Animation.Completed += (s, e) =>
            {
                _ScanTransform.BeginAnimation(TranslateTransform.XProperty, null);
                _Animation = null;

                StartNewCycle();
            };

            _ScanTransform.BeginAnimation(TranslateTransform.XProperty, _Animation);
        }
    }

    public class UIHelper
    {
        public static void ShowButton(Border NormalButton, bool Enable)
        {
            if (Enable)
            {
                NormalButton.Cursor = Cursors.Hand;
                NormalButton.IsEnabled = true;
                NormalButton.Opacity = 1;
            }
            else
            {
                NormalButton.Cursor = null;
                NormalButton.Opacity = 0.5;
                NormalButton.IsEnabled = false;
            }
        }

        public static Grid SelectLine = null;

        public static double DefLineHeight = 42;
        public static double DefFontSize = 15;

        public static readonly Typeface _TypeFace =
    new Typeface(SystemFonts.MessageFontFamily,
                 FontStyles.Normal,
                 FontWeights.Normal,
                 FontStretches.Normal);

        public static double MeasureTextWidth(string Text, double FontSize)
        {
            if (string.IsNullOrEmpty(Text))
                return 0;


            if (Text.Length < 16)
                return Text.Length * FontSize * 0.6;

            var Font = new FormattedText(
                Text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                _TypeFace,
                FontSize,
                Brushes.Black,
                null,
                1);

            return Font.WidthIncludingTrailingWhitespace;
        }

        public static FakeGrid CreateFakeLine(string Type, string Key, string SourceText, string TransText, double Score)
        {
            double AutoHeight = DefLineHeight;

            if (!string.IsNullOrEmpty(SourceText))
            {
                var MaxWidth = (DeFine.WorkingWin.ActualWidth / 3) - 135;

                int RoughCharLimit = (int)(MaxWidth / (SystemFonts.MessageFontSize * 0.6));

                if (SourceText.Length > RoughCharLimit)
                {
                    AutoHeight = 82;
                }
                else
                {
                    double Width = MeasureTextWidth(SourceText, SystemFonts.MessageFontSize);
                    if (Width > MaxWidth)
                        AutoHeight = 82;
                }
            }

            return new FakeGrid(AutoHeight, Type, Key, SourceText, TransText, Score);
        }

        public static Grid CreateLine(FakeGrid Item)
        {
            return CreateLine(Item.Height, Item.Type, Item.Key, Item.SourceText, Item.TransText, Item.Score);
        }

        public static Grid CreateLine(double Height, string Type, string Key, string SourceText, string TransText, double Score)
        {
            Grid MainGrid = DeFine.RowStyleWin.CreateLine(Height, new BaseUnit(Phoenix.GetFileUniqueKey(), Key, Type, SourceText, TransText, Score));
            return MainGrid;
        }

        public static void SyncFromStringsFile(YDListView View)
        {
            var CanVasHandle = View.GetMainCanvas();
            CanVasHandle.Dispatcher.Invoke(new Action(() =>
            {
                CanVasHandle.IsEnabled = false;
            }));

            for (int i = 0; i < View.RealLines.Count; i++)
            {
                var Line = View.RealLines[i];

                if (EspReader.Records.ContainsKey(Line.Key))
                {
                    var GetRealRecord = EspReader.Records[Line.Key];
                    if (EspReader.FromStringsFile.Strings.ContainsKey(GetRealRecord.StringID))
                    {
                        View.RealLines[i].SourceText = EspReader.FromStringsFile.Strings[GetRealRecord.StringID].Value;
                        View.RealLines[i].RealSource = string.Empty;
                        View.RealLines[i].SyncUI(View);
                    }
                }
            }

            CanVasHandle.Dispatcher.Invoke(new Action(() =>
            {
                CanVasHandle.IsEnabled = true;
            }));
        }

        public static void TransViewSyncEspRecord(YDListView View)
        {
            var CanVasHandle = View.GetMainCanvas();
            CanVasHandle.Dispatcher.Invoke(new Action(() =>
            {
                CanVasHandle.IsEnabled = false;
            }));

            var AllRecords = EspReader.Records.Values.ToList();

            const int BatchSize = 10000;
            int Total = AllRecords.Count;

            for (int i = 0; i < Total; i += BatchSize)
            {
                var Batch = AllRecords.Skip(i).Take(BatchSize).ToList();

                View.Parent.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (var Record in Batch)
                    {
                        View.AddRowR(LineRenderer.CreateLine(
                            Record.ParentSig,
                            Record.FormID,
                            Record.UniqueKey,
                            Record.String,
                            "",
                            999));
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }

            CanVasHandle.Dispatcher.Invoke(new Action(() =>
            {
                CanVasHandle.IsEnabled = true;
            }));
        }

        private static CancellationTokenSource AutoCancelSelectIDETrd;
        public static Thread AutoSelectIDETrd = null;
        public static void SelectLineFromIDE(string GetKey)
        {
            try
            {
                if (AutoSelectIDETrd != null)
                {
                    CancelAutoSelect();
                }
            }
            catch { }

            AutoCancelSelectIDETrd = new CancellationTokenSource();
            var Token = AutoCancelSelectIDETrd.Token;

            AutoSelectIDETrd = new Thread(() =>
            {
                try
                {
                    string[] Params = GetKey.Split(',');
                    if (Params.Length > 1)
                    {
                        Task.Delay(200, Token).Wait(Token);

                        Token.ThrowIfCancellationRequested();

                        //foreach (var Item in DeFine.WorkingWin.GlobalPexReader.HeuristicEngine.DStringItems)
                        //{
                        //    if (Item.Key.Contains(","))
                        //    {
                        //        if (Item.Key.Equals(Params[0] + "," + Params[1]))
                        //        {
                        //            DeFine.ActiveIDE.Dispatcher.Invoke(() =>
                        //            {
                        //                int lineOffset = DeFine.ActiveIDE.Document.Text.IndexOf(Item.SourceLine);
                        //                if (lineOffset == -1) return;

                        //                int relativeOffset = Item.SourceLine.IndexOf("\"" + Item.Str + "\"");
                        //                if (relativeOffset == -1) return;

                        //                int absoluteOffset = lineOffset + relativeOffset;
                        //                DeFine.ActiveIDE.ScrollToLine(DeFine.ActiveIDE.Document.GetLineByOffset(absoluteOffset).LineNumber);
                        //                DeFine.ActiveIDE.Select(absoluteOffset, ("\"" + Item.Str + "\"").Length);
                        //            });
                        //        }
                        //    }

                        //}
                    }
                }
                catch (OperationCanceledException)
                {

                }
            });

            AutoSelectIDETrd.Start();
        }

        public static void CancelAutoSelect()
        {
            AutoCancelSelectIDETrd?.Cancel();
        }

        public static bool LeftMenuIsShow = false;

        static class NodeLightController
        {
            private static readonly ConditionalWeakTable<Grid, CancellationTokenSource> _lightMap
                = new ConditionalWeakTable<Grid, CancellationTokenSource>();

            public static void Blink(Grid grid, ContentControl light, int ms)
            {
                if (_lightMap.TryGetValue(grid, out _))
                    return;

                var cts = new CancellationTokenSource();
                _lightMap.Add(grid, cts);

                light.Dispatcher.Invoke(() =>
                {
                    light.Style = (Style)Application.Current.FindResource("IndicatorOnStyle");
                });

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(ms, cts.Token);

                        light.Dispatcher.Invoke(() =>
                        {
                            light.Style = (Style)Application.Current.FindResource("IndicatorOffStyle");
                        });
                    }
                    catch (TaskCanceledException) { }
                    finally
                    {
                        _lightMap.Remove(grid);
                    }
                });
            }
        }

        public static void NodeCallCallback(int CustomID, PlatformType Sign)
        {
            try
            {
                if (!LeftMenuIsShow) return;

                if (!DeFine.WorkingWin.IsExpanded)
                {
                    return;
                }

                DeFine.WorkingWin.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        for (int i = 0; i < DeFine.WorkingWin.Nodes.Children.Count; i++)
                        {
                            if (DeFine.WorkingWin.Nodes.Children[i] is Grid)
                            {
                                Grid SetGrid = DeFine.WorkingWin.Nodes.Children[i] as Grid;
                                if (SetGrid.Children[0] is Grid)
                                {
                                    HeaderInFo GetHeader = SetGrid.Tag as HeaderInFo;
                                    if (GetHeader.CustomID <= 0 && GetHeader.MainType == PlatformType.Null)
                                    {
                                        if (Sign == PlatformType.PhoenixEngine)
                                        {
                                            ContentControl GetLight = DeFine.NodeStyleWin.GetNodeLight(SetGrid);
                                            NodeLightController.Blink(SetGrid, GetLight, 1000);
                                        }
                                    }
                                    else
                                    if (GetHeader.CustomID <= 0 && GetHeader.MainType == Sign)
                                    {
                                        ContentControl GetLight = DeFine.NodeStyleWin.GetNodeLight(SetGrid);
                                        NodeLightController.Blink(SetGrid, GetLight, 1000);
                                    }
                                    else
                                    if (GetHeader.CustomID > 0 && GetHeader.CustomID == CustomID)
                                    {
                                        ContentControl GetLight = DeFine.NodeStyleWin.GetNodeLight(SetGrid);
                                        NodeLightController.Blink(SetGrid, GetLight, 1000);
                                    }
                                }

                            }
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }

        public static void SyncAvalonEditTextLayout()
        {
            if (DeFine.GlobalLocalSetting.TextDisplay == TextLayout.LTR)
            {
                DeFine.WorkingWin.ToStr.FlowDirection = FlowDirection.LeftToRight;
            }
            else
            {
                DeFine.WorkingWin.ToStr.FlowDirection = FlowDirection.RightToLeft;
            }
        }
        public enum StyleType
        {
            BlueStyle = 0, RetroStyle = 1
        }
        public static void SetGlobalStyle(StyleType Style)
        {
            switch (Style)
            {
                case StyleType.BlueStyle:
                    {
                        LoadResourceDictionary("/Themes/BlueStyle.xaml");
                    }
                    break;
                case StyleType.RetroStyle:
                    {
                        LoadResourceDictionary("/Themes/RetroStyle.xaml");
                    }
                    break;
            }
        }
        public static void LoadResourceDictionary(string resourceName)
        {
            var Dict = new ResourceDictionary();
            Dict.Source = new Uri(resourceName, UriKind.Relative);
            try
            {
                Application.Current.Resources.MergedDictionaries.Clear();
            }
            catch { }
            try
            {
                Application.Current.Resources.MergedDictionaries.Add(Dict);
            }
            catch { }
            Application.Current.MainWindow?.InvalidateVisual();
            Application.Current.MainWindow?.UpdateLayout();
        }


        public static Grid CreatMatchLine(string From, string Type, string Translated)
        {
            Grid NewLine = new Grid();
            NewLine.Tag = Translated;
            NewLine.Height = 38;
            NewLine.Cursor = Cursors.Hand;

            NewLine.MouseEnter += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                var GetLastGrid = (Grid)sender;
                ((Grid)((Grid)sender).Children[0]).Background = new SolidColorBrush((Color)Application.Current.Resources["LineASelected"]);
            });

            NewLine.MouseLeave += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                var GetLastGrid = (Grid)sender;
                ((Grid)((Grid)sender).Children[0]).Background = new SolidColorBrush((Color)Application.Current.Resources["LineANormal"]);
            });

            RowDefinition Row1st = new RowDefinition();
            Row1st.Height = new GridLength(1, GridUnitType.Star);
            RowDefinition Row2nd = new RowDefinition();
            Row2nd.Height = new GridLength(1, GridUnitType.Pixel);

            NewLine.RowDefinitions.Add(Row1st);
            NewLine.RowDefinitions.Add(Row2nd);

            NewLine.Style = (Style)Application.Current.FindResource("LineStyle");
            NewLine.Margin = new Thickness(0, 0, 0, 1);

            ColumnDefinition Column1st = new ColumnDefinition();
            Column1st.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition Column2nd = new ColumnDefinition();
            Column2nd.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition Column3rd = new ColumnDefinition();
            Column3rd.Width = new GridLength(1, GridUnitType.Star);

            NewLine.ColumnDefinitions.Add(Column1st);
            NewLine.ColumnDefinitions.Add(Column2nd);
            NewLine.ColumnDefinitions.Add(Column3rd);

            Grid BottomGrid = new Grid();
            Grid.SetRow(BottomGrid, 1);
            Grid.SetColumnSpan(BottomGrid, 3);

            NewLine.Children.Add(BottomGrid);

            Style LabelStyle = (Style)Application.Current.FindResource("LineTypeFont");

            Label FromLab = new Label();
            FromLab.Content = From;
            FromLab.Style = LabelStyle;

            NewLine.Children.Add(FromLab);
            Grid.SetColumn(FromLab, 0);

            Label TypeLab = new Label();
            TypeLab.Content = Type;

            TypeLab.Style = LabelStyle;
            Brush DefaultBrush = (Brush)TypeLab.Foreground;

            TypeLab.MouseEnter += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                TypeLab.Foreground = new SolidColorBrush((Color)Application.Current.Resources["LineASelected"]);
            });

            TypeLab.MouseLeave += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                TypeLab.Foreground = DefaultBrush;
            });

            TypeLab.MouseLeftButtonDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) =>
            {
                DeFine.WorkingWin.TransViewList?.Goto(ConvertHelper.ObjToStr(TypeLab.Content));
            });

            NewLine.Children.Add(TypeLab);
            Grid.SetColumn(TypeLab, 1);

            TextBox TranslatedLab = new TextBox();
            TranslatedLab.Margin = new Thickness(5);
            TranslatedLab.Text = Translated;
            TranslatedLab.IsReadOnly = true;
            TranslatedLab.AcceptsReturn = true;
            TranslatedLab.TextWrapping = TextWrapping.Wrap;
            TranslatedLab.Background = null;
            TranslatedLab.BorderBrush = null;
            TranslatedLab.FontSize = 11;
            TranslatedLab.BorderThickness = new Thickness(0);
            TranslatedLab.VerticalContentAlignment = VerticalAlignment.Center;
            TranslatedLab.HorizontalContentAlignment = HorizontalAlignment.Center;
            TranslatedLab.Cursor = Cursors.Hand;

            TranslatedLab.MouseEnter += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                TranslatedLab.Foreground = new SolidColorBrush((Color)Application.Current.Resources["LineASelected"]);
            });

            TranslatedLab.MouseLeave += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                TranslatedLab.Foreground = DefaultBrush;
            });

            if (DeFine.GlobalLocalSetting.Style == 1)
            {
                TranslatedLab.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                TranslatedLab.Foreground = new SolidColorBrush(Colors.Black);
            }

            NewLine.Children.Add(TranslatedLab);
            Grid.SetColumn(TranslatedLab, 2);

            NewLine.PreviewMouseDown += MatchLine_PreviewMouseDown;

            return NewLine;
        }


        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T parentT)
                    return parentT;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
        private static void MatchLine_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid)
            {
                Grid GetGrid = (Grid)sender;
                if (DeFine.WorkingWin != null)
                {
                    if (e.OriginalSource is DependencyObject source)
                    {
                        if (FindParent<TextBox>(source) != null)
                        {
                            DeFine.WorkingWin.ToStr.Text = ConvertHelper.ObjToStr(GetGrid.Tag);
                        }
                    }
                }
            }
        }

        public static StackPanel CreatModuleItem(string ModuleName, string Version)
        {
            StackPanel LinePanel = new StackPanel();
            LinePanel.Orientation = Orientation.Horizontal;
            LinePanel.HorizontalAlignment = HorizontalAlignment.Center;

            TextBox Name = new TextBox();
            Name.Foreground = new SolidColorBrush(Colors.White);
            Name.Text = ModuleName + "-" + Version;
            Name.BorderBrush = null;
            Name.BorderThickness = new Thickness(0);
            Name.Background = null;
            Name.IsReadOnly = true;
            Name.VerticalAlignment = VerticalAlignment.Center;

            Name.FontSize = 13.5;
            Name.FontWeight = FontWeights.DemiBold;

            LinePanel.Children.Add(Name);

            return LinePanel;
        }


        public static T CloneElement<T>(T source) where T : UIElement
        {
            try
            {
                if (source == null) return null;

                string xaml = XamlWriter.Save(source);
                StringReader stringReader = new StringReader(xaml);
                XmlReader xmlReader = XmlReader.Create(stringReader);

                return (T)XamlReader.Load(xmlReader);
            }
            catch { return null; }
        }


        public static void SyncNodes()
        {
            List<PlatformConfig> CustomPlatforms = new List<PlatformConfig>();

            List<PlatformConfig> LocalAIPlatforms = new List<PlatformConfig>();
            List<PlatformConfig> CloudAIPlatforms = new List<PlatformConfig>();
            List<PlatformConfig> TraditionalPlatforms = new List<PlatformConfig>();

            for (int i = 0; i < Phoenix.Config.PlatformConfigs.Count; i++)
            {
                var GetKey = Phoenix.Config.PlatformConfigs.ElementAt(i).Key;
                if (Phoenix.Config.PlatformConfigs[GetKey].Platform == PlatformType.CustomPlatform)
                {
                    CustomPlatforms.Add(Phoenix.Config.PlatformConfigs[GetKey]);
                }
                else
                if (Phoenix.Config.PlatformConfigs[GetKey].Platform == PlatformType.LMLocalAI)
                {
                    LocalAIPlatforms.Add(Phoenix.Config.PlatformConfigs[GetKey]);
                }
                else
                if (Phoenix.Config.PlatformConfigs[GetKey].Platform == PlatformType.ChatGpt
                    || Phoenix.Config.PlatformConfigs[GetKey].Platform == PlatformType.Gemini
                    || Phoenix.Config.PlatformConfigs[GetKey].Platform == PlatformType.DeepSeek)
                {
                    CloudAIPlatforms.Add(Phoenix.Config.PlatformConfigs[GetKey]);
                }
                else
                {
                    TraditionalPlatforms.Add(Phoenix.Config.PlatformConfigs[GetKey]);
                }
            }

            DeFine.WorkingWin.Nodes.Children.Clear();

            DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenMainNodeTree("Engine Nodes"));
            DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNode("PreTranslate Node", PlatformType.Null, CustomPlatformType.Null, 0, Phoenix.Config.PreTranslateEnable));

            DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNodeTree("Cloud AI Nodes"));
            foreach (var Get in CloudAIPlatforms)
            {
                DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNode(Get.Platform.ToString(), Get.Platform, CustomPlatformType.CloudAI, 0, Get.Enable));
            }

            foreach (var Get in CustomPlatforms)
            {
                if (Get.CustomInFo != null)
                {
                    if (Get.CustomInFo.Type == CustomPlatformType.CloudAI)
                    {
                        DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNode(Get.CustomInFo.Name, Get.Platform, Get.CustomInFo.Type, Get.CustomInFo.CustomID, Get.Enable));
                    }
                }
            }
            DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenEmptyNode(CustomPlatformType.CloudAI));


            DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNodeTree("Local AI Nodes"));
            foreach (var Get in LocalAIPlatforms)
            {
                string AutoName = Get.Platform.ToString();
                if (AutoName == "LMLocalAI")
                {
                    AutoName = "LM Studio";
                }
                DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNode(AutoName, Get.Platform, CustomPlatformType.LocalAI, 0, Get.Enable));
            }

            foreach (var Get in CustomPlatforms)
            {
                if (Get.CustomInFo != null)
                {
                    if (Get.CustomInFo.Type == CustomPlatformType.LocalAI)
                    {
                        DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNode(Get.CustomInFo.Name, Get.Platform, Get.CustomInFo.Type, Get.CustomInFo.CustomID, Get.Enable));
                    }
                }
            }
            DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenEmptyNode(CustomPlatformType.LocalAI));

            DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNodeTree("Traditional Nodes"));
            foreach (var Get in TraditionalPlatforms)
            {
                DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNode(Get.Platform.ToString(), Get.Platform, CustomPlatformType.Traditional, 0, Get.Enable));
            }

            foreach (var Get in CustomPlatforms)
            {
                if (Get.CustomInFo != null)
                {
                    if (Get.CustomInFo.Type == CustomPlatformType.Traditional)
                    {
                        DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenNode(Get.CustomInFo.Name, Get.Platform, Get.CustomInFo.Type, Get.CustomInFo.CustomID, Get.Enable));
                    }
                }
            }

            DeFine.WorkingWin.Nodes.Children.Add(DeFine.NodeStyleWin.GenEmptyNode(CustomPlatformType.Traditional));

            DeFine.NodeStyleWin.SyncCount();
        }

    }

    public enum TextLayout
    {
        LTR = 0, RTL = 1
    }
}

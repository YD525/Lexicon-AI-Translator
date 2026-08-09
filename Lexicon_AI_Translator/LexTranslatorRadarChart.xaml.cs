using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LexTranslator
{
    public partial class LexTranslatorRadarChart : UserControl
    {
        public static readonly DependencyProperty TopValueProperty =
            DependencyProperty.Register("TopValue", typeof(double), typeof(LexTranslatorRadarChart),
                new FrameworkPropertyMetadata(80.0, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged));

        public static readonly DependencyProperty TopRightValueProperty =
            DependencyProperty.Register("TopRightValue", typeof(double), typeof(LexTranslatorRadarChart),
                new FrameworkPropertyMetadata(70.0, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged));

        public static readonly DependencyProperty BottomRightValueProperty =
            DependencyProperty.Register("BottomRightValue", typeof(double), typeof(LexTranslatorRadarChart),
                new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged));

        public static readonly DependencyProperty BottomLeftValueProperty =
            DependencyProperty.Register("BottomLeftValue", typeof(double), typeof(LexTranslatorRadarChart),
                new FrameworkPropertyMetadata(60.0, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged));

        public static readonly DependencyProperty TopLeftValueProperty =
            DependencyProperty.Register("TopLeftValue", typeof(double), typeof(LexTranslatorRadarChart),
                new FrameworkPropertyMetadata(40.0, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged));

        public double TopValue
        {
            get { return (double)GetValue(TopValueProperty); }
            set { SetValue(TopValueProperty, Clamp(value, 0, 100)); }
        }

        public double TopRightValue
        {
            get { return (double)GetValue(TopRightValueProperty); }
            set { SetValue(TopRightValueProperty, Clamp(value, 0, 100)); }
        }

        public double BottomRightValue
        {
            get { return (double)GetValue(BottomRightValueProperty); }
            set { SetValue(BottomRightValueProperty, Clamp(value, 0, 100)); }
        }

        public double BottomLeftValue
        {
            get { return (double)GetValue(BottomLeftValueProperty); }
            set { SetValue(BottomLeftValueProperty, Clamp(value, 0, 100)); }
        }

        public double TopLeftValue
        {
            get { return (double)GetValue(TopLeftValueProperty); }
            set { SetValue(TopLeftValueProperty, Clamp(value, 0, 100)); }
        }

        private static void OnValueChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            ((LexTranslatorRadarChart)D).UpdateRadar();
        }

        private const double CenterX = 280;
        private const double CenterY = 220;
        private const double Radius = 130;

        public LexTranslatorRadarChart()
        {
            InitializeComponent();
            Loaded += (S, E) => UpdateRadar();
        }

        private void UpdateRadar()
        {
            double[] Angles = { Math.PI / 2, Math.PI / 2 + 2 * Math.PI / 5, Math.PI / 2 + 4 * Math.PI / 5, Math.PI / 2 + 6 * Math.PI / 5, Math.PI / 2 + 8 * Math.PI / 5 };
            double[] Values = { TopValue / 100.0, TopRightValue / 100.0, BottomRightValue / 100.0, BottomLeftValue / 100.0, TopLeftValue / 100.0 };

            Point[] DataPoints = new Point[5];
            for (int I = 0; I < 5; I++)
            {
                double X = CenterX + Radius * Values[I] * Math.Cos(Angles[I]);
                double Y = CenterY - Radius * Values[I] * Math.Sin(Angles[I]);
                DataPoints[I] = new Point(X, Y);
            }
            DataPolygon.Points = new PointCollection(DataPoints);

            double[] Levels = { 1.0, 0.8, 0.6, 0.4, 0.2 };
            Polygon[] GridPolygons = { GridOuter, GridA, GridB, GridC, GridD };
            for (int J = 0; J < Levels.Length; J++)
            {
                Point[] Pts = new Point[5];
                for (int I = 0; I < 5; I++)
                {
                    double R = Radius * Levels[J];
                    double X = CenterX + R * Math.Cos(Angles[I]);
                    double Y = CenterY - R * Math.Sin(Angles[I]);
                    Pts[I] = new Point(X, Y);
                }
                GridPolygons[J].Points = new PointCollection(Pts);
            }

            Point[] AxisEnds = new Point[5];
            for (int I = 0; I < 5; I++)
            {
                double X = CenterX + Radius * Math.Cos(Angles[I]);
                double Y = CenterY - Radius * Math.Sin(Angles[I]);
                AxisEnds[I] = new Point(X, Y);
            }
            AxisTop.X1 = CenterX; AxisTop.Y1 = CenterY; AxisTop.X2 = AxisEnds[0].X; AxisTop.Y2 = AxisEnds[0].Y;
            AxisTopRight.X1 = CenterX; AxisTopRight.Y1 = CenterY; AxisTopRight.X2 = AxisEnds[1].X; AxisTopRight.Y2 = AxisEnds[1].Y;
            AxisBottomRight.X1 = CenterX; AxisBottomRight.Y1 = CenterY; AxisBottomRight.X2 = AxisEnds[2].X; AxisBottomRight.Y2 = AxisEnds[2].Y;
            AxisBottomLeft.X1 = CenterX; AxisBottomLeft.Y1 = CenterY; AxisBottomLeft.X2 = AxisEnds[3].X; AxisBottomLeft.Y2 = AxisEnds[3].Y;
            AxisTopLeft.X1 = CenterX; AxisTopLeft.Y1 = CenterY; AxisTopLeft.X2 = AxisEnds[4].X; AxisTopLeft.Y2 = AxisEnds[4].Y;

            double[] LabelOffsets = { 0.8, 0.6, 0.4, 0.2 };
            TextBlock[] Labels = { LabelA, LabelB, LabelC, LabelD };
            double TopAngle = Math.PI / 2;
            for (int K = 0; K < LabelOffsets.Length; K++)
            {
                double R = Radius * LabelOffsets[K];
                double X = CenterX + R * Math.Cos(TopAngle);
                double Y = CenterY - R * Math.Sin(TopAngle);
                Canvas.SetLeft(Labels[K], X - 6);
                Canvas.SetTop(Labels[K], Y - 8);
            }
        }

        private double Clamp(double Value, double Min, double Max)
        {
            if (Value < Min) return Min;
            if (Value > Max) return Max;
            return Value;
        }
    }
}
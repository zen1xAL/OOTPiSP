using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System;
using System.Collections.Generic;

namespace DrawingApp
{
    public class EllipseShape : Shape
    {
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }

        public override void Initialize(Point startPoint)
        {
            TopLeft = startPoint;
            BottomRight = startPoint;
        }

        public override void Update(Point currentPoint)
        {
            BottomRight = currentPoint;
        }

        public override UIElement Draw()
        {
            double left = Math.Min(TopLeft.X, BottomRight.X);
            double top = Math.Min(TopLeft.Y, BottomRight.Y);
            double width = Math.Abs(BottomRight.X - TopLeft.X);
            double height = Math.Abs(BottomRight.Y - TopLeft.Y);

            Ellipse ellipse = new Ellipse
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness,
                Fill = new SolidColorBrush(FillColor)
            };
            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);
            return ellipse;
        }

        public override bool IsMultiPointShape => false;

        public override Dictionary<string, object> GetSerializationData()
        {
            var data = base.GetSerializationData();
            data.Add("TopLeft", TopLeft);
            data.Add("BottomRight", BottomRight);
            return data;
        }

        public override void SetSerializationData(Dictionary<string, object> data)
        {
            Thickness = (double)data["Thickness"];
            StrokeColor = (Color)ColorConverter.ConvertFromString((string)data["StrokeColor"]);
            FillColor = (Color)ColorConverter.ConvertFromString((string)data["FillColor"]);
            TopLeft = Point.Parse((string)data["TopLeft"]);
            BottomRight = Point.Parse((string)data["BottomRight"]);
        }
    }
}
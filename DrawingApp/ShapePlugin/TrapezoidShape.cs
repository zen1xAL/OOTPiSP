using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShapePlugin
{
    public class TrapezoidShape : DrawingApp.Shape
    {
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }
        private double topWidthPercentage = 0.5;

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
            double right = Math.Max(TopLeft.X, BottomRight.X);
            double top = Math.Min(TopLeft.Y, BottomRight.Y);
            double bottom = Math.Max(TopLeft.Y, BottomRight.Y);

            double width = right - left;
            double height = bottom - top;
            double topWidth = width * topWidthPercentage;
            double offset = (width - topWidth) / 2;

            PointCollection points = new PointCollection
            {
                new Point(left, bottom),             
                new Point(left + width, bottom),   
                new Point(left + width - offset, top),
                new Point(left + offset, top)       
            };

            Polygon trapezoid = new Polygon
            {
                Points = points,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness,
                Fill = new SolidColorBrush(FillColor)
            };

            return trapezoid;
        }

        public override bool IsMultiPointShape => false;

        public override IEnumerable<UIElement> DrawPreview(Point previewPoint, double thickness, Color strokeColor)
        {
            return new List<UIElement>();
        }

        public override Dictionary<string, object> GetSerializationData()
        {
            var data = base.GetSerializationData();
            data.Add("TopLeft", TopLeft.ToString(System.Globalization.CultureInfo.InvariantCulture));
            data.Add("BottomRight", BottomRight.ToString(System.Globalization.CultureInfo.InvariantCulture));
            data.Add("TopWidthPercentage", topWidthPercentage);
            return data;
        }

        public override void SetSerializationData(Dictionary<string, object> data)
        {
            Thickness = (double)data["Thickness"];
            StrokeColor = (Color)ColorConverter.ConvertFromString((string)data["StrokeColor"]);
            FillColor = (Color)ColorConverter.ConvertFromString((string)data["FillColor"]);
            TopLeft = Point.Parse((string)data["TopLeft"]);
            BottomRight = Point.Parse((string)data["BottomRight"]);
            topWidthPercentage = (double)data["TopWidthPercentage"];
        }
    }
}
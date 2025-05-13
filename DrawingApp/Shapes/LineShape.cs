using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Collections.Generic;
using System.Globalization;

namespace DrawingApp
{
    public class LineShape : Shape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public override void Initialize(Point startPoint)
        {
            Start = startPoint;
            End = startPoint;
        }

        public override void Update(Point currentPoint)
        {
            End = currentPoint;
        }

        public override UIElement Draw()
        {
            return new Line
            {
                X1 = Start.X,
                Y1 = Start.Y,
                X2 = End.X,
                Y2 = End.Y,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness
            };
        }

        public override bool IsMultiPointShape => false;

        public override Dictionary<string, object> GetSerializationData()
        {
            var data = base.GetSerializationData();
            data.Add("Start", Start);
            data.Add("End", End);
            return data;
        }

        public override void SetSerializationData(Dictionary<string, object> data)
        {
            Thickness = (double)data["Thickness"];
            StrokeColor = (Color)ColorConverter.ConvertFromString((string)data["StrokeColor"]);
            FillColor = (Color)ColorConverter.ConvertFromString((string)data["FillColor"]);
            Start = Point.Parse((string)data["Start"]);
            End = Point.Parse((string)data["End"]);
        }
    }
}
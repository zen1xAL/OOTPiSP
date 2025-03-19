using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingApp
{
    public class PolylineShape : Shape
    {
        public List<Point> Points { get; set; } = new List<Point>();

        public override UIElement Draw()
        {
            Polyline polyline = new Polyline
            {
                Points = new PointCollection(Points),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness
                // Заливка не добавляется, так как это ломаная линия
            };
            return polyline;
        }

        public override void SetProperties(double thickness, Color strokeColor, Color fillColor)
        {
            Thickness = thickness;
            StrokeColor = strokeColor;
            FillColor = fillColor; // Не используется в Draw, но сохраняем для совместимости
        }
    }
}
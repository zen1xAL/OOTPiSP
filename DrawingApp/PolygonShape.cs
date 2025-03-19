using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingApp
{
    public class PolygonShape : Shape
    {
        public List<Point> Points { get; set; } = new List<Point>();

        public override UIElement Draw()
        {
            Polygon polygon = new Polygon
            {
                Points = new PointCollection(Points),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness,
                Fill = new SolidColorBrush(FillColor) // Добавляем заливку
            };
            return polygon;
        }

        public override void SetProperties(double thickness, Color strokeColor, Color fillColor)
        {
            Thickness = thickness;
            StrokeColor = strokeColor;
            FillColor = fillColor;
        }
    }
}
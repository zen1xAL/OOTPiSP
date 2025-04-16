using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace DrawingApp
{
    public class PolylineShape : Shape
    {
        public List<Point> Points { get; set; } = new List<Point>();

        public override void Initialize(Point startPoint)
        {
            Points.Add(startPoint);
        }

        public override void Update(Point currentPoint)
        {
        }

        public override void FinalizeShape()
        {
        }

        public override UIElement Draw()
        {
            return new Polyline
            {
                Points = new PointCollection(Points),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness
            };
        }

        public override bool IsMultiPointShape => true;

        public override IEnumerable<UIElement> DrawPreview(Point previewPoint, double thickness, Color strokeColor)
        {
            if (Points.Count == 0) return new List<UIElement>();

            var elements = new List<UIElement>();
            Point lastPoint = Points[Points.Count - 1];

            // Рисуем пунктирную линию от последней точки до текущей позиции мыши
            Line previewLine = new Line
            {
                X1 = lastPoint.X,
                Y1 = lastPoint.Y,
                X2 = previewPoint.X,
                Y2 = previewPoint.Y,
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = thickness,
                StrokeDashArray = new DoubleCollection { 4, 4 }
            };
            elements.Add(previewLine);

            return elements;
        }
    }
}
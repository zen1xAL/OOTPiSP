using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace DrawingApp
{
    public class PolygonShape : Shape
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
            if (Points.Count > 2)
            {
                Points.Add(Points[0]); // Замыкаем полигон
            }
        }

        public override UIElement Draw()
        {
            return new Polygon
            {
                Points = new PointCollection(Points),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness,
                Fill = new SolidColorBrush(FillColor)
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

            // Рисуем прозрачный предпросмотр полигона
            if (Points.Count > 1)
            {
                PolygonShape previewPolygon = new PolygonShape();
                foreach (var point in Points)
                {
                    previewPolygon.Points.Add(point);
                }
                previewPolygon.Points.Add(previewPoint);
                previewPolygon.SetProperties(thickness, strokeColor, FillColor);

                UIElement previewElement = previewPolygon.Draw();
                if (previewElement is System.Windows.Shapes.Polygon polygon)
                {
                    polygon.Opacity = 0.5;
                }
                elements.Add(previewElement);
            }

            return elements;
        }
    }
}
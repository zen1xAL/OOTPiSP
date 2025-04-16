using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShapePlugin
{
    public class TriangleShape : DrawingApp.Shape
    {
        public Point Point1 { get; set; }
        public Point Point2 { get; set; }
        public Point Point3 { get; set; }

        public TriangleShape()
        {
        }

        public override void Initialize(Point startPoint)
        {
            Point1 = startPoint;
            Point2 = startPoint;
            Point3 = startPoint;
        }

        public override void Update(Point currentPoint)
        {
            Point2 = new Point((Point1.X + currentPoint.X) / 2, Point1.Y);
            Point3 = currentPoint;
        }

        public override void FinalizeShape()
        {
        }

        public override UIElement Draw()
        {
            Polygon triangle = new Polygon
            {
                Points = new PointCollection(new[] { Point1, Point2, Point3 }),
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness,
                Fill = new SolidColorBrush(FillColor)
            };
            return triangle;
        }

        public override bool IsMultiPointShape => false;

        public override IEnumerable<UIElement> DrawPreview(Point previewPoint, double thickness, Color strokeColor)
        {
            return new List<UIElement>();
        }
    }
}
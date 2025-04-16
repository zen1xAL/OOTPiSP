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

        public override void FinalizeShape()
        {
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

        public override IEnumerable<UIElement> DrawPreview(Point previewPoint, double thickness, Color strokeColor)
        {
            return new List<UIElement>(); // Эллипсу не нужен предпросмотр
        }
    }
}
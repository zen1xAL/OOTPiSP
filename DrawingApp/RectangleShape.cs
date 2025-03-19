using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingApp
{
    public class RectangleShape : Shape
    {
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }

        public override UIElement Draw()
        {
            double left = Math.Min(TopLeft.X, BottomRight.X);
            double top = Math.Min(TopLeft.Y, BottomRight.Y);
            double width = Math.Abs(BottomRight.X - TopLeft.X);
            double height = Math.Abs(BottomRight.Y - TopLeft.Y);

            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness,
                Fill = new SolidColorBrush(FillColor)
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            return rect;
        }

        public override void SetProperties(double thickness, Color strokeColor, Color fillColor)
        {
            base.SetProperties(thickness, strokeColor, fillColor);
        }
    }
}
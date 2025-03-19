using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingApp
{
    public class LineShape : Shape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public override UIElement Draw()
        {
            Line line = new Line
            {
                X1 = Start.X,
                Y1 = Start.Y,
                X2 = End.X,
                Y2 = End.Y,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = Thickness
            };
            return line;
        }

        public override void SetProperties(double thickness, Color strokeColor, Color fillColor)
        {
            Thickness = thickness;
            StrokeColor = strokeColor;
            FillColor = fillColor; 
        }
    }
}
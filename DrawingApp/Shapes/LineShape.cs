using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Collections.Generic;

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

        public override void FinalizeShape()
        {
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

        public override IEnumerable<UIElement> DrawPreview(Point previewPoint, double thickness, Color strokeColor)
        {
            return new List<UIElement>(); // Линии не нужен предпросмотр
        }
    }
}
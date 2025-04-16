using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DrawingApp
{
    public abstract class Shape
    {
        public double Thickness { get; set; }
        public Color StrokeColor { get; set; }
        public Color FillColor { get; set; }

        public abstract UIElement Draw();
        public virtual void SetProperties(double thickness, Color strokeColor, Color fillColor)
        {
            Thickness = thickness;
            StrokeColor = strokeColor;
            FillColor = fillColor;
        }

        public abstract void Initialize(Point startPoint);
        public abstract void Update(Point currentPoint);
        public abstract void FinalizeShape();
        public abstract bool IsMultiPointShape { get; }

        public abstract IEnumerable<UIElement> DrawPreview(Point previewPoint, double thickness, Color strokeColor);
    }
}
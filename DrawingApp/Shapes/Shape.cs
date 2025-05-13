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
        public virtual void Update(Point currentPoint)
        {
        }

        public virtual void FinalizeShape()
        {
        }

        public abstract bool IsMultiPointShape { get; }

        public virtual IEnumerable<UIElement> DrawPreview(Point previewPoint, double thickness, Color strokeColor)
        {
            return new List<UIElement>();
        }

        public virtual Dictionary<string, object> GetSerializationData()
        {
            return new Dictionary<string, object>
            {
                { "Thickness", Thickness },
                { "StrokeColor", StrokeColor.ToString() },
                { "FillColor", FillColor.ToString() }
            };
        }

        public abstract void SetSerializationData(Dictionary<string, object> data);
    }
}
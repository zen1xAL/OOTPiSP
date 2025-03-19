using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

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
    }
}
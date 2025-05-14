using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace DrawingApp
{
    public static class ColorManager
    {
        private static readonly Dictionary<string, Color> colorMap = new Dictionary<string, Color>
        {
            { "Red", Colors.Red },
            { "Blue", Colors.Blue },
            { "Green", Colors.Green },
            { "Black", Colors.Black },
            { "None", Colors.Transparent }
        };

        public static Color GetColorFromName(string colorName)
        {
            if (string.IsNullOrEmpty(colorName))
            {
			
                return Colors.Black;
            }

            return colorMap.TryGetValue(colorName, out Color color) ? color : Colors.Black;
        }

       
        public static void RegisterColor(string name, Color color)
        {
            if (!string.IsNullOrEmpty(name) && !colorMap.ContainsKey(name))
            {
                colorMap[name] = color;
            }
        }
    }
}
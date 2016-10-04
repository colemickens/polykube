using System;
using System.Globalization;

namespace Api.Services.Color
{
    public class Color
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public Color(int red, int green, int blue)
        {
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
        }

        public Color(string hexColor)
        {
            if (hexColor.IndexOf('#') != -1)
            {
                hexColor = hexColor.Replace("#", "");
            }
 
            if (hexColor.Length == 6)
            {
                this.Red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                this.Green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                this.Blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            }
            else if (hexColor.Length == 3)
            {
                this.Red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
                this.Green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
                this.Blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
            }
        }
    }
}

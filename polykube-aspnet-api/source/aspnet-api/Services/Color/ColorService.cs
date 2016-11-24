using System;

namespace Api.Services.Color
{
    public class ColorService : IColorService
    {
        private Random rand = new Random(Guid.NewGuid().GetHashCode());

        private Color[] blueColors = new Color[]{
            new Color("#2196F3"), new Color("#E3F2FD"), new Color("#BBDEFB"), new Color("#90CAF9"),
            new Color("#64B5F6"), new Color("#42A5F5"), new Color("#2196F3"), new Color("#1E88E5"),
            new Color("#1976D2"), new Color("#1565C0"), new Color("#0D47A1"),
        };

        public Color GetColorShade()
        {
            var colors = this.blueColors;

            var idx = this.rand.Next(colors.Length);
            return colors[idx];
        }
    }
}

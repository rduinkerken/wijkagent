using System.Windows.Media;

namespace Find_My_Boef.Controller
{
    public static class Helper
    {
        // This class supports making of SolidColorBrush directly from Argb
        private static readonly SolidColorBrush[] s_colors = { FromArgb(200, 0, 50, 200), FromArgb(200, 200, 50, 50), FromArgb(200, 0, 200, 50) };
        public static SolidColorBrush FromArgb(byte a, byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        public static SolidColorBrush RandomColor()
        {
            return s_colors[MainInstance.MainRandom.Next(0, s_colors.Length)];
        }
    }
}

using System.Drawing.Drawing2D;
using System.Drawing;

namespace RoastMyCode.Extensions
{
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle bounds, int cornerRadius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int diameter = cornerRadius * 2;
                path.AddArc(new Rectangle(bounds.Left, bounds.Top, diameter, diameter), 180, 90);
                path.AddArc(new Rectangle(bounds.Right - diameter, bounds.Top, diameter, diameter), 270, 90);
                path.AddArc(new Rectangle(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter), 0, 90);
                path.AddArc(new Rectangle(bounds.Left, bounds.Bottom - diameter, diameter, diameter), 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }
}

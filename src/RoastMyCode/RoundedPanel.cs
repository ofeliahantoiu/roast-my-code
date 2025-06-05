using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RoastMyCode
{
    /// <summary>
    /// Panel with rounded corners
    /// </summary>
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 18;
        public Color BorderColor { get; set; } = Color.Gray;
        public int BorderWidth { get; set; } = 1;

        public RoundedPanel()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.BackColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = GetRoundedRect(rect, CornerRadius))
            using (var brush = new SolidBrush(this.BackColor))
            using (var pen = new Pen(BorderColor, BorderWidth))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

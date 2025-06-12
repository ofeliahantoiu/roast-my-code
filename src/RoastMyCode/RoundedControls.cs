using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RoastMyCode
{
    public class RoundedRichTextBox : RichTextBox
    {
        private int cornerRadius = 20;
        private Color borderColor = Color.Transparent;
        private int borderWidth = 0;
        private string _placeholderText = string.Empty;
        private Color _placeholderColor = Color.FromArgb(150, 150, 150);

        public string PlaceholderText
        {
            get => _placeholderText;
            set
            {
                _placeholderText = value;
                Invalidate();
            }
        }

        public Color PlaceholderColor
        {
            get => _placeholderColor;
            set
            {
                _placeholderColor = value;
                Invalidate();
            }
        }

        public int CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public int BorderWidth
        {
            get => borderWidth;
            set { borderWidth = value; Invalidate(); }
        }

        public RoundedRichTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                int radius = CornerRadius;
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseAllFigures();

                this.Region = new Region(path);

                if (borderWidth > 0)
                {
                    using (Pen pen = new Pen(BorderColor, BorderWidth))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                // Draw placeholder text if control is empty
                if (Text.Length == 0 && !Focused && !string.IsNullOrEmpty(_placeholderText))
                {
                    using (Brush brush = new SolidBrush(_placeholderColor))
                    {
                        SizeF stringSize = e.Graphics.MeasureString(_placeholderText, Font);
                        PointF location = new PointF(
                            (Width - stringSize.Width) / 2,
                            (Height - stringSize.Height) / 2
                        );
                        e.Graphics.DrawString(_placeholderText, Font, brush, location);
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}

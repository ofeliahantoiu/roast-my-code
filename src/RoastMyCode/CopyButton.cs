using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RoastMyCode
{
    /// <summary>
    /// A custom button control that appears when hovering over text to provide copy functionality
    /// </summary>
    public class CopyButton : UserControl
    {
        private bool _isHovered = false;
        private readonly int _cornerRadius = 6;
        private readonly Color _normalColor = Color.FromArgb(60, 60, 60);
        private readonly Color _hoverColor = Color.FromArgb(80, 80, 80);
        private readonly Color _iconColor = Color.FromArgb(220, 220, 220);
        private readonly ToolTip _toolTip = new ToolTip();

        public event EventHandler CopyClicked;

        public CopyButton()
        {
            Size = new Size(32, 32);
            Cursor = Cursors.Hand;
            Visible = false; // Initially hidden
            _toolTip.SetToolTip(this, "Copy text");

            // Enable double buffering for smoother rendering
            SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                     ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw rounded rectangle background
            using (GraphicsPath path = CreateRoundedRectangle(ClientRectangle, _cornerRadius))
            using (SolidBrush brush = new SolidBrush(_isHovered ? _hoverColor : _normalColor))
            {
                g.FillPath(brush, path);
            }

            // Draw copy icon
            using (Pen pen = new Pen(_iconColor, 1.5f))
            {
                // Draw document shape
                int offset = 8;
                int width = 16;
                int height = 16;

                // First document (behind)
                Rectangle docRect = new Rectangle(
                    offset - 1, 
                    offset - 1, 
                    width, 
                    height
                );
                
                // Clip the corner
                int cornerClip = 4;
                using (GraphicsPath docPath = new GraphicsPath())
                {
                    docPath.AddLine(docRect.X, docRect.Y, docRect.Right - cornerClip, docRect.Y);
                    docPath.AddLine(docRect.Right - cornerClip, docRect.Y, docRect.Right, docRect.Y + cornerClip);
                    docPath.AddLine(docRect.Right, docRect.Y + cornerClip, docRect.Right, docRect.Bottom);
                    docPath.AddLine(docRect.Right, docRect.Bottom, docRect.X, docRect.Bottom);
                    docPath.CloseFigure();
                    g.FillPath(new SolidBrush(_iconColor), docPath);
                }
            }
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            // Top left arc
            path.AddArc(arc, 180, 90);

            // Top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            CopyClicked?.Invoke(this, e);
        }

        /// <summary>
        /// Shows the copy button with a brief fade-in animation
        /// </summary>
        public void ShowWithAnimation()
        {
            if (Visible) return;
            
            this.Opacity = 0;
            this.Visible = true;
            
            Timer fadeTimer = new Timer();
            fadeTimer.Interval = 20;
            fadeTimer.Tick += (s, e) =>
            {
                this.Opacity += 0.1;
                if (this.Opacity >= 1)
                {
                    this.Opacity = 1;
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                }
            };
            fadeTimer.Start();
        }

        /// <summary>
        /// Hides the copy button with a brief fade-out animation
        /// </summary>
        public void HideWithAnimation()
        {
            if (!Visible) return;
            
            Timer fadeTimer = new Timer();
            fadeTimer.Interval = 20;
            fadeTimer.Tick += (s, e) =>
            {
                this.Opacity -= 0.1;
                if (this.Opacity <= 0)
                {
                    this.Opacity = 0;
                    this.Visible = false;
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                }
            };
            fadeTimer.Start();
        }
    }
}

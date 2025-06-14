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
        private readonly Color _normalColor = Color.FromArgb(100, 100, 100);
        private readonly Color _hoverColor = Color.FromArgb(130, 130, 130);
        private readonly Color _iconColor = Color.FromArgb(255, 255, 255);
        private readonly ToolTip _toolTip = new ToolTip();
        private double _opacity = 1.0;
        
        /// <summary>
        /// Gets or sets the opacity of the control (0.0 to 1.0)
        /// </summary>
        public double Opacity
        {
            get => _opacity;
            set
            {
                _opacity = Math.Clamp(value, 0.0, 1.0);
                this.Invalidate();
            }
        }

        public event EventHandler? CopyClicked;

        public CopyButton()
        {
            Size = new Size(80, 32); // Wider to accommodate text
            Cursor = Cursors.Hand;
            Visible = false; // Initially hidden
            _toolTip.SetToolTip(this, "Copy text");
            BackColor = Color.Transparent;

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

            // Apply opacity to colors
            Color backgroundColor = _isHovered ? _hoverColor : _normalColor;
            Color iconColor = _iconColor;
            Color textColor = _iconColor;
            
            if (_opacity < 1.0)
            {
                int alpha = (int)(_opacity * 255);
                backgroundColor = Color.FromArgb(alpha, backgroundColor);
                iconColor = Color.FromArgb(alpha, iconColor);
                textColor = Color.FromArgb(alpha, textColor);
            }

            // Draw rounded rectangle background
            using (GraphicsPath path = CreateRoundedRectangle(ClientRectangle, _cornerRadius))
            using (SolidBrush brush = new SolidBrush(backgroundColor))
            {
                g.FillPath(brush, path);
            }

            // Draw copy icon
            using (Pen pen = new Pen(iconColor, 1.5f))
            {
                // Draw document shape
                int offset = 8;
                int iconSize = 16;
                int cornerClip = 4;
                
                Rectangle docRect = new Rectangle(
                    offset, 
                    (Height - iconSize) / 2, 
                    iconSize, 
                    iconSize);
                
                // Draw the document with a clipped corner
                using (GraphicsPath docPath = new GraphicsPath())
                {
                    docPath.AddLine(docRect.X, docRect.Y, docRect.Right - cornerClip, docRect.Y);
                    docPath.AddLine(docRect.Right - cornerClip, docRect.Y, docRect.Right, docRect.Y + cornerClip);
                    docPath.AddLine(docRect.Right, docRect.Y + cornerClip, docRect.Right, docRect.Bottom);
                    docPath.AddLine(docRect.Right, docRect.Bottom, docRect.X, docRect.Bottom);
                    docPath.CloseFigure();
                    g.FillPath(new SolidBrush(iconColor), docPath);
                }
                
                // Draw "Copy" text
                using (Font textFont = new Font("Segoe UI", 9f, FontStyle.Regular))
                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    int textX = offset + iconSize + 4;
                    int textY = (Height - iconSize) / 2;
                    g.DrawString("Copy", textFont, textBrush, textX, textY);
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
            
            System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer();
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
            
            System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer();
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

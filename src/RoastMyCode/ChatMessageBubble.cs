using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

namespace RoastMyCode
{
    public partial class ChatMessageBubble : UserControl
    {
        private string _messageText = string.Empty;
        private string _role = "assistant";

        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                Invalidate();
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                Invalidate();
            }
        }



        public ChatMessageBubble()
        {
            InitializeComponent();
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(100, 40);
            MaximumSize = new Size(600, 0);
            Padding = new Padding(15, 10, 15, 10);
            Margin = new Padding(12, 8, 12, 8);
            Font = new Font("Segoe UI", 12);
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "ChatMessageBubble";
            this.ResumeLayout(false);
        }

        public void SetMessage(string message, string role)
        {
            _messageText = message;
            _role = role;
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (Parent == null || string.IsNullOrEmpty(_messageText)) return;

            using (Graphics g = CreateGraphics())
            {
                TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
                int availableWidth = MaximumSize.Width - Padding.Horizontal;
                Size proposedSize = new Size(availableWidth, int.MaxValue);
                Size textSize = TextRenderer.MeasureText(_messageText, Font, proposedSize, flags);

                int bubbleWidth = Math.Min(textSize.Width + Padding.Horizontal + 8, MaximumSize.Width);
                this.Width = bubbleWidth;
                this.Height = textSize.Height + Padding.Vertical + 8;

                if (_role == "user")
                {
                    if (Parent != null)
                    {
                        this.Left = Parent.ClientSize.Width - this.Width - Margin.Right;
                    }
                }
                else
                {
                    this.Left = Margin.Left;
                }

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color backColor = (_role == "user") ? Color.FromArgb(70, 70, 70) : Color.FromArgb(70, 70, 70);
            Color textColor = Color.White;
            int cornerRadius = 12;

            Rectangle bubbleBounds = new Rectangle(0, 0, this.Width, this.Height);
            using (GraphicsPath path = GetRoundedRect(bubbleBounds, cornerRadius))
            {
                using (Brush brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(Color.FromArgb(100, 100, 100)))
                {
                    g.DrawPath(pen, path);
                }
            }

            TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
            if (_role == "user")
            {
                flags |= TextFormatFlags.Right;
            }
            TextRenderer.DrawText(e.Graphics, _messageText, Font, bubbleBounds, textColor, flags);
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            
            // Top-left corner
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            
            // Top-right corner
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            
            // Bottom-right corner
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            
            // Bottom-left corner
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            
            path.CloseFigure();
            return path;
        }
    }
}

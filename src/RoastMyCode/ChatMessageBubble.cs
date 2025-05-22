using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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
            MinimumSize = new Size(100, 30);
            Padding = new Padding(12, 8, 12, 8);
            Margin = new Padding(10, 6, 10, 6);
            Font = new Font("Segoe UI", 12);
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "ChatMessageBubble";
            this.ResumeLayout(false);
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
        Size textSize = TextRenderer.MeasureText(g, _messageText, Font, proposedSize, flags);

        this.Height = textSize.Height + Padding.Vertical;
    }

    this.Anchor = (_role == "user") ? AnchorStyles.Right : AnchorStyles.Left;
    Invalidate();
}

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color backColor = (_role == "user") ? Color.FromArgb(60, 120, 230) : Color.FromArgb(70, 70, 70);
            Color textColor = Color.White;
            int cornerRadius = 12;

            Rectangle bubbleBounds = new Rectangle(0, 0, this.Width, this.Height);

            using (GraphicsPath path = new GraphicsPath())
            {
                int diameter = cornerRadius * 2;
                Rectangle arc = new Rectangle(bubbleBounds.X, bubbleBounds.Y, diameter, diameter);

                path.AddArc(arc, 180, 90); // Top-left
                arc.X = bubbleBounds.Right - diameter;
                path.AddArc(arc, 270, 90); // Top-right
                arc.Y = bubbleBounds.Bottom - diameter;
                path.AddArc(arc, 0, 90); // Bottom-right
                arc.X = bubbleBounds.X;
                path.AddArc(arc, 90, 90); // Bottom-left
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }
            }

            Rectangle textRect = new Rectangle(
                bubbleBounds.X + Padding.Left,
                bubbleBounds.Y + Padding.Top,
                bubbleBounds.Width - Padding.Horizontal,
                bubbleBounds.Height - Padding.Vertical
            );

            TextFormatFlags textFlags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.Top;
            textFlags |= (_role == "user") ? TextFormatFlags.Right : TextFormatFlags.Left;

            TextRenderer.DrawText(g, _messageText, Font, textRect, textColor, textFlags);
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RoastMyCode.Extensions;
using RoastMyCode.Services;

namespace RoastMyCode
{
    public partial class ChatMessageBubble : UserControl
    {
        private string _messageText = string.Empty;
        private string _role = "assistant";
        private string _language = "text";

        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                if (value.StartsWith("📷"))
                {
                    _language = "PHOTO";
                }
                else
                {
                    _language = LanguageDetector.DetectLanguage(value);
                }
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
            MinimumSize = new Size(100, 80);
            MaximumSize = new Size(600, 0);
            Padding = new Padding(20);
            Margin = new Padding(12);
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

                // Language label space
                int languageLabelHeight = 0;
                if (_role == "user" || _language == "PHOTO")
                {
                    Size languageSize = TextRenderer.MeasureText(_language.ToUpper(), Font);
                    languageLabelHeight = languageSize.Height + 10;
                }

                int minHeight = Math.Max(textSize.Height + Padding.Vertical, 80);
                int totalHeight = minHeight + languageLabelHeight;

                // Set width and height
                this.Width = Math.Min(textSize.Width + Padding.Horizontal, MaximumSize.Width);
                this.Height = Math.Max(totalHeight, MinimumSize.Height);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color backColor = Color.FromArgb(70, 70, 70);
            Color textColor = Color.White;
            int cornerRadius = 12;

            int labelOffsetY = 0;

            if (_role == "user" || _language == "PHOTO")
            {
                Size langSize = TextRenderer.MeasureText(_language.ToUpper(), Font);
                int labelPadding = 8;
                int labelWidth = langSize.Width + labelPadding * 2;
                int labelHeight = langSize.Height + labelPadding;

                Rectangle labelRect = new Rectangle(
                    (this.Width - labelWidth) / 2,
                    0,
                    labelWidth,
                    labelHeight
                );

                using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(50, 50, 50)))
                {
                    g.FillRoundedRectangle(labelBrush, labelRect, 6);
                }

                TextRenderer.DrawText(
                    g,
                    _language.ToUpper(),
                    Font,
                    new Point(labelRect.Left + labelPadding, labelRect.Top + labelPadding / 2),
                    textColor
                );

                labelOffsetY = labelRect.Bottom + 6;
            }

            int bubbleHeight = this.Height - labelOffsetY;
            Rectangle bubbleRect = new Rectangle(
                0,
                labelOffsetY,
                this.Width,
                bubbleHeight
            );

            using (GraphicsPath path = new GraphicsPath())
            {
                int diameter = cornerRadius * 2;
                path.AddArc(new Rectangle(bubbleRect.Left, bubbleRect.Top, diameter, diameter), 180, 90);
                path.AddArc(new Rectangle(bubbleRect.Right - diameter, bubbleRect.Top, diameter, diameter), 270, 90);
                path.AddArc(new Rectangle(bubbleRect.Right - diameter, bubbleRect.Bottom - diameter, diameter, diameter), 0, 90);
                path.AddArc(new Rectangle(bubbleRect.Left, bubbleRect.Bottom - diameter, diameter, diameter), 90, 90);
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }
            }

            Rectangle textRect = new Rectangle(
                Padding.Left,
                bubbleRect.Top + Padding.Top,
                this.Width - Padding.Horizontal,
                bubbleRect.Height - Padding.Vertical
            );

            TextRenderer.DrawText(
                g,
                _messageText,
                Font,
                textRect,
                textColor,
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter
            );
        }
    }
}

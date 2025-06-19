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
        private Image? _image;

        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                _language = LanguageDetector.DetectLanguage(value);
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

        public Image? Image
        {
            get => _image;
            set
            {
                _image?.Dispose();
                _image = value;
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

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (Parent == null) return;

            using (Graphics g = CreateGraphics())
            {
                int availableWidth = MaximumSize.Width - Padding.Horizontal;
                int totalHeight = 0;
                int contentWidth = 0;

                // Calculate text dimensions
                if (!string.IsNullOrEmpty(_messageText))
                {
                    TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
                    Size proposedSize = new Size(availableWidth, int.MaxValue);
                    Size textSize = TextRenderer.MeasureText(_messageText, Font, proposedSize, flags);
                    contentWidth = Math.Max(contentWidth, textSize.Width);
                    totalHeight += textSize.Height;
                }

                // Calculate image dimensions
                if (_image != null)
                {
                    try
                    {
                        // Validate the image before using it
                        if (_image.Width <= 0 || _image.Height <= 0)
                        {
                            // Image is invalid, skip it
                            return;
                        }

                        int maxImageWidth = Math.Min(400, availableWidth);
                        int maxImageHeight = 300;
                        
                        double scaleX = (double)maxImageWidth / _image.Width;
                        double scaleY = (double)maxImageHeight / _image.Height;
                        double scale = Math.Min(scaleX, scaleY);
                        
                        int imageWidth = (int)(_image.Width * scale);
                        int imageHeight = (int)(_image.Height * scale);
                        
                        contentWidth = Math.Max(contentWidth, imageWidth);
                        totalHeight += imageHeight;
                        
                        // Add spacing between text and image
                        if (!string.IsNullOrEmpty(_messageText))
                        {
                            totalHeight += 10;
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Image is invalid or disposed, skip it
                        return;
                    }
                    catch (Exception)
                    {
                        // Any other image-related error, skip it
                        return;
                    }
                }

                // Language label space
                int languageLabelHeight = 0;
                if (_role == "user")
                {
                    Size languageSize = TextRenderer.MeasureText(_language.ToUpper(), Font);
                    languageLabelHeight = languageSize.Height + 12;
                }

                int minHeight = Math.Max(totalHeight + Padding.Vertical * 2, 30);
                int finalHeight = minHeight + languageLabelHeight + 10;

                this.Width = Math.Min(contentWidth + Padding.Horizontal + 8, MaximumSize.Width);
                this.Height = Math.Max(finalHeight, 50);

                this.Left = (_role == "user")
                    ? Parent.ClientSize.Width - this.Width - Margin.Right
                    : Margin.Left;

                Invalidate();
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

            if (_role == "user")
            {
                Size langSize = TextRenderer.MeasureText(_language.ToUpper(), Font);
                int labelPadding = 6;
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

                labelOffsetY = labelRect.Bottom + 5;
            }

            int bubbleHeight = this.Height - labelOffsetY;
            if (bubbleHeight < 30)
            {
                bubbleHeight = 30;
                this.Height = bubbleHeight + labelOffsetY;
                Invalidate();
                return;
            }

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

            int currentY = bubbleRect.Top + Padding.Top;

            // Draw text if present
            if (!string.IsNullOrEmpty(_messageText))
            {
                Rectangle textRect = new Rectangle(
                    Padding.Left,
                    currentY,
                    this.Width - Padding.Horizontal,
                    Math.Max(bubbleRect.Height - Padding.Vertical, 20)
                );

                TextRenderer.DrawText(
                    g,
                    _messageText,
                    Font,
                    textRect,
                    textColor,
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl
                );

                // Calculate text height for image positioning
                Size textSize = TextRenderer.MeasureText(_messageText, Font, new Size(this.Width - Padding.Horizontal, int.MaxValue), 
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                currentY += textSize.Height + 10;
            }

            // Draw image if present
            if (_image != null)
            {
                try
                {
                    // Validate the image before using it
                    if (_image.Width <= 0 || _image.Height <= 0)
                    {
                        // Image is invalid, skip drawing
                        return;
                    }

                    int maxImageWidth = Math.Min(400, this.Width - Padding.Horizontal);
                    int maxImageHeight = 300;

                    double scaleX = (double)maxImageWidth / _image.Width;
                    double scaleY = (double)maxImageHeight / _image.Height;
                    double scale = Math.Min(scaleX, scaleY);

                    int imageWidth = (int)(_image.Width * scale);
                    int imageHeight = (int)(_image.Height * scale);

                    Rectangle imageRect = new Rectangle(
                        Padding.Left + (this.Width - Padding.Horizontal - imageWidth) / 2,
                        currentY,
                        imageWidth,
                        imageHeight
                    );

                    g.DrawImage(_image, imageRect);
                }
                catch (ArgumentException)
                {
                    // Image is invalid or disposed, skip drawing
                    return;
                }
                catch (Exception)
                {
                    // Any other image-related error, skip drawing
                    return;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

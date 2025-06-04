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
        private bool _isDarkMode = true;

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
        
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
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

            // Colors based on role and dark/light mode
            Color backColor;
            Color textColor;
            Color borderColor;
            
            if (_isDarkMode)
            {
                // Dark mode colors
                if (_role == "user")
                {
                    backColor = Color.FromArgb(70, 70, 70);  // Dark gray for user
                    borderColor = Color.FromArgb(90, 90, 90); // Slightly lighter border
                }
                else
                {
                    backColor = Color.FromArgb(50, 50, 80);  // Dark blue for assistant
                    borderColor = Color.FromArgb(70, 70, 100); // Slightly lighter border
                }
                textColor = Color.White;
            }
            else
            {
                // Light mode colors
                if (_role == "user")
                {
                    backColor = Color.FromArgb(220, 220, 220); // Light gray for user
                    borderColor = Color.FromArgb(200, 200, 200); // Slightly darker border
                }
                else
                {
                    backColor = Color.FromArgb(220, 220, 240); // Light blue for assistant
                    borderColor = Color.FromArgb(200, 200, 220); // Slightly darker border
                }
                textColor = Color.Black;
            }
            
            int cornerRadius = 12;
            Rectangle bubbleBounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = new GraphicsPath())
            {
                int diameter = cornerRadius * 2;
                Rectangle arc = new Rectangle(bubbleBounds.X, bubbleBounds.Y, diameter, diameter);

                path.AddArc(arc, 180, 90); 
                arc.X = bubbleBounds.Right - diameter;
                path.AddArc(arc, 270, 90); 
                arc.Y = bubbleBounds.Bottom - diameter;
                path.AddArc(arc, 0, 90); 
                arc.X = bubbleBounds.X;
                path.AddArc(arc, 90, 90); 
                path.CloseFigure();

                // Fill bubble background
                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }
                
                // Draw border
                using (Pen pen = new Pen(borderColor, 1))
                {
                    g.DrawPath(pen, path);
                }
            }

            Rectangle textRect = new Rectangle(
                Padding.Left,
                Padding.Top,
                this.Width - Padding.Horizontal,
                this.Height - Padding.Vertical
            );

            TextFormatFlags textFlags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter;
            
            TextRenderer.DrawText(g, _messageText, Font, textRect, textColor, textFlags);
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RoastMyCode
{
    /// <summary>
    /// A custom control that displays the detected programming language as a badge.
    /// </summary>
    public class LanguageBadge : UserControl
    {
        private string _language = "Unknown";
        private Color _badgeColor = Color.FromArgb(60, 60, 60);
        private Color _textColor = Color.White;
        private readonly Font _badgeFont = new Font("Segoe UI", 9f, FontStyle.Regular);
        private int _cornerRadius = 8;
        private readonly Padding _textPadding = new Padding(8, 4, 8, 4);

        /// <summary>
        /// Gets or sets the language displayed in the badge.
        /// </summary>
        public string Language
        {
            get => _language;
            set
            {
                _language = value;
                UpdateSize();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background color of the badge.
        /// </summary>
        public Color BadgeColor
        {
            get => _badgeColor;
            set
            {
                _badgeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the text color of the badge.
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the corner radius of the badge.
        /// </summary>
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Initializes a new instance of the LanguageBadge class.
        /// </summary>
        public LanguageBadge()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | 
                    ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.OptimizedDoubleBuffer | 
                    ControlStyles.ResizeRedraw, true);
            UpdateSize();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "LanguageBadge";
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Updates the size of the badge based on the text content.
        /// </summary>
        private void UpdateSize()
        {
            using (Graphics g = CreateGraphics())
            {
                SizeF textSize = g.MeasureString(_language, _badgeFont);
                this.Size = new Size(
                    (int)textSize.Width + _textPadding.Horizontal,
                    (int)textSize.Height + _textPadding.Vertical
                );
            }
        }

        /// <summary>
        /// Paints the badge with the language text.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Get language-specific color
            Color badgeColor = GetLanguageColor(_language);

            // Draw rounded rectangle background
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = CreateRoundedRectangle(rect, _cornerRadius))
            {
                using (SolidBrush brush = new SolidBrush(badgeColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw text
            using (SolidBrush textBrush = new SolidBrush(_textColor))
            {
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(_language, _badgeFont, textBrush, rect, format);
            }
        }

        /// <summary>
        /// Creates a rounded rectangle GraphicsPath.
        /// </summary>
        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top left corner
            path.AddArc(arcRect, 180, 90);

            // Top right corner
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // Bottom right corner
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // Bottom left corner
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Gets a color associated with the specified programming language.
        /// </summary>
        private Color GetLanguageColor(string language)
        {
            return language.ToLowerInvariant() switch
            {
                "c#" => Color.FromArgb(104, 33, 122),    // Purple
                "javascript" => Color.FromArgb(240, 219, 79),  // Yellow
                "typescript" => Color.FromArgb(49, 120, 198),  // Blue
                "python" => Color.FromArgb(53, 114, 165),      // Blue
                "java" => Color.FromArgb(176, 114, 25),        // Brown
                "c++" => Color.FromArgb(86, 76, 149),          // Purple
                "c" => Color.FromArgb(85, 85, 85),             // Gray
                "php" => Color.FromArgb(79, 93, 149),          // Blue-purple
                "ruby" => Color.FromArgb(169, 0, 0),           // Red
                "go" => Color.FromArgb(0, 173, 216),           // Light blue
                "rust" => Color.FromArgb(183, 65, 14),         // Rust color
                "swift" => Color.FromArgb(252, 79, 51),        // Orange-red
                "kotlin" => Color.FromArgb(143, 100, 174),     // Purple
                "dart" => Color.FromArgb(0, 180, 171),         // Teal
                "html" => Color.FromArgb(227, 76, 38),         // Orange
                "css" => Color.FromArgb(38, 77, 228),          // Blue
                "xml" => Color.FromArgb(97, 175, 239),         // Light blue
                "json" => Color.FromArgb(252, 152, 3),         // Orange
                "yaml" => Color.FromArgb(91, 163, 138),        // Green-blue
                "markdown" => Color.FromArgb(0, 0, 0),         // Black
                _ => Color.FromArgb(60, 60, 60)                // Dark gray (default)
            };
        }
    }
}

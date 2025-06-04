using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RoastMyCode
{
    /// <summary>
    /// A control that displays the currently detected programming language in the corner of the application.
    /// </summary>
    public class CurrentLanguageDisplay : UserControl
    {
        private string _language = "None";
        private readonly Font _titleFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        private readonly Font _languageFont = new Font("Segoe UI", 11f, FontStyle.Bold);
        private readonly int _cornerRadius = 10;
        private readonly Color _darkBackColor = Color.FromArgb(60, 60, 60);
        private readonly Color _lightBackColor = Color.FromArgb(240, 240, 240);
        private bool _isDarkMode = true;

        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    Invalidate();
                }
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    Invalidate();
                }
            }
        }

        public CurrentLanguageDisplay()
        {
            SetStyle(ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw, true);
            
            Size = new Size(200, 80);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Background
            Color backColor = _isDarkMode ? _darkBackColor : _lightBackColor;
            Color textColor = _isDarkMode ? Color.White : Color.Black;
            Color accentColor = GetLanguageColor(_language);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = CreateRoundedRectangle(rect, _cornerRadius))
            {
                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }

                // Add a subtle border
                using (Pen borderPen = new Pen(_isDarkMode ? Color.FromArgb(80, 80, 80) : Color.FromArgb(200, 200, 200), 1))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title text
            string titleText = "Selected programming language:";
            Rectangle titleRect = new Rectangle(10, 10, Width - 20, 20);
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                g.DrawString(titleText, _titleFont, textBrush, titleRect);
            }

            // Language text with colored badge
            Rectangle languageRect = new Rectangle(10, 35, Width - 20, 30);
            
            // Draw language badge background
            int badgeWidth = Math.Min(Width - 20, (int)g.MeasureString(_language, _languageFont).Width + 20);
            Rectangle badgeRect = new Rectangle(10, 35, badgeWidth, 30);
            
            using (GraphicsPath badgePath = CreateRoundedRectangle(badgeRect, 5))
            using (SolidBrush badgeBrush = new SolidBrush(accentColor))
            {
                g.FillPath(badgeBrush, badgePath);
            }
            
            // Draw language text
            using (SolidBrush textBrush = new SolidBrush(GetTextColorForBackground(accentColor)))
            {
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(_language, _languageFont, textBrush, 
                    new Rectangle(badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height), format);
            }
        }

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

        private Color GetLanguageColor(string language)
        {
            return language.ToLowerInvariant() switch
            {
                "c#" => Color.FromArgb(155, 50, 183),    // Brighter Purple
                "javascript" => Color.FromArgb(240, 219, 79),  // Yellow
                "typescript" => Color.FromArgb(49, 120, 198),  // Blue
                "python" => Color.FromArgb(55, 118, 171),   // Blue
                "java" => Color.FromArgb(244, 85, 36),    // Orange
                "cpp" => Color.FromArgb(0, 119, 186),     // Brighter Blue
                "c" => Color.FromArgb(60, 73, 167),     // Brighter Dark Blue
                "php" => Color.FromArgb(139, 143, 199),  // Brighter Purple
                "ruby" => Color.FromArgb(224, 72, 65),    // Brighter Red
                "go" => Color.FromArgb(0, 193, 236),    // Brighter Light Blue
                "rust" => Color.FromArgb(203, 85, 34),    // Brighter Rust
                "swift" => Color.FromArgb(255, 96, 61),    // Brighter Orange
                "kotlin" => Color.FromArgb(163, 124, 246),  // Brighter Purple
                "dart" => Color.FromArgb(0, 200, 236),    // Brighter Light Blue
                "html" => Color.FromArgb(248, 97, 58),    // Brighter Orange
                "css" => Color.FromArgb(41, 134, 202),   // Brighter Blue
                "json" => Color.FromArgb(252, 152, 3),         // Orange
                "yaml" => Color.FromArgb(91, 163, 138),        // Green-blue
                "markdown" => Color.FromArgb(0, 0, 0),         // Black
                "unknown" => Color.FromArgb(100, 100, 100),    // Gray
                "none" => Color.FromArgb(100, 100, 100),       // Gray
                _ => _isDarkMode ? Color.FromArgb(100, 100, 100) : Color.FromArgb(160, 160, 160)  // More visible Gray
            };
        }

        private Color GetTextColorForBackground(Color backgroundColor)
        {
            // Calculate luminance to determine if text should be black or white
            double luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;
            return luminance > 0.5 ? Color.Black : Color.White;
        }
    }
}

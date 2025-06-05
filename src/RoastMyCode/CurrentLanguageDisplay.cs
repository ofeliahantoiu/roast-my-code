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
        private string _previousLanguage = "None";
        private readonly int _cornerRadius = 8; // Reduced corner radius for modern look
        private bool _isDarkMode = true;
        private bool _isAnimating = false;
        private float _animationProgress = 0;
        private Timer _animationTimer;
        
        // Animation properties
        private const int AnimationDuration = 500; // milliseconds
        private const int AnimationInterval = 16; // ~60fps

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
            
            Size = new Size(200, 70); // Slightly smaller height for minimal look
            BackColor = Color.Transparent;
            
            // Initialize animation timer
            _animationTimer = new Timer
            {
                Interval = AnimationInterval
            };
            _animationTimer.Tick += AnimationTimer_Tick;
        }
        
        /// <summary>
        /// Handles animation timer ticks to update the animation progress
        /// </summary>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            _animationProgress += (float)AnimationInterval / AnimationDuration;
            
            if (_animationProgress >= 1.0f)
            {
                _animationProgress = 1.0f;
                _isAnimating = false;
                _animationTimer.Stop();
            }
            
            Invalidate();
        }
        
        /// <summary>
        /// Starts the animation effect when language changes
        /// </summary>
        public void AnimateLanguageChange()
        {
            // Store the previous language before starting animation
            _previousLanguage = _language;
            _isAnimating = true;
            _animationProgress = 0;
            _animationTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Get colors from ThemeManager
            Color backColor = _isDarkMode ? ThemeManager.DarkTheme.Surface : ThemeManager.LightTheme.Surface;
            Color textColor = _isDarkMode ? ThemeManager.DarkTheme.TextPrimary : ThemeManager.LightTheme.TextPrimary;
            Color borderColor = _isDarkMode ? ThemeManager.DarkTheme.Border : ThemeManager.LightTheme.Border;
            
            // Get accent colors for current and previous language
            Color currentAccentColor = GetLanguageColor(_language);
            Color previousAccentColor = GetLanguageColor(_previousLanguage);
            
            // Create a transition between previous and current accent colors if animating
            Color accentColor;
            if (_isAnimating && _previousLanguage != _language)
            {
                // Blend the colors based on animation progress
                accentColor = BlendColors(previousAccentColor, currentAccentColor, _animationProgress);
            }
            else
            {
                accentColor = currentAccentColor;
            }

            // Apply animation effect if active
            float scale = 1.0f;
            float opacity = 1.0f;
            if (_isAnimating)
            {
                // Ease-out animation with more pronounced scale effect
                float progress = (float)Math.Pow(_animationProgress, 0.5);
                
                // More noticeable scale animation: start smaller and grow to normal size
                scale = 0.90f + (0.10f * progress);
                
                // Opacity animation to fade in
                opacity = progress;
            }

            // Calculate scaled rectangle
            int widthDiff = (int)((1 - scale) * Width);
            int heightDiff = (int)((1 - scale) * Height);
            Rectangle rect = new Rectangle(
                widthDiff / 2,
                heightDiff / 2,
                Width - widthDiff - 1,
                Height - heightDiff - 1);

            // Draw background with subtle shadow effect
            using (GraphicsPath path = CreateRoundedRectangle(rect, _cornerRadius))
            {
                // Draw shadow (if not animating or if animation is progressing)
                if (!_isAnimating || _animationProgress > 0.2f)
                {
                    using (GraphicsPath shadowPath = CreateRoundedRectangle(
                        new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height), _cornerRadius))
                    {
                        using (PathGradientBrush shadowBrush = new PathGradientBrush(shadowPath))
                        {
                            Color shadowColor = Color.FromArgb(
                                (int)(20 * opacity),
                                _isDarkMode ? Color.Black : Color.Gray);
                            shadowBrush.CenterColor = shadowColor;
                            shadowBrush.SurroundColors = new Color[] { Color.FromArgb(0, shadowColor) };
                            g.FillPath(shadowBrush, shadowPath);
                        }
                    }
                }

                // Draw main background
                using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255 * opacity), backColor)))
                {
                    g.FillPath(brush, path);
                }

                // Add a subtle border
                using (Pen borderPen = new Pen(Color.FromArgb((int)(255 * opacity), borderColor), 1))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title text
            string titleText = "Language Detected:";
            Rectangle titleRect = new Rectangle(12, 10, Width - 24, 20);
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb((int)(255 * opacity), 
                _isDarkMode ? ThemeManager.DarkTheme.TextSecondary : ThemeManager.LightTheme.TextSecondary)))
            {
                g.DrawString(titleText, ThemeManager.Typography.Small, textBrush, titleRect);
            }

            // Language badge
            int badgeWidth = Math.Min(Width - 24, 
                (int)g.MeasureString(_language, ThemeManager.Typography.BodyBold).Width + 24);
            Rectangle badgeRect = new Rectangle(12, 32, badgeWidth, 26);
            
            // Draw language badge background with rounded corners
            using (GraphicsPath badgePath = CreateRoundedRectangle(badgeRect, 6))
            using (SolidBrush badgeBrush = new SolidBrush(Color.FromArgb((int)(255 * opacity), accentColor)))
            {
                g.FillPath(badgeBrush, badgePath);
            }
            
            // Draw language text
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb((int)(255 * opacity), 
                GetTextColorForBackground(accentColor))))
            {
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(_language, ThemeManager.Typography.BodyBold, textBrush, 
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
                "markdown" => Color.FromArgb(30, 30, 30),      // Dark Gray for markdown
                "unknown" => _isDarkMode ? ThemeManager.DarkTheme.TextSecondary : ThemeManager.LightTheme.TextSecondary,
                "none" => _isDarkMode ? ThemeManager.DarkTheme.TextSecondary : ThemeManager.LightTheme.TextSecondary,
                _ => _isDarkMode ? ThemeManager.DarkTheme.TextSecondary : ThemeManager.LightTheme.TextSecondary
            };
        }

        private Color GetTextColorForBackground(Color backgroundColor)
        {
            // Calculate the perceived brightness of the background color
            double brightness = (backgroundColor.R * 0.299 + backgroundColor.G * 0.587 + backgroundColor.B * 0.114) / 255;
            
            // Return white for dark backgrounds and black for light backgrounds
            return brightness < 0.6 ? Color.White : Color.Black;
        }

        /// <summary>
        /// Blends two colors based on the specified blend factor (0.0 to 1.0)
        /// </summary>
        /// <param name="color1">Starting color</param>
        /// <param name="color2">Ending color</param>
        /// <param name="blendFactor">Blend factor (0.0 = color1, 1.0 = color2)</param>
        /// <returns>Blended color</returns>
        private Color BlendColors(Color color1, Color color2, float blendFactor)
        {
            // Ensure blend factor is between 0 and 1
            blendFactor = Math.Max(0, Math.Min(1, blendFactor));
            
            // Use a smooth easing function for more natural color transition
            blendFactor = (float)Math.Pow(blendFactor, 0.7); // Slightly ease-in for smoother color transition
            
            // Calculate the blended color components
            int r = (int)Math.Round(color1.R + (color2.R - color1.R) * blendFactor);
            int g = (int)Math.Round(color1.G + (color2.G - color1.G) * blendFactor);
            int b = (int)Math.Round(color1.B + (color2.B - color1.B) * blendFactor);
            int a = (int)Math.Round(color1.A + (color2.A - color1.A) * blendFactor);
            
            return Color.FromArgb(a, r, g, b);
        }
    }
}

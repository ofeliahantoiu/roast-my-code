using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace RoastMyCode
{
    /// <summary>
    /// A custom RichTextBox that provides syntax highlighting for code
    /// </summary>
    public class SyntaxHighlightingTextBox : RichTextBox
    {
        private string _language = "c#";
        private bool _isDarkMode = true;
        private SyntaxHighlighter _highlighter;
        private Timer _highlightTimer;
        private bool _highlightingEnabled = true;
        private int _cornerRadius = 10;
        private Color _borderColor = Color.FromArgb(60, 60, 60);
        private int _borderWidth = 1;

        /// <summary>
        /// Gets or sets the programming language for syntax highlighting
        /// </summary>
        public string Language
        {
            get { return _language; }
            set 
            { 
                _language = value; 
                if (HighlightingEnabled)
                {
                    ApplySyntaxHighlighting();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether dark mode is enabled
        /// </summary>
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    UpdateTheme();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether syntax highlighting is enabled
        /// </summary>
        public bool HighlightingEnabled
        {
            get => _highlightingEnabled;
            set
            {
                _highlightingEnabled = value;
                if (value)
                    ApplySyntaxHighlighting();
                else
                    ResetTextColor();
            }
        }

        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                Invalidate();
            }
        }

        public SyntaxHighlightingTextBox()
        {
            SetStyle(ControlStyles.UserPaint | 
                    ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.OptimizedDoubleBuffer | 
                    ControlStyles.ResizeRedraw, true);
            
            _highlighter = new SyntaxHighlighter(_isDarkMode);
            
            // Initialize the timer for delayed highlighting (to avoid performance issues during typing)
            _highlightTimer = new Timer();
            _highlightTimer.Interval = 500; // Delay in milliseconds
            _highlightTimer.Tick += HighlightTimer_Tick;
            
            // Subscribe to text change events
            TextChanged += SyntaxHighlightingTextBox_TextChanged;
            
            // Set default appearance
            BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            ForeColor = _isDarkMode ? Color.White : Color.Black;
            BorderStyle = BorderStyle.None; // We'll draw our own border
            Font = new Font("Consolas", 10F);
            AcceptsTab = true;
            Multiline = true;
        }

        private void SyntaxHighlightingTextBox_TextChanged(object? sender, EventArgs e)
        {
            // Reset and restart the timer on each text change
            _highlightTimer.Stop();
            _highlightTimer.Start();
        }

        private void HighlightTimer_Tick(object? sender, EventArgs e)
        {
            _highlightTimer.Stop();
            ApplySyntaxHighlighting();
        }

        /// <summary>
        /// Applies syntax highlighting based on the current language setting
        /// </summary>
        public void ApplySyntaxHighlighting()
        {
            if (!_highlightingEnabled || string.IsNullOrEmpty(_language) || _language == "None")
                return;

            _highlighter.ApplyHighlighting(this, _language);
        }

        private void ResetTextColor()
        {
            // Reset all text to default color
            SelectionStart = 0;
            SelectionLength = TextLength;
            SelectionColor = _isDarkMode ? Color.White : Color.Black;
            SelectionLength = 0;
        }

        public void UpdateTheme()
        {
            BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            ForeColor = _isDarkMode ? Color.White : Color.Black;
            BorderColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(200, 200, 200);
            
            _highlighter.UpdateTheme(_isDarkMode);
            ApplySyntaxHighlighting();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Draw rounded corners and border
            using (GraphicsPath path = CreateRoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), _cornerRadius))
            {
                this.Region = new Region(path);
                
                if (_borderWidth > 0)
                {
                    using (Pen pen = new Pen(_borderColor, _borderWidth))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _highlightTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

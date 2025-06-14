using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RoastMyCode
{
    /// <summary>
    /// A control that displays character and line counts for text input
    /// </summary>
    public class TextCounterDisplay : UserControl
    {
        private int _charCount = 0;
        private int _lineCount = 1;
        private readonly Color _lightModeTextColor = Color.FromArgb(80, 80, 80);
        private readonly Color _darkModeTextColor = Color.FromArgb(200, 200, 200);
        private bool _isDarkMode = true;
        private System.Windows.Forms.Timer _updateTimer;
        
        public Color TextColor => _isDarkMode ? _darkModeTextColor : _lightModeTextColor;
        
        /// <summary>
        /// Gets or sets whether dark mode is enabled
        /// </summary>
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set 
            { 
                _isDarkMode = value;
                Invalidate();
            }
        }
        
        /// <summary>
        /// Gets or sets the character count to display
        /// </summary>
        public int CharacterCount
        {
            get => _charCount;
            set
            {
                if (_charCount != value)
                {
                    _charCount = value;
                    Invalidate();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the line count to display
        /// </summary>
        public int LineCount
        {
            get => _lineCount;
            set
            {
                if (_lineCount != value)
                {
                    _lineCount = Math.Max(1, value);
                    Invalidate();
                }
            }
        }
        
        /// <summary>
        /// Creates a new TextCounterDisplay instance
        /// </summary>
        public TextCounterDisplay()
        {
            Size = new Size(120, 24);
            DoubleBuffered = true;
            
            // Set up anti-flicker styles
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            
            // Update the display occasionally rather than on every keystroke to avoid flickering
            _updateTimer = new System.Windows.Forms.Timer();
            _updateTimer.Interval = 200; // Update every 200ms
            _updateTimer.Tick += (s, e) => 
            {
                if (Tag is RichTextBox rtb)
                {
                    // Check if this is a RoundedRichTextBox with placeholder text
                    if ((rtb is RoundedRichTextBox roundedRtb && 
                        !string.IsNullOrEmpty(roundedRtb.PlaceholderText) && 
                        rtb.Text.Length == 0 && 
                        !rtb.Focused) ||
                        rtb.Text == "Type your message here...")
                    {
                        // Don't count placeholder text
                        UpdateCounts("");
                    }
                    else
                    {
                        UpdateCounts(rtb.Text);
                    }
                }
            };
            _updateTimer.Start();
        }
        
        /// <summary>
        /// Connect this counter to a text input control
        /// </summary>
        /// <param name="textBox">The RichTextBox to count characters and lines from</param>
        public void ConnectToTextBox(RichTextBox textBox)
        {
            Tag = textBox;
            textBox.TextChanged += (s, e) => 
            {
                // The actual update happens on the timer tick to avoid excessive updates
                // This just ensures the timer is running
                if (!_updateTimer.Enabled)
                    _updateTimer.Start();
            };
            
            // Don't count initial placeholder text
            if (textBox is RoundedRichTextBox roundedTextBox && 
                !string.IsNullOrEmpty(roundedTextBox.PlaceholderText) && 
                textBox.Text.Length == 0)
            {
                UpdateCounts(""); // Use empty string instead of placeholder
            }
            else
            {
                UpdateCounts(textBox.Text);
            }
        }
        
        /// <summary>
        /// Updates the character and line counts based on the provided text
        /// </summary>
        /// <param name="text">The text to count</param>
        public void UpdateCounts(string text)
        {
            // Check for empty text or placeholder text
            if (string.IsNullOrEmpty(text) || text == "Type your message here...")
            {
                CharacterCount = 0;
                LineCount = 1;
                return;
            }
            
            CharacterCount = text.Length;
            LineCount = text.Split(new[] { '\n' }, StringSplitOptions.None).Length;
        }
        
        /// <summary>
        /// Paints the control with character and line counts
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            using (Brush brush = new SolidBrush(TextColor))
            {
                string displayText = $"Chars: {_charCount} | Lines: {_lineCount}";
                Font displayFont = new Font("Segoe UI", 8.5f);
                
                // Center the text vertically
                SizeF textSize = g.MeasureString(displayText, displayFont);
                float y = (Height - textSize.Height) / 2;
                
                g.DrawString(displayText, displayFont, brush, new PointF(0, y));
            }
        }
        
        /// <summary>
        /// Updates the theme (light/dark mode)
        /// </summary>
        public void UpdateTheme(bool isDarkMode)
        {
            IsDarkMode = isDarkMode;
        }
        
        /// <summary>
        /// Cleans up resources when the control is disposed
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateTimer?.Stop();
                _updateTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

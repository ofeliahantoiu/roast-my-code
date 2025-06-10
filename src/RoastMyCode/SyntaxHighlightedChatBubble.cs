using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RoastMyCode
{
    /// <summary>
    /// A custom chat bubble control that supports syntax highlighting for code
    /// </summary>
    public class SyntaxHighlightedChatBubble : Panel
    {
        private string _role = "user";
        private string _message = "";
        private string _language = "None";
        private bool _isDarkMode = true;
        private SyntaxHighlighter _highlighter;
        private SyntaxHighlightingTextBox _codeTextBox = null!;
        private Label _messageLabel = null!;
        private int _cornerRadius = 10;
        private bool _isCodeMessage = false;

        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                UpdateAppearance();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                UpdateContent();
            }
        }

        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    if (_isCodeMessage && _codeTextBox != null)
                    {
                        _codeTextBox.Language = value;
                        _codeTextBox.ApplySyntaxHighlighting();
                    }
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
                    UpdateAppearance();
                    if (_codeTextBox != null)
                    {
                        _codeTextBox.IsDarkMode = value;
                    }
                }
            }
        }

        public SyntaxHighlightedChatBubble()
        {
            SetStyle(ControlStyles.UserPaint | 
                    ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.OptimizedDoubleBuffer | 
                    ControlStyles.ResizeRedraw, true);
            
            _highlighter = new SyntaxHighlighter(_isDarkMode);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);
            MinimumSize = new Size(100, 0);
            MaximumSize = new Size(500, 0);
        }

        private void UpdateAppearance()
        {
            BackColor = _role == "user" 
                ? (_isDarkMode ? Color.FromArgb(70, 70, 70) : Color.FromArgb(220, 220, 220))
                : (_isDarkMode ? Color.FromArgb(50, 50, 80) : Color.FromArgb(220, 220, 240));
        }

        private void UpdateContent()
        {
            Controls.Clear();
            
            // Check if the message contains code
            _isCodeMessage = ContainsCode(_message);
            
            if (_isCodeMessage)
            {
                // Extract code from the message
                string code = ExtractCode(_message);
                
                // Create a read-only syntax highlighting text box for the code
                _codeTextBox = new SyntaxHighlightingTextBox
                {
                    Text = code,
                    ReadOnly = true,
                    Dock = DockStyle.Fill,
                    BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromArgb(245, 245, 245),
                    ForeColor = _isDarkMode ? Color.White : Color.Black,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Consolas", 10),
                    IsDarkMode = _isDarkMode,
                    Language = _language,
                    HighlightingEnabled = true,
                    CornerRadius = 5,
                    Multiline = true,
                    ScrollBars = RichTextBoxScrollBars.Vertical
                };
                
                Controls.Add(_codeTextBox);
                _codeTextBox.ApplySyntaxHighlighting();
                
                // Adjust height based on content
                _codeTextBox.Height = Math.Min(400, Math.Max(100, _codeTextBox.GetPositionFromCharIndex(_codeTextBox.TextLength).Y + 30));
            }
            else
            {
                // Regular text message
                _messageLabel = new Label
                {
                    Text = _message,
                    AutoSize = true,
                    MaximumSize = new Size(Width - Padding.Horizontal, 0),
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    ForeColor = _isDarkMode ? Color.White : Color.Black,
                    Font = new Font("Segoe UI", 10)
                };
                
                Controls.Add(_messageLabel);
            }
            
            PerformLayout();
        }

        private bool ContainsCode(string message)
        {
            // Check for code block markers
            if (message.Contains("```"))
                return true;
                
            // Check for common code patterns
            if ((message.Contains("{") && message.Contains("}")) || 
                (message.Contains("def ") && message.Contains(":")) ||
                (message.Contains("class ") && message.Contains("{")) ||
                (message.Contains("function") && message.Contains("{")))
                return true;
                
            return false;
        }

        private string ExtractCode(string message)
        {
            // Try to extract code between markdown code blocks
            int startIndex = message.IndexOf("```");
            if (startIndex >= 0)
            {
                // Find the language identifier line end
                int lineEnd = message.IndexOf('\n', startIndex);
                if (lineEnd > startIndex)
                {
                    // Find the closing code block
                    int endIndex = message.IndexOf("```", lineEnd);
                    if (endIndex > lineEnd)
                    {
                        return message.Substring(lineEnd + 1, endIndex - lineEnd - 1).Trim();
                    }
                }
            }
            
            // If no markdown blocks, return the whole message
            return message;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Draw rounded corners
            using (GraphicsPath path = CreateRoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), _cornerRadius))
            {
                this.Region = new Region(path);
                
                using (Pen pen = new Pen(_isDarkMode ? Color.FromArgb(80, 80, 80) : Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
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
    }
}

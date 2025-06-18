using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RoastMyCode.Extensions;
using RoastMyCode.Services;
using RoastMyCode.Controls;

namespace RoastMyCode
{
    public partial class ChatMessageBubble : UserControl
    {
        private string _messageText = string.Empty;
        private string _role = "assistant";
        private string _language = "text";
        private Color _bubbleColor;
        private Color _textColor;
        private Button? _menuButton;
        private ContextMenuStrip? _contextMenu;
        private bool _showCopyFeedback = false;
        private DateTime _feedbackStartTime = DateTime.MinValue;
        private const int FeedbackDurationMs = 1000; // Show feedback for 1 second
        
        // Syntax highlighting components
        private SyntaxHighlightedCodeView? _codeView;
        private Label? _languageBadge;
        private bool _isCodeBlock = false;

        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                
                // Detect if this is a code block and what language it is
                _language = LanguageDetector.DetectLanguage(value);
                _isCodeBlock = !string.IsNullOrEmpty(_language) && _language != "text";
                
                // Update the code view if needed
                UpdateCodeView();
                
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

        // Default constructor for designer and object initialization
        public ChatMessageBubble()
        {
            InitializeComponent();
            
            this.MinimumSize = new Size(100, 40);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MaximumSize = new Size(600, 0);
            
            // Set up padding and margin
            this.Padding = new Padding(15, 10, 15, 10);
            this.Margin = new Padding(12, 8, 12, 8);
            
            // Default colors
            _bubbleColor = Color.FromArgb(52, 58, 64); // Dark gray for assistant
            _textColor = Color.FromArgb(220, 220, 220); // Light gray text
            
            Font = new Font("Segoe UI", 12);
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            
            // Create the menu button outside the bubble
            CreateMenuButton();
            
            // Set up timer to check for feedback expiration
            System.Windows.Forms.Timer feedbackTimer = new System.Windows.Forms.Timer();
            feedbackTimer.Interval = 100; // Check every 100ms
            feedbackTimer.Tick += (sender, e) => {
                if (_showCopyFeedback && (DateTime.Now - _feedbackStartTime).TotalMilliseconds > FeedbackDurationMs)
                {
                    _showCopyFeedback = false;
                    this.Invalidate();
                }
            };
            feedbackTimer.Start();
        }
        
        // Constructor with parameters for programmatic creation
        public ChatMessageBubble(string message, string role, string language = "")
            : this() // Call the default constructor first
        {
            _messageText = message;
            _role = role.ToLower();
            _language = language;
            
            // Set colors based on role
            if (_role == "user")
            {
                _bubbleColor = Color.FromArgb(70, 130, 180); // Steel blue for user
                _textColor = Color.White;
            }
            else if (_role == "system")
            {
                _bubbleColor = Color.FromArgb(220, 53, 69); // Red for system/error messages
                _textColor = Color.White;
            }
            else
            {
                _bubbleColor = Color.FromArgb(52, 58, 64); // Dark gray for assistant
                _textColor = Color.FromArgb(220, 220, 220); // Light gray text
            }
        }
        
        private void CreateMenuButton()
        {
            // Initialize menu button
            _menuButton = new Button();
            _menuButton.FlatStyle = FlatStyle.Flat;
            _menuButton.FlatAppearance.BorderSize = 0;
            _menuButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 100, 100);
            _menuButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(120, 120, 120);
            _menuButton.Size = new Size(45, 22);
            _menuButton.Text = "Copy"; // Show Copy text directly
            _menuButton.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            _menuButton.Cursor = Cursors.Hand;
            _menuButton.BackColor = Color.FromArgb(60, 60, 60);
            _menuButton.ForeColor = Color.White;
            // Add rounded corners effect
            _menuButton.Region = new Region(CreateRoundedRectangle(_menuButton.Width, _menuButton.Height, 4));
            _menuButton.Click += MenuButton_Click;
            _menuButton.Visible = false; // Initially hidden until positioned
            _menuButton.MouseEnter += (s, e) => { if (_menuButton != null) _menuButton.ForeColor = Color.FromArgb(220, 220, 220); };
            _menuButton.MouseLeave += (s, e) => { if (_menuButton != null) _menuButton.ForeColor = Color.White; };
            
            // Create context menu
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Copy", null, CopyMenuItem_Click);
            _contextMenu.BackColor = Color.FromArgb(60, 60, 60);
            _contextMenu.ForeColor = Color.White;
            
            // Add the button to the parent control when parent is set
            this.ParentChanged += (sender, e) => {
                if (this.Parent != null && _menuButton != null && !this.Parent.Controls.Contains(_menuButton))
                {
                    this.Parent.Controls.Add(_menuButton);
                    _menuButton.BringToFront();
                    UpdateMenuButtonPosition();
                    
                    // Add scroll event handler to hide menu button when scrolling
                    if (this.Parent is Panel panel)
                    {
                        panel.Scroll += (s, args) => {
                            if (_menuButton != null && _menuButton.Visible)
                            {
                                _menuButton.Visible = false;
                            }
                        };
                    }
                }
            };
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

                int minHeight = Math.Max(textSize.Height + Padding.Vertical * 2, 30); 
                int totalHeight = minHeight; 
                
                // Add extra space for code blocks
                if (_isCodeBlock && _role == "user")
                {
                    // Code blocks need more space
                    int codeLines = _messageText.Split('\n').Length;
                    int codeHeight = Math.Max(codeLines * 20, 100); // Estimate height based on line count
                    totalHeight = Math.Max(totalHeight, codeHeight + 40); // Add padding
                }

                this.Width = Math.Min(textSize.Width + Padding.Horizontal + 8, MaximumSize.Width);
                this.Height = Math.Max(totalHeight, 50); 

                this.Left = (_role == "user")
                    ? Parent.ClientSize.Width - this.Width - Margin.Right
                    : Margin.Left;

                // Update code view and language badge positions
                if (_isCodeBlock && _role == "user")
                {
                    UpdateCodeView();
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

            // Use the role-specific colors
            Color backColor = _bubbleColor;
            Color textColor = _textColor;
            int cornerRadius = 12;
            
            // If copy feedback is active, use a different background color
            if (_showCopyFeedback)
            {
                backColor = Color.FromArgb(60, 100, 60); // Slightly green to indicate success
            }

            // Skip drawing text if this is a code block for a user message
            // The syntax highlighted code view will handle that
            if (_isCodeBlock && _role == "user" && _codeView != null)
            {
                // Just draw the bubble background
                using (GraphicsPath path = new GraphicsPath())
                {
                    int diameter = cornerRadius * 2;
                    path.AddArc(new Rectangle(0, 0, diameter, diameter), 180, 90);
                    path.AddArc(new Rectangle(this.Width - diameter, 0, diameter, diameter), 270, 90);
                    path.AddArc(new Rectangle(this.Width - diameter, this.Height - diameter, diameter, diameter), 0, 90);
                    path.AddArc(new Rectangle(0, this.Height - diameter, diameter, diameter), 90, 90);
                    path.CloseFigure();

                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
                return;
            }

            int bubbleHeight = this.Height;
            if (bubbleHeight < 30) 
            {
                bubbleHeight = 30;
                this.Height = bubbleHeight;
                Invalidate(); 
                return;
            }

            Rectangle bubbleRect = new Rectangle(
                0,
                0,
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
        }

        // No need for special mouse handling for the menu button
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }
        
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }
        
        /// <summary>
        /// Creates a rounded rectangle shape for the button
        /// </summary>
        private GraphicsPath CreateRoundedRectangle(int width, int height, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(width - radius * 2, height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            return path;
        }
        
        private void UpdateMenuButtonPosition()
        {
            if (_menuButton == null || this.Parent == null) return;
            
            // Position the menu button outside the message bubble but in the parent's coordinate system
            Point bubbleLocation = this.Location;
            
            if (_role == "user")
            {
                // For user messages (right-aligned), position at the left side outside the bubble
                _menuButton.Left = bubbleLocation.X - _menuButton.Width - 5;
                _menuButton.Top = bubbleLocation.Y + 5;
            }
            else
            {
                // For assistant/system messages (left-aligned), position at the right side outside the bubble
                _menuButton.Left = bubbleLocation.X + this.Width + 5;
                _menuButton.Top = bubbleLocation.Y + 5;
            }
            
            // Make sure it's visible
            _menuButton.Visible = !string.IsNullOrEmpty(_messageText);
            if (_menuButton.Parent != null)
            {
                _menuButton.BringToFront();
            }
        }
        
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateMenuButtonPosition();
        }
        
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            UpdateMenuButtonPosition();
        }
        
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            
            // Update menu button position when parent changes
            if (Parent != null && _menuButton != null)
            {
                if (!Parent.Controls.Contains(_menuButton))
                {
                    Parent.Controls.Add(_menuButton);
                }
                UpdateMenuButtonPosition();
            }
            
            // Update language badge position when parent changes
            if (Parent != null && _languageBadge != null)
            {
                if (!Parent.Controls.Contains(_languageBadge))
                {
                    Parent.Controls.Add(_languageBadge);
                }
                UpdateLanguageBadgePosition();
            }
            
            // Update code view if needed
            if (_isCodeBlock && _role == "user")
            {
                UpdateCodeView();
            }
        }
        
        private void MenuButton_Click(object? sender, EventArgs e)
        {
            // Since we now have a direct Copy button, copy the text immediately
            if (!string.IsNullOrEmpty(_messageText))
            {
                try
                {
                    Clipboard.SetText(_messageText);
                    ShowCopyFeedback();
                }
                catch (Exception ex)
                {
                    // Handle clipboard access errors silently
                    System.Diagnostics.Debug.WriteLine($"Clipboard error: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Updates the code view with syntax highlighting
        /// </summary>
        private void UpdateCodeView()
        {
            // Only apply code view for user messages that are code blocks
            if (_isCodeBlock && _role == "user")
            {
                // Create the code view if it doesn't exist
                if (_codeView == null)
                {
                    _codeView = new SyntaxHighlightedCodeView();
                    this.Controls.Add(_codeView);
                }
                
                // Calculate proper margins for the code view to be inside the bubble
                // with proper padding (similar to ChatGPT style)
                int codeMargin = 10;
                
                // Position and size the code view within the bubble
                Rectangle bubbleContentRect = new Rectangle(
                    codeMargin,
                    codeMargin + 5, // Add a bit more space at the top for the language badge
                    this.Width - (codeMargin * 2),
                    this.Height - (codeMargin * 2) - 5
                );
                
                // Set the code view position and size
                _codeView.Location = bubbleContentRect.Location;
                _codeView.Size = bubbleContentRect.Size;
                
                // Remove border since we're adding rounded corners in the control itself
                _codeView.BorderStyle = BorderStyle.None;
                
                // Update the code and language
                _codeView.Code = _messageText;
                _codeView.Language = _language;
                _codeView.Visible = true;
                _codeView.BringToFront(); // Make sure it's visible above other controls
                
                // Create the language badge if it doesn't exist
                if (_languageBadge == null)
                {
                    CreateLanguageBadge();
                    
                    // Make sure the badge is added to the parent control
                    if (this.Parent != null && _languageBadge != null)
                    {
                        this.Parent.Controls.Add(_languageBadge);
                        _languageBadge.BringToFront();
                        UpdateLanguageBadgePosition();
                    }
                }
                else
                {
                    // Update existing badge
                    string displayLanguage = FormatLanguageForDisplay(_language);
                    if (_languageBadge != null)
                    {
                        _languageBadge.Text = displayLanguage;
                        _languageBadge.Tag = _language;
                        _languageBadge.Visible = true;
                        UpdateLanguageBadgePosition();
                    }
                }
            }
            else
            {
                // Hide code view and language badge for non-code blocks
                if (_codeView != null)
                    _codeView.Visible = false;
                    
                if (_languageBadge != null)
                    _languageBadge.Visible = false;
            }
        }
        
        /// <summary>
        /// Creates a language badge to display above the code block
        /// </summary>
        private void CreateLanguageBadge()
        {
            if (string.IsNullOrEmpty(_language) || _language == "text") return;
            
            // Format the language name to be more consistent and user-friendly
            string displayLanguage = FormatLanguageForDisplay(_language);
            
            // Create the language badge with styling closer to ChatGPT
            _languageBadge = new Label
            {
                Text = displayLanguage,
                AutoSize = true,
                BackColor = Color.FromArgb(50, 50, 50), // Darker background for better contrast
                ForeColor = Color.FromArgb(240, 240, 240), // Brighter text for better visibility
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular), // Larger font
                Padding = new Padding(12, 5, 12, 5), // More padding for better visibility
                Visible = true,
                BorderStyle = BorderStyle.None,
                Tag = _language // Store the original language for reference
            };
            
            // Style the badge to look more like ChatGPT's language badge
            _languageBadge.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 3; // Smaller radius for more subtle rounding
                    var rect = new Rectangle(0, 0, _languageBadge.Width - 1, _languageBadge.Height - 1);
                    path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                    path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseAllFigures();
                    _languageBadge.Region = new Region(path);
                }
            };
        }
        
        /// <summary>
        /// Updates the position of the language badge
        /// </summary>
        private void UpdateLanguageBadgePosition()
        {
            if (_languageBadge == null || this.Parent == null) return;
            
            // Position the badge in the top-right corner of the bubble
            _languageBadge.Left = this.Right - _languageBadge.Width - 5; // Close to the edge
            _languageBadge.Top = this.Top - _languageBadge.Height / 2; // Position slightly above the bubble
            
            // Only show the badge for code messages when we have a valid language
            _languageBadge.Visible = _isCodeBlock && 
                                     !string.IsNullOrEmpty(_language) && 
                                     _language != "text";
            
            // Make sure the badge is above other controls
            _languageBadge.BringToFront();
        }
        
        /// <summary>
        /// Formats a language identifier for display in the badge
        /// </summary>
        private string FormatLanguageForDisplay(string language)
        {
            if (string.IsNullOrEmpty(language))
                return string.Empty;
                
            // Format the language name to be more consistent and user-friendly
            switch (language.ToLowerInvariant())
            {
                case "csharp":
                    return "C#";
                    
                case "javascript":
                    return "JavaScript";
                    
                case "typescript":
                    return "TypeScript";
                    
                case "python":
                    return "Python";
                    
                case "java":
                    return "Java";
                    
                case "php":
                case "php3":
                case "php4":
                case "php5":
                case "php7":
                case "php8":
                case "phtml":
                case "pht":
                    return "PHP";
                    
                case "html":
                    return "HTML";
                    
                case "css":
                    return "CSS";
                    
                case "sql":
                    return "SQL";
                    
                case "xml":
                    return "XML";
                    
                case "json":
                    return "JSON";
                    
                case "markdown":
                case "md":
                    return "Markdown";
                    
                case "ruby":
                    return "Ruby";
                    
                case "go":
                    return "Go";
                    
                case "rust":
                    return "Rust";
                    
                case "swift":
                    return "Swift";
                    
                case "kotlin":
                    return "Kotlin";
                    
                default:
                    // Capitalize first letter for other languages
                    if (language.Length > 0)
                    {
                        return char.ToUpper(language[0]) + language.Substring(1).ToLowerInvariant();
                    }
                    return language;
            }
        }
        
        private void CopyMenuItem_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_messageText))
            {
                try
                {
                    Clipboard.SetText(_messageText);
                    ShowCopyFeedback();
                }
                catch (Exception ex)
                {
                    // Handle clipboard access errors silently
                    System.Diagnostics.Debug.WriteLine($"Clipboard error: {ex.Message}");
                }
            }
        }
        
        private void ShowCopyFeedback()
        {
            // Store original color
            Color originalColor = _bubbleColor;
            
            // Change to feedback color (light green)
            _bubbleColor = Color.FromArgb(75, 180, 75);
            this.Invalidate(); // Redraw with new color
            
            // Also update the button text to show feedback
            if (_menuButton != null)
            {
                _menuButton.Text = "Copied!";
                _menuButton.BackColor = Color.FromArgb(60, 150, 60);
            }
            
            // Create a timer to revert back after a short delay
            System.Windows.Forms.Timer feedbackTimer = new System.Windows.Forms.Timer();
            feedbackTimer.Interval = 1000; // 1 second
            feedbackTimer.Tick += (s, e) =>
            {
                // Revert to original color
                _bubbleColor = originalColor;
                this.Invalidate();
                
                // Revert button text and color
                if (_menuButton != null)
                {
                    _menuButton.Text = "Copy";
                    _menuButton.BackColor = Color.FromArgb(60, 60, 60);
                }
                
                // Clean up timer
                feedbackTimer.Stop();
                feedbackTimer.Dispose();
            };
            feedbackTimer.Start();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up resources
                if (_menuButton != null)
                {
                    _menuButton.Click -= MenuButton_Click;
                    if (_menuButton.Parent != null)
                    {
                        _menuButton.Parent.Controls.Remove(_menuButton);
                    }
                    _menuButton.Dispose();
                }
                
                if (_contextMenu != null)
                {
                    foreach (ToolStripItem item in _contextMenu.Items)
                    {
                        // Remove event handlers for Copy menu item
                        if (item.Name == "Copy" || item.Text == "Copy")
                        {
                            item.Click -= CopyMenuItem_Click;
                        }
                    }
                    _contextMenu.Dispose();
                }
                
                // Clean up syntax highlighting resources
                if (_codeView != null)
                {
                    _codeView.Dispose();
                    _codeView = null;
                }
                
                if (_languageBadge != null)
                {
                    if (_languageBadge.Parent != null)
                    {
                        _languageBadge.Parent.Controls.Remove(_languageBadge);
                    }
                    _languageBadge.Dispose();
                    _languageBadge = null;
                }
            }
            
            base.Dispose(disposing);
        }
    }
}

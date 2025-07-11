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
        private Image? _image = null;
        private Color _bubbleColor;
        private Color _textColor;
        private Button? _menuButton;
        private ContextMenuStrip? _contextMenu;
        private bool _showCopyFeedback = false;
        private DateTime _feedbackStartTime = DateTime.MinValue;
        private const int FeedbackDurationMs = 1000; // Show feedback for 1 second
        
        // Syntax highlighting components
        private SyntaxHighlightedCodeView? _codeView;
        private bool _isCodeBlock = false;

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
                    // Detect if this is a code block and what language it is
                    _language = LanguageDetector.DetectLanguage(value);
                    _isCodeBlock = !string.IsNullOrEmpty(_language) && _language != "text";
                    
                    // Update the code view if needed
                    UpdateCodeView();
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

        // Default constructor for designer and object initialization
        public ChatMessageBubble()
        {
            InitializeComponent();
            
            this.MinimumSize = new Size(100, 80);
            this.MaximumSize = new Size(600, 0);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Padding = new Padding(20);
            this.Margin = new Padding(12);
            this.MaximumSize = new Size(600, 0);
            
            // Set up moderate padding and margin to ensure text and code stay within bubble boundaries
            this.Padding = new Padding(20, 25, 20, 20);
            this.Margin = new Padding(15, 20, 15, 15);
            
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

            // Check if we have content to display (either message text or image)
            if (Parent == null || (string.IsNullOrEmpty(_messageText) && _image == null)) return;

            using (Graphics g = CreateGraphics())
            {
                TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
                int availableWidth = MaximumSize.Width - Padding.Horizontal;
                Size proposedSize = new Size(availableWidth, int.MaxValue);
                
                // Calculate text size
                Size textSize = Size.Empty;
                if (!string.IsNullOrEmpty(_messageText))
                {
                    textSize = TextRenderer.MeasureText(_messageText, Font, proposedSize, flags);
                }

                // Calculate image size
                Size imageSize = Size.Empty;
                if (_image != null)
                {
                    try
                    {
                        int maxImageWidth = availableWidth;
                        int maxImageHeight = 300; // Maximum height for images
                        
                        double scaleX = (double)maxImageWidth / _image.Width;
                        double scaleY = (double)maxImageHeight / _image.Height;
                        double scale = Math.Min(scaleX, scaleY);
                        
                        imageSize = new Size(
                            (int)(_image.Width * scale),
                            (int)(_image.Height * scale)
                        );
                    }
                    catch (Exception)
                    {
                        // Image is invalid, treat as null
                        _image = null;
                        imageSize = Size.Empty;
                    }
                }

                // Language label space
                int languageLabelHeight = 0;
                if (_role == "user" || _language == "PHOTO")
                {
                    Size languageSize = TextRenderer.MeasureText(_language.ToUpper(), Font);
                    languageLabelHeight = languageSize.Height + 10;
                }

                // Calculate total height and width
                int contentHeight = textSize.Height + imageSize.Height;
                int contentWidth = Math.Max(textSize.Width, imageSize.Width);
                
                if (textSize.Height > 0 && imageSize.Height > 0)
                {
                    contentHeight += 10; // Spacing between text and image
                }
                
                int minHeight = Math.Max(contentHeight + Padding.Vertical, 80);
                int totalHeight = minHeight + languageLabelHeight;

                // Add extra space for code blocks
                if (_isCodeBlock && _role == "user")
                {
                    // Calculate appropriate space for code blocks with more moderate padding
                    int codeLines = _messageText.Split('\n').Length;
                    int codeHeight = Math.Max(codeLines * 20, 150); // More reasonable height estimate based on line count
                    totalHeight = Math.Max(totalHeight, codeHeight + 80); // Moderate additional padding
                    
                    // Calculate width based on content while keeping reasonable margins
                    int maxLineLength = _messageText.Split('\n').Max(line => line.Length);
                    int estimatedCodeWidth = Math.Max(maxLineLength * 10, contentWidth + 80); // More efficient width estimation
                    contentWidth = Math.Max(contentWidth, estimatedCodeWidth);
                    
                    // Enforce sensible minimum dimensions for code blocks
                    contentWidth = Math.Max(contentWidth, 350);
                    totalHeight = Math.Max(totalHeight, 180);
                }
                this.Width = Math.Min(contentWidth + Padding.Horizontal, MaximumSize.Width);
                this.Height = Math.Max(totalHeight, MinimumSize.Height);

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

            // Skip drawing text if this is a code block for a user message
            // The syntax highlighted code view will handle that
            if (_isCodeBlock && _role == "user" && _codeView != null)
            {
                // Just draw the bubble background
                using (GraphicsPath path = new GraphicsPath())
                {
                    int diameter = cornerRadius * 2;
                    path.AddArc(new Rectangle(0, labelOffsetY, diameter, diameter), 180, 90);
                    path.AddArc(new Rectangle(this.Width - diameter, labelOffsetY, diameter, diameter), 270, 90);
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

            // Check if this is a photo message (special handling for images)
            if (_language == "PHOTO" && _image != null)
            {
                try
                {
                    int maxImageWidth = this.Width - Padding.Horizontal;
                    int maxImageHeight = 300;
                    
                    double scaleX = (double)maxImageWidth / _image.Width;
                    double scaleY = (double)maxImageHeight / _image.Height;
                    double scale = Math.Min(scaleX, scaleY);
                    
                    int imageWidth = (int)(_image.Width * scale);
                    int imageHeight = (int)(_image.Height * scale);
                    
                    // Center the image horizontally
                    int imageX = Padding.Left + (maxImageWidth - imageWidth) / 2;
                    
                    Rectangle imageRect = new Rectangle(imageX, currentY, imageWidth, imageHeight);
                    
                    // Draw image with rounded corners
                    using (GraphicsPath imagePath = new GraphicsPath())
                    {
                        int imageCornerRadius = 8;
                        int imageDiameter = imageCornerRadius * 2;
                        
                        imagePath.AddArc(new Rectangle(imageRect.Left, imageRect.Top, imageDiameter, imageDiameter), 180, 90);
                        imagePath.AddArc(new Rectangle(imageRect.Right - imageDiameter, imageRect.Top, imageDiameter, imageDiameter), 270, 90);
                        imagePath.AddArc(new Rectangle(imageRect.Right - imageDiameter, imageRect.Bottom - imageDiameter, imageDiameter, imageDiameter), 0, 90);
                        imagePath.AddArc(new Rectangle(imageRect.Left, imageRect.Bottom - imageDiameter, imageDiameter, imageDiameter), 90, 90);
                        imagePath.CloseFigure();
                        
                        g.SetClip(imagePath);
                        g.DrawImage(_image, imageRect);
                        g.ResetClip();
                    }
                }
                catch (Exception)
                {
                    // Image is invalid, treat as null
                    _image = null;
                }
            }
            // Skip regular text drawing if this is a code block for a user message
            // The syntax highlighted code view will handle that
            else if (_isCodeBlock && _role == "user" && _codeView != null)
            {
                // Text is drawn by the code view component
                return;
            }
            // Draw text normally if it's not a code block or photo
            else if (!string.IsNullOrEmpty(_messageText))
            {
                Rectangle textRect = new Rectangle(
                    Padding.Left,
                    currentY,
                    this.Width - Padding.Horizontal,
                    bubbleRect.Height - Padding.Vertical
                );

                TextRenderer.DrawText(
                    g,
                    _messageText,
                    Font,
                    textRect,
                    textColor,
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl
                );

                // If we need to display an image after text
                if (_image != null && _language != "PHOTO")
                {
                    // Measure text height to position image below it
                    TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
                    Size proposedSize = new Size(this.Width - Padding.Horizontal, int.MaxValue);
                    Size textSize = TextRenderer.MeasureText(_messageText, Font, proposedSize, flags);
                    currentY += textSize.Height + 10; // Add spacing after text
                    
                    try
                    {
                        int maxImageWidth = this.Width - Padding.Horizontal;
                        int maxImageHeight = 300;
                        
                        double scaleX = (double)maxImageWidth / _image.Width;
                        double scaleY = (double)maxImageHeight / _image.Height;
                        double scale = Math.Min(scaleX, scaleY);
                        
                        int imageWidth = (int)(_image.Width * scale);
                        int imageHeight = (int)(_image.Height * scale);
                        
                        // Center the image horizontally
                        int imageX = Padding.Left + (maxImageWidth - imageWidth) / 2;
                        
                        Rectangle imageRect = new Rectangle(imageX, currentY, imageWidth, imageHeight);
                        
                        // Draw image with rounded corners
                        using (GraphicsPath imagePath = new GraphicsPath())
                        {
                            int imageCornerRadius = 8;
                            int imageDiameter = imageCornerRadius * 2;
                            
                            imagePath.AddArc(new Rectangle(imageRect.Left, imageRect.Top, imageDiameter, imageDiameter), 180, 90);
                            imagePath.AddArc(new Rectangle(imageRect.Right - imageDiameter, imageRect.Top, imageDiameter, imageDiameter), 270, 90);
                            imagePath.AddArc(new Rectangle(imageRect.Right - imageDiameter, imageRect.Bottom - imageDiameter, imageDiameter, imageDiameter), 0, 90);
                            imagePath.AddArc(new Rectangle(imageRect.Left, imageRect.Bottom - imageDiameter, imageDiameter, imageDiameter), 90, 90);
                            imagePath.CloseFigure();
                            
                            g.SetClip(imagePath);
                            g.DrawImage(_image, imageRect);
                            g.ResetClip();
                        }
                    }
                    catch (Exception)
                    {
                        // Image is invalid, clear it
                        _image = null;
                    }
                }
            }
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
            
            // Language badge references removed
            
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
                // Use more reasonable insets to optimize code display area while keeping it inside the bubble
                int codeMarginX = 25; // Moderate margin from left/right edges
                int codeMarginTop = 45; // Moderate margin from top for language badge
                int codeMarginBottom = 25; // Moderate margin from bottom
                
                // Position and size the code view within the bubble with very generous safety margin
                Rectangle bubbleContentRect = new Rectangle(
                    codeMarginX,
                    codeMarginTop,
                    this.Width - (codeMarginX * 2),
                    this.Height - codeMarginTop - codeMarginBottom
                );
                
                // Ensure the bubble is always big enough to contain the code view with proper margins
                if (bubbleContentRect.Width < 300) bubbleContentRect.Width = 300;
                if (bubbleContentRect.Height < 100) bubbleContentRect.Height = 100;
                
                // Apply size adjustments if the content area is too small
                if (bubbleContentRect.Width < 0) bubbleContentRect.Width = 0;
                if (bubbleContentRect.Height < 0) bubbleContentRect.Height = 0;
                
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
                
                // Language badge has been removed in favor of the top panel language display
            }
            else
            {
                // Hide code view for non-code blocks
                if (_codeView != null)
                    _codeView.Visible = false;
            }
        }
        
        // Language badge methods removed
                    
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
                
                // Language badge cleanup code removed
            }
            
            if (disposing)
            {
                // Clean up resources
                _image?.Dispose();
                
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
            }
            base.Dispose(disposing);
        }
    }
}

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
        private Color _bubbleColor;
        private Color _textColor;
        private Button? _menuButton;
        private ContextMenuStrip? _contextMenu;
        private bool _showCopyFeedback = false;
        private DateTime _feedbackStartTime = DateTime.MinValue;
        private const int FeedbackDurationMs = 1000; // Show feedback for 1 second

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
            _menuButton.Size = new Size(20, 20);
            _menuButton.Text = "⋮"; // Vertical ellipsis
            _menuButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _menuButton.Cursor = Cursors.Hand;
            _menuButton.BackColor = Color.FromArgb(80, 80, 80);
            _menuButton.ForeColor = Color.White;
            _menuButton.Click += MenuButton_Click;
            _menuButton.Visible = false; // Initially hidden until positioned
            
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

                // Language label space
                int languageLabelHeight = 0;
                if (_role == "user")
                {
                    Size languageSize = TextRenderer.MeasureText(_language.ToUpper(), Font);
                    languageLabelHeight = languageSize.Height + 12; 
                }

                int minHeight = Math.Max(textSize.Height + Padding.Vertical * 2, 30); 
                int totalHeight = minHeight + languageLabelHeight + 10; 

                this.Width = Math.Min(textSize.Width + Padding.Horizontal + 8, MaximumSize.Width);
                this.Height = Math.Max(totalHeight, 50); 

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
        
        private void UpdateMenuButtonPosition()
        {
            if (_menuButton == null || this.Parent == null) return;
            
            // Position the menu button outside the message bubble but in the parent's coordinate system
            Point bubbleLocation = this.Location;
            
            if (_role == "user")
            {
                // For user messages (right-aligned), position at the left side outside the bubble
                _menuButton.Left = bubbleLocation.X - _menuButton.Width - 2;
                _menuButton.Top = bubbleLocation.Y + 5;
            }
            else
            {
                // For assistant/system messages (left-aligned), position at the right side outside the bubble
                _menuButton.Left = bubbleLocation.X + this.Width + 2;
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
            UpdateMenuButtonPosition();
        }
        
        private void MenuButton_Click(object? sender, EventArgs e)
        {
            if (_contextMenu != null && sender is Button button)
            {
                // Show context menu below the button
                _contextMenu.Show(button, new Point(0, button.Height));
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
            _showCopyFeedback = true;
            _feedbackStartTime = DateTime.Now;
            this.Invalidate();
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
            }
            
            base.Dispose(disposing);
        }
    }
}

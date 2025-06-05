using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace RoastMyCode
{
    /// <summary>
    /// Chat bubble control with animation capabilities for smooth transitions
    /// </summary>
    public class AnimatedChatBubble : Control
    {
        // Animation properties
        private System.Windows.Forms.Timer _animationTimer;
        private float _animationProgress = 0.0f;
        private int _animationDuration = 500; // milliseconds
        private DateTime _animationStartTime;
        private bool _isAnimating = false;
        private string _animationType = "FadeIn"; // FadeIn, SlideIn, etc.
        private Point _originalLocation;
        private int _slideDistance = 50;
        
        // Appearance properties
        private string _messageText = string.Empty;
        private string _role = "user"; // user, assistant, system
        private Color _bubbleColor = Color.FromArgb(240, 240, 240);
        private Color _textColor = Color.Black;
        private int _cornerRadius = 15;
        private bool _isDarkMode = false;
        private float _opacity = 1.0f;
        
        /// <summary>
        /// Gets or sets the opacity of the control (0.0 to 1.0)
        /// </summary>
        public float Opacity
        {
            get { return _opacity; }
            set 
            { 
                _opacity = Math.Clamp(value, 0.0f, 1.0f);
                Invalidate();
            }
        }
        
        /// <summary>
        /// Gets or sets the message text to display
        /// </summary>
        [Description("The message text to display in the bubble"), Category("Appearance")]
        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                AdjustHeight();
                Invalidate();
            }
        }
        
        /// <summary>
        /// Gets or sets the role of the message sender (user, assistant, system)
        /// </summary>
        [Description("The role of the message sender"), Category("Appearance")]
        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                UpdateColors();
                Invalidate();
            }
        }
        
        /// <summary>
        /// Gets or sets the animation type (FadeIn, SlideIn, etc.)
        /// </summary>
        [Description("The type of animation to use"), Category("Animation")]
        public string AnimationType
        {
            get => _animationType;
            set => _animationType = value;
        }
        
        /// <summary>
        /// Gets or sets the animation duration in milliseconds
        /// </summary>
        [Description("The duration of the animation in milliseconds"), Category("Animation")]
        public int AnimationDuration
        {
            get => _animationDuration;
            set => _animationDuration = value;
        }
        
        /// <summary>
        /// Gets or sets whether dark mode is enabled
        /// </summary>
        [Description("Whether dark mode is enabled"), Category("Appearance")]
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                UpdateColors();
                Invalidate();
            }
        }
        
        /// <summary>
        /// Creates a new animated chat bubble
        /// </summary>
        public AnimatedChatBubble()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.OptimizedDoubleBuffer | 
                    ControlStyles.ResizeRedraw | 
                    ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.UserPaint, true);
            
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(50, 30);
            Padding = new Padding(10);
            
            // Initialize animation timer
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 16; // ~60fps
            _animationTimer.Tick += AnimationTimer_Tick;
            
            UpdateColors();
        }
        
        /// <summary>
        /// Updates colors based on role and dark mode setting
        /// </summary>
        private void UpdateColors()
        {
            if (_isDarkMode)
            {
                switch (_role)
                {
                    case "user":
                        _bubbleColor = Color.FromArgb(70, 70, 70);
                        _textColor = Color.White;
                        break;
                    case "assistant":
                        _bubbleColor = Color.FromArgb(0, 120, 212);
                        _textColor = Color.White;
                        break;
                    case "system":
                        _bubbleColor = Color.FromArgb(50, 50, 50);
                        _textColor = Color.FromArgb(255, 200, 0);
                        break;
                }
            }
            else
            {
                switch (_role)
                {
                    case "user":
                        _bubbleColor = Color.FromArgb(240, 240, 240);
                        _textColor = Color.Black;
                        break;
                    case "assistant":
                        _bubbleColor = Color.FromArgb(0, 120, 212);
                        _textColor = Color.White;
                        break;
                    case "system":
                        _bubbleColor = Color.FromArgb(255, 240, 200);
                        _textColor = Color.FromArgb(180, 0, 0);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Starts the animation
        /// </summary>
        public void StartAnimation()
        {
            _animationProgress = 0.0f;
            _isAnimating = true;
            _animationStartTime = DateTime.Now;
            
            // Store original location for slide animations
            _originalLocation = Location;
            
            // Apply initial state based on animation type
            switch (_animationType)
            {
                case "FadeIn":
                    Opacity = 0;
                    break;
                case "SlideIn":
                    if (_role == "user")
                        Location = new Point(_originalLocation.X - _slideDistance, _originalLocation.Y);
                    else
                        Location = new Point(_originalLocation.X + _slideDistance, _originalLocation.Y);
                    Opacity = 0;
                    break;
                case "GrowIn":
                    Size = new Size(Width / 2, Height / 2);
                    Opacity = 0;
                    break;
            }
            
            _animationTimer.Start();
        }
        
        /// <summary>
        /// Updates the animation state on each timer tick
        /// </summary>
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isAnimating) return;
            
            // Calculate progress (0.0 to 1.0)
            double elapsed = (DateTime.Now - _animationStartTime).TotalMilliseconds;
            _animationProgress = (float)Math.Min(elapsed / _animationDuration, 1.0);
            
            // Apply animation based on type and progress
            switch (_animationType)
            {
                case "FadeIn":
                    Opacity = _animationProgress;
                    break;
                case "SlideIn":
                    Opacity = _animationProgress;
                    if (_role == "user")
                        Location = new Point(
                            _originalLocation.X - (int)(_slideDistance * (1 - _animationProgress)), 
                            _originalLocation.Y);
                    else
                        Location = new Point(
                            _originalLocation.X + (int)(_slideDistance * (1 - _animationProgress)), 
                            _originalLocation.Y);
                    break;
                case "GrowIn":
                    Opacity = _animationProgress;
                    Size = new Size(
                        (int)(Width + (Width * (1 - _animationProgress))),
                        (int)(Height + (Height * (1 - _animationProgress))));
                    break;
            }
            
            // Check if animation is complete
            if (_animationProgress >= 1.0f)
            {
                _isAnimating = false;
                _animationTimer.Stop();
                
                // Ensure final state is correct
                Opacity = 1.0f;
                Location = _originalLocation;
            }
            
            Invalidate();
        }
        
        /// <summary>
        /// Adjusts the height of the control based on the message text
        /// </summary>
        private void AdjustHeight()
        {
            if (string.IsNullOrEmpty(_messageText)) return;
            
            using (Graphics g = CreateGraphics())
            {
                SizeF textSize = g.MeasureString(_messageText, Font, Width - Padding.Horizontal);
                Height = (int)textSize.Height + Padding.Vertical;
            }
        }
        
        /// <summary>
        /// Paints the chat bubble with rounded corners and the message text
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw the bubble background with rounded corners
            using (GraphicsPath path = CreateRoundedRectangle(ClientRectangle, _cornerRadius))
            using (SolidBrush brush = new SolidBrush(_bubbleColor))
            {
                g.FillPath(brush, path);
            }
            
            // Draw the message text
            using (SolidBrush textBrush = new SolidBrush(_textColor))
            {
                Rectangle textRect = new Rectangle(
                    Padding.Left, 
                    Padding.Top, 
                    Width - Padding.Horizontal, 
                    Height - Padding.Vertical);
                
                g.DrawString(_messageText, Font, textBrush, textRect);
            }
        }
        
        /// <summary>
        /// Creates a rounded rectangle path
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
        /// Disposes of resources used by the control
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

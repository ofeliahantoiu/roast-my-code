using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using AForge;
using AForge.Imaging;

namespace RoastMyCode
{
    /// <summary>
    /// Control for handling webcam capture and applying visual effects
    /// </summary>
    public class WebcamControl : Panel
    {
        // Webcam devices and capture
        private FilterInfoCollection? _videoDevices;
        private VideoCaptureDevice? _videoSource;
        private bool _isWebcamActive = false;
        
        // UI components
        private PictureBox? _displayBox;
        private Button? _startButton;
        private Button? _stopButton;
        private ComboBox? _effectsComboBox;
        private Label? _statusLabel;
        
        // Effect settings
        private string _currentEffect = "None";
        private Bitmap? _currentFrame;
        private bool _isDarkMode = true;
        
        // Animation properties
        private System.Windows.Forms.Timer? _animationTimer;
        private float _animationIntensity = 0.0f;
        private bool _animationIncreasing = true;
        private int _animationSpeed = 50; // milliseconds
        private float _animationStep = 0.05f;
        private int _animationDuration = 3000; // milliseconds
        private DateTime _animationStartTime;
        private bool _isAnimating = false;
        private int _roastSeverity = 1; // 1-5 scale, 5 being most severe
        
        // Events
        public event EventHandler<WebcamPermissionEventArgs>? WebcamPermissionChanged;

        public bool IsWebcamActive => _isWebcamActive;
        
        public string CurrentEffect
        {
            get => _currentEffect;
            set
            {
                _currentEffect = value;
                // Apply effect immediately if webcam is active
                if (_isWebcamActive && _currentFrame != null)
                {
                    ApplyEffect();
                }
            }
        }
        
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                UpdateAppearance();
            }
        }
        
        public WebcamControl()
        {
            InitializeComponent();
            InitializeWebcam();
        }
        
        private void InitializeComponent()
        {
            // Configure the main panel
            this.Size = new Size(320, 280);
            this.BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            
            // Create the display box for webcam feed
            _displayBox = new PictureBox
            {
                Size = new Size(320, 240),
                Location = new System.Drawing.Point(0, 0),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = _isDarkMode ? Color.FromArgb(20, 20, 20) : Color.LightGray
            };
            
            // Create status label
            _statusLabel = new Label
            {
                Text = "Webcam: Not active",
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                Location = new System.Drawing.Point(10, 245),
                AutoSize = true
            };
            
            // Create start button
            _startButton = new Button
            {
                Text = "Start Webcam",
                Location = new System.Drawing.Point(10, 245),
                Size = new Size(100, 25),
                BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray,
                ForeColor = _isDarkMode ? Color.White : Color.Black
            };
            _startButton.Click += StartButton_Click;
            
            // Create stop button
            _stopButton = new Button
            {
                Text = "Stop Webcam",
                Location = new System.Drawing.Point(120, 245),
                Size = new Size(100, 25),
                BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray,
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                Enabled = false
            };
            _stopButton.Click += StopButton_Click;
            
            // Create effects combo box
            _effectsComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(230, 245),
                Size = new Size(80, 25),
                BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.White,
                ForeColor = _isDarkMode ? Color.White : Color.Black
            };
            _effectsComboBox.Items.AddRange(new object[] { "None", "Grayscale", "Invert", "Clown", "Pixelate", "Sepia" });
            _effectsComboBox.SelectedIndex = 0;
            _effectsComboBox.SelectedIndexChanged += EffectsComboBox_SelectedIndexChanged;
            
            // Add controls to the panel
            this.Controls.Add(_displayBox);
            this.Controls.Add(_startButton);
            this.Controls.Add(_stopButton);
            this.Controls.Add(_effectsComboBox);
            this.Controls.Add(_statusLabel);
            
            // Initially hide the status label (shown only when there's an error)
            _statusLabel.Visible = false;
        }
        
        private void InitializeWebcam()
        {
            try
            {
                // Get available video devices
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                
                if (_videoDevices.Count == 0)
                {
                    ShowWebcamError("No webcam devices found");
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowWebcamError($"Error initializing webcam: {ex.Message}");
            }
        }
        
        private void StartButton_Click(object? sender, EventArgs e)
        {
            StartWebcam();
        }
        
        private void StopButton_Click(object? sender, EventArgs e)
        {
            StopWebcam();
        }
        
        private void EffectsComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _currentEffect = _effectsComboBox?.SelectedItem?.ToString() ?? "None";
        }
        
        public void StartWebcam()
        {
            try
            {
                if (_videoDevices == null || _videoDevices.Count == 0)
                {
                    ShowWebcamError("No webcam devices available");
                    OnWebcamPermissionChanged(false);
                    return;
                }
                
                // Use the first available webcam
                _videoSource = new VideoCaptureDevice(_videoDevices[0].MonikerString);
                _videoSource.NewFrame += VideoSource_NewFrame;
                
                // Start the video source
                _videoSource.Start();
                _isWebcamActive = true;
                
                // Update UI
                if (_startButton != null)
                    _startButton.Enabled = false;
                if (_stopButton != null)
                    _stopButton.Enabled = true;
                if (_statusLabel != null)
                    _statusLabel.Visible = false;
                
                // Notify that webcam is active
                OnWebcamPermissionChanged(true);
            }
            catch (Exception ex)
            {
                ShowWebcamError($"Error starting webcam: {ex.Message}");
                OnWebcamPermissionChanged(false);
            }
        }
        
        public void StopWebcam()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                // Signal that we're stopping the webcam
                _videoSource.SignalToStop();
                _videoSource.WaitForStop();
                _videoSource.NewFrame -= VideoSource_NewFrame;
                _videoSource = null;
                _isWebcamActive = false;
                
                // Clear the display
                if (_displayBox != null && _displayBox.Image != null)
                {
                    _displayBox.Image.Dispose();
                    _displayBox.Image = null;
                }
                _currentFrame = null;
                
                // Update UI
                if (_startButton != null)
                    _startButton.Enabled = true;
                if (_stopButton != null)
                    _stopButton.Enabled = false;
                
                // Notify that webcam is inactive
                OnWebcamPermissionChanged(false);
            }
        }
        
        private void VideoSource_NewFrame(object? sender, NewFrameEventArgs eventArgs)
        {
            // Make a copy of the current frame
            _currentFrame = (Bitmap)eventArgs.Frame.Clone();
            
            // Apply any selected effect
            ApplyEffect();
            
            // Update the display
            if (_displayBox != null)
            {
                if (_displayBox.Image != null)
                {
                    _displayBox.Image.Dispose();
                }
                
                if (_currentFrame != null)
                {
                    _displayBox.Image = (Bitmap)_currentFrame.Clone();
                }
            }
        }
        
        /// <summary>
        /// Initializes the animation timer for dynamic effects
        /// </summary>
        private void InitializeAnimationTimer()
        {
            if (_animationTimer == null)
            {
                _animationTimer = new System.Windows.Forms.Timer();
                _animationTimer.Interval = _animationSpeed;
                _animationTimer.Tick += AnimationTimer_Tick;
            }
        }
        
        /// <summary>
        /// Updates the animation state on each timer tick
        /// </summary>
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isAnimating) return;
            
            // Check if animation duration has elapsed
            if ((DateTime.Now - _animationStartTime).TotalMilliseconds > _animationDuration)
            {
                StopAnimation();
                return;
            }
            
            // Update animation intensity
            if (_animationIncreasing)
            {
                _animationIntensity += _animationStep;
                if (_animationIntensity >= 1.0f)
                {
                    _animationIntensity = 1.0f;
                    _animationIncreasing = false;
                }
            }
            else
            {
                _animationIntensity -= _animationStep;
                if (_animationIntensity <= 0.0f)
                {
                    _animationIntensity = 0.0f;
                    _animationIncreasing = true;
                }
            }
        }
        
        /// <summary>
        /// Starts an animated effect based on roast severity
        /// </summary>
        /// <param name="severity">Severity level 1-5</param>
        /// <param name="effectName">Name of the effect to apply</param>
        public void StartAnimatedEffect(int severity, string effectName)
        {
            _roastSeverity = Math.Clamp(severity, 1, 5);
            _currentEffect = effectName;
            
            // Configure animation based on severity
            _animationStep = 0.05f * _roastSeverity / 3.0f;
            _animationDuration = 3000 + (_roastSeverity * 1000);
            
            // Initialize animation state
            _animationIntensity = 0.0f;
            _animationIncreasing = true;
            _isAnimating = true;
            _animationStartTime = DateTime.Now;
            
            // Start the animation timer
            InitializeAnimationTimer();
            if (_animationTimer != null && !_animationTimer.Enabled)
            {
                _animationTimer.Start();
            }
        }
        
        /// <summary>
        /// Stops the current animation
        /// </summary>
        public void StopAnimation()
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
            }
            
            _isAnimating = false;
            _animationIntensity = 0.0f;
        }
        
        /// <summary>
        /// Disposes of webcam and animation resources
        /// </summary>
        public void Dispose()
        {
            StopWebcam();
            
            // Stop and dispose animation timer
            StopAnimation();
            if (_animationTimer != null)
            {
                _animationTimer.Tick -= AnimationTimer_Tick;
                _animationTimer.Dispose();
                _animationTimer = null;
            }
            
            // Dispose of the video source if it exists
            if (_videoSource != null)
            {
                _videoSource.SignalToStop();
                _videoSource = null;
            }
            
            // Dispose of the current frame if it exists
            if (_currentFrame != null)
            {
                _currentFrame.Dispose();
                _currentFrame = null;
            }
        }
        
        private void ApplyEffect()
        {
            if (_currentFrame == null) return;
            
            Bitmap processedFrame = (Bitmap)_currentFrame.Clone();
            
            // Calculate effect intensity based on animation and roast severity
            float effectIntensity = _isAnimating ? _animationIntensity * _roastSeverity / 3.0f : 1.0f;
            
            switch (_currentEffect)
            {
                case "Grayscale":
                    Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
                    processedFrame = grayscaleFilter.Apply(processedFrame);
                    
                    // Add dynamic contrast based on animation
                    if (_isAnimating)
                    {
                        BrightnessCorrection brightnessFilter = new BrightnessCorrection((int)(effectIntensity * 30));
                        processedFrame = brightnessFilter.Apply(processedFrame);
                    }
                    break;
                    
                case "Invert":
                    Invert invertFilter = new Invert();
                    processedFrame = invertFilter.Apply(processedFrame);
                    
                    // Add dynamic color shift based on animation
                    if (_isAnimating)
                    {
                        ChannelFiltering channelFilter = new ChannelFiltering();
                        channelFilter.Red = new IntRange(0, (int)(255 * (1.0f - effectIntensity * 0.5f)));
                        processedFrame = channelFilter.Apply(processedFrame);
                    }
                    break;
                    
                case "Clown":
                    // Increase saturation and distort colors for clown effect
                    HSLFiltering hslFilter = new HSLFiltering();
                    
                    // Dynamic saturation based on animation intensity
                    float saturationMin = 0.6f + (effectIntensity * 0.2f);
                    float saturationMax = 0.8f + (effectIntensity * 0.2f);
                    hslFilter.Saturation = new AForge.Range(saturationMin, saturationMax);
                    
                    // Dynamic hue shift based on animation
                    if (_isAnimating)
                    {
                        hslFilter.Hue = new AForge.Range(-0.1f * effectIntensity, 0.1f * effectIntensity);
                    }
                    
                    processedFrame = hslFilter.Apply(processedFrame);
                    
                    // Add some color distortion with animation
                    ChannelFiltering channelFilter = new ChannelFiltering();
                    channelFilter.Red = new IntRange((int)(100 * (1.0f - effectIntensity * 0.3f)), 255);
                    channelFilter.Green = new IntRange(0, (int)(200 * (1.0f + effectIntensity * 0.2f)));
                    channelFilter.Blue = new IntRange(0, (int)(200 * (1.0f - effectIntensity * 0.3f)));
                    processedFrame = channelFilter.Apply(processedFrame);
                    break;
                    
                case "Pixelate":
                    // Create a pixelation effect with dynamic pixel size
                    Pixellate pixellateFilter = new Pixellate();
                    pixellateFilter.PixelSize = (int)(3 + (effectIntensity * _roastSeverity));
                    processedFrame = pixellateFilter.Apply(processedFrame);
                    break;
                    
                case "Sepia":
                    // Apply a sepia tone effect
                    Sepia sepiaFilter = new Sepia();
                    processedFrame = sepiaFilter.Apply(processedFrame);
                    
                    // Add vignette effect based on animation
                    if (_isAnimating && effectIntensity > 0.5f)
                    {
                        // Create a simple vignette by darkening edges
                        using (Graphics g = Graphics.FromImage(processedFrame))
                        {
                            int width = processedFrame.Width;
                            int height = processedFrame.Height;
                            
                            // Create a gradient brush for vignette
                            using (System.Drawing.Drawing2D.PathGradientBrush pgb = 
                                   new System.Drawing.Drawing2D.PathGradientBrush(new System.Drawing.Point[] {
                                       new System.Drawing.Point(0, 0),
                                       new System.Drawing.Point(width, 0),
                                       new System.Drawing.Point(width, height),
                                       new System.Drawing.Point(0, height)
                                   }))
                            {
                                pgb.CenterColor = Color.FromArgb(0, 0, 0, 0); // Transparent center
                                pgb.SurroundColors = new Color[] { Color.FromArgb((int)(150 * effectIntensity), 0, 0, 0) };
                                g.FillRectangle(pgb, 0, 0, width, height);
                            }
                        }
                    }
                    break;
                    
                case "None":
                default:
                    // No effect, use the original frame
                    break;
            }
            
            _currentFrame = processedFrame;
        }
        
        private void ShowWebcamError(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = message;
                _statusLabel.Visible = true;
                _statusLabel.ForeColor = Color.Red;
            }
            Debug.WriteLine($"Webcam error: {message}");
            
            // Show permission denied UI
            ShowPermissionDeniedUI();
        }
        
        /// <summary>
        /// Shows a user-friendly UI when webcam permission is denied
        /// </summary>
        private void ShowPermissionDeniedUI()
        {
            // Clear any existing permission UI elements
            foreach (Control control in Controls)
            {
                if (control.Tag?.ToString() == "permission_ui")
                {
                    Controls.Remove(control);
                    control.Dispose();
                }
            }
            
            // Create a panel for the permission denied message
            Panel permissionPanel = new Panel
            {
                Size = new Size(300, 200),
                Location = new System.Drawing.Point(10, 20),
                BackColor = _isDarkMode ? Color.FromArgb(40, 40, 40) : Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = "permission_ui"
            };
            
            // Add an icon
            PictureBox cameraIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new System.Drawing.Point((permissionPanel.Width - 48) / 2, 20),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Tag = "permission_ui"
            };
            
            // Create camera icon dynamically
            Bitmap iconBitmap = new Bitmap(48, 48);
            using (Graphics g = Graphics.FromImage(iconBitmap))
            {
                g.Clear(_isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // Draw camera shape
                using (Pen pen = new Pen(_isDarkMode ? Color.White : Color.Black, 2))
                {
                    g.DrawRectangle(pen, 8, 12, 32, 24);
                    
                    // Draw lens
                    g.DrawEllipse(pen, 16, 16, 16, 16);
                    
                    // Draw slash for "no camera"
                    pen.Color = Color.Red;
                    g.DrawLine(pen, 8, 8, 40, 40);
                }
            }
            cameraIcon.Image = iconBitmap;
            permissionPanel.Controls.Add(cameraIcon);
            
            // Add heading
            Label headingLabel = new Label
            {
                Text = "Camera Access Denied",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                AutoSize = true,
                Location = new System.Drawing.Point(0, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = permissionPanel.Width,
                Tag = "permission_ui"
            };
            permissionPanel.Controls.Add(headingLabel);
            
            // Add instructions
            Label instructionsLabel = new Label
            {
                Text = "Please enable camera access in your browser or system settings to use this feature.",
                Font = new Font("Segoe UI", 9),
                ForeColor = _isDarkMode ? Color.LightGray : Color.DarkGray,
                Location = new System.Drawing.Point(10, 110),
                Size = new Size(280, 60),
                TextAlign = ContentAlignment.TopCenter,
                Tag = "permission_ui"
            };
            permissionPanel.Controls.Add(instructionsLabel);
            
            // Add retry button
            Button retryButton = new Button
            {
                Text = "Retry",
                Size = new Size(100, 30),
                Location = new System.Drawing.Point((permissionPanel.Width - 100) / 2, 160),
                BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray,
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                FlatStyle = FlatStyle.Flat,
                Tag = "permission_ui"
            };
            retryButton.Click += (s, e) => {
                Controls.Remove(permissionPanel);
                permissionPanel.Dispose();
                StartWebcam();
            };
            permissionPanel.Controls.Add(retryButton);
            
            // Add the panel to the control
            Controls.Add(permissionPanel);
            permissionPanel.BringToFront();
        }
        
        private void UpdateAppearance()
        {
            this.BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            
            if (_displayBox != null)
                _displayBox.BackColor = _isDarkMode ? Color.FromArgb(20, 20, 20) : Color.LightGray;
                
            if (_statusLabel != null)
                _statusLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
                
            if (_startButton != null)
            {
                _startButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray;
                _startButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            }
            
            if (_stopButton != null)
            {
                _stopButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray;
                _stopButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            }
            
            if (_effectsComboBox != null)
            {
                _effectsComboBox.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.White;
                _effectsComboBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
            }
        }
        
        protected void OnWebcamPermissionChanged(bool isGranted)
        {
            WebcamPermissionChanged?.Invoke(this, new WebcamPermissionEventArgs(isGranted));
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Make sure to stop the webcam when the control is disposed
                StopWebcam();
            }
            
            base.Dispose(disposing);
        }
    }
    
    public class WebcamPermissionEventArgs : EventArgs
    {
        public bool IsPermissionGranted { get; }
        
        public WebcamPermissionEventArgs(bool isPermissionGranted)
        {
            IsPermissionGranted = isPermissionGranted;
        }
    }
}

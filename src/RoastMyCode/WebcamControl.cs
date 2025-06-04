using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;

namespace RoastMyCode
{
    /// <summary>
    /// Control for handling webcam capture and applying visual effects
    /// </summary>
    public class WebcamControl : Panel
    {
        // Webcam devices and capture
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private bool _isWebcamActive = false;
        
        // UI components
        private PictureBox _displayBox;
        private Button _startButton;
        private Button _stopButton;
        private ComboBox _effectsComboBox;
        private Label _statusLabel;
        
        // Effect settings
        private string _currentEffect = "None";
        private Bitmap _currentFrame;
        private bool _isDarkMode = true;
        
        // Events
        public event EventHandler<WebcamPermissionEventArgs> WebcamPermissionChanged;

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
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = _isDarkMode ? Color.FromArgb(20, 20, 20) : Color.LightGray
            };
            
            // Create status label
            _statusLabel = new Label
            {
                Text = "Webcam: Not active",
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                Location = new Point(10, 245),
                AutoSize = true
            };
            
            // Create start button
            _startButton = new Button
            {
                Text = "Start Webcam",
                Location = new Point(10, 245),
                Size = new Size(100, 25),
                BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray,
                ForeColor = _isDarkMode ? Color.White : Color.Black
            };
            _startButton.Click += StartButton_Click;
            
            // Create stop button
            _stopButton = new Button
            {
                Text = "Stop Webcam",
                Location = new Point(120, 245),
                Size = new Size(100, 25),
                BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray,
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                Enabled = false
            };
            _stopButton.Click += StopButton_Click;
            
            // Create effects combo box
            _effectsComboBox = new ComboBox
            {
                Location = new Point(230, 245),
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
        
        private void StartButton_Click(object sender, EventArgs e)
        {
            StartWebcam();
        }
        
        private void StopButton_Click(object sender, EventArgs e)
        {
            StopWebcam();
        }
        
        private void EffectsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentEffect = _effectsComboBox.SelectedItem.ToString();
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
                _startButton.Enabled = false;
                _stopButton.Enabled = true;
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
                _displayBox.Image = null;
                _currentFrame = null;
                
                // Update UI
                _startButton.Enabled = true;
                _stopButton.Enabled = false;
                
                // Notify that webcam is inactive
                OnWebcamPermissionChanged(false);
            }
        }
        
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Make a copy of the current frame
            _currentFrame = (Bitmap)eventArgs.Frame.Clone();
            
            // Apply any selected effect
            ApplyEffect();
            
            // Update the display
            if (_displayBox.Image != null)
            {
                _displayBox.Image.Dispose();
            }
            
            _displayBox.Image = (Bitmap)_currentFrame.Clone();
        }
        
        private void ApplyEffect()
        {
            if (_currentFrame == null) return;
            
            Bitmap processedFrame = (Bitmap)_currentFrame.Clone();
            
            switch (_currentEffect)
            {
                case "Grayscale":
                    Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
                    processedFrame = grayscaleFilter.Apply(processedFrame);
                    break;
                    
                case "Invert":
                    Invert invertFilter = new Invert();
                    processedFrame = invertFilter.Apply(processedFrame);
                    break;
                    
                case "Clown":
                    // Apply a combination of effects for a clown-like appearance
                    // Increase saturation and apply a red tint
                    HSLFiltering hslFilter = new HSLFiltering();
                    hslFilter.SaturationAdjustment = 0.6f;
                    processedFrame = hslFilter.Apply(processedFrame);
                    
                    // Add some color distortion
                    ChannelFiltering channelFilter = new ChannelFiltering();
                    channelFilter.Red = new IntRange(100, 255);
                    channelFilter.Green = new IntRange(0, 200);
                    channelFilter.Blue = new IntRange(0, 200);
                    processedFrame = channelFilter.Apply(processedFrame);
                    break;
                    
                case "Pixelate":
                    // Create a pixelation effect
                    Pixellate pixellateFilter = new Pixellate();
                    pixellateFilter.PixelSize = 5;
                    processedFrame = pixellateFilter.Apply(processedFrame);
                    break;
                    
                case "Sepia":
                    // Apply a sepia tone effect
                    Sepia sepiaFilter = new Sepia();
                    processedFrame = sepiaFilter.Apply(processedFrame);
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
            _statusLabel.Text = message;
            _statusLabel.ForeColor = Color.Red;
            _statusLabel.Visible = true;
            _startButton.Enabled = true;
            _stopButton.Enabled = false;
        }
        
        private void UpdateAppearance()
        {
            this.BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            _displayBox.BackColor = _isDarkMode ? Color.FromArgb(20, 20, 20) : Color.LightGray;
            _statusLabel.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _startButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray;
            _startButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _stopButton.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray;
            _stopButton.ForeColor = _isDarkMode ? Color.White : Color.Black;
            _effectsComboBox.BackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.White;
            _effectsComboBox.ForeColor = _isDarkMode ? Color.White : Color.Black;
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

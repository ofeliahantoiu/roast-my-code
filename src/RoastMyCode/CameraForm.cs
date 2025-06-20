using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using RoastMyCode.Services;

namespace RoastMyCode
{
    public partial class CameraForm : Form
    {
        private readonly ICameraService _cameraService;
        private PictureBox? _previewBox;
        private ComboBox? _cameraComboBox;
        private Button? _startButton;
        private Button? _captureButton;
        private Button? _saveButton;
        private Button? _closeButton;
        private Bitmap? _capturedImage;
        private bool _isCameraRunning = false;

        // Event for communicating with the main form
        public event EventHandler<(string message, Image image)>? ImageCaptured;

        public CameraForm(ICameraService cameraService)
        {
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            InitializeComponent();
            InitializeCameraForm();
            LoadAvailableCameras();
        }

        private void InitializeComponent()
        {
            this.Text = "Camera - Roast My Code";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - 650, 100);
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.TopMost = true;
            this.ShowInTaskbar = false;
        }

        private void InitializeCameraForm()
        {
            // Camera selection panel
            var cameraPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            var cameraLabel = new Label
            {
                Text = "Select Camera:",
                Location = new Point(10, 20),
                AutoSize = true,
                ForeColor = Color.White
            };

            _cameraComboBox = new ComboBox
            {
                Location = new Point(100, 17),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };

            _startButton = new Button
            {
                Text = "Start Camera",
                Location = new Point(320, 17),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _startButton.Click += StartButton_Click;

            cameraPanel.Controls.AddRange(new Control[] { cameraLabel, _cameraComboBox, _startButton });

            // Preview panel
            var previewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Padding = new Padding(10)
            };

            _previewBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };

            previewPanel.Controls.Add(_previewBox);

            // Control panel
            var controlPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            _captureButton = new Button
            {
                Text = "Capture",
                Location = new Point(10, 20),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _captureButton.Click += CaptureButton_Click;

            _saveButton = new Button
            {
                Text = "Save Image",
                Location = new Point(120, 20),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _saveButton.Click += SaveButton_Click;

            var sendToChatButton = new Button
            {
                Text = "Send to Chat",
                Location = new Point(230, 20),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 150, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            sendToChatButton.Click += SendToChatButton_Click;

            var sendFileButton = new Button
            {
                Text = "Send File",
                Location = new Point(340, 20),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(150, 100, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            sendFileButton.Click += SendFileButton_Click;

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(450, 20),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _closeButton.Click += CloseButton_Click;

            controlPanel.Controls.AddRange(new Control[] { _captureButton, _saveButton, sendToChatButton, sendFileButton, _closeButton });

            // Add panels to form
            this.Controls.AddRange(new Control[] { cameraPanel, previewPanel, controlPanel });

            // Subscribe to camera service events
            _cameraService.CameraError += CameraService_CameraError;
        }

        private void LoadAvailableCameras()
        {
            try
            {
                var cameras = _cameraService.GetAvailableCameras();
                if (_cameraComboBox != null)
                {
                    _cameraComboBox.Items.Clear();
                    if (cameras != null && cameras.Any())
                    {
                        _cameraComboBox.Items.AddRange(cameras.ToArray());
                    }
                }

                if (_cameraComboBox != null)
                {
                    if (_cameraComboBox.Items.Count > 0)
                    {
                        _cameraComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("No cameras found on this system.", "No Cameras", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading cameras: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            if (!_isCameraRunning)
            {
                StartCamera();
            }
            else
            {
                StopCamera();
            }
        }

        private void StartCamera()
        {
            var selectedItem = _cameraComboBox?.SelectedItem;
            string selectedCamera = string.Empty;

            if (selectedItem is string s)
            {
                selectedCamera = s;
            }
            else if (selectedItem != null)
            {
                selectedCamera = selectedItem.ToString() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(selectedCamera))
            {
                MessageBox.Show("Please select a camera first.", "No Camera Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_previewBox != null)
                {
                    _cameraService.StartCamera(selectedCamera, _previewBox);
                    _isCameraRunning = true;
                    if (_startButton != null) _startButton.Text = "Stop Camera";
                    if (_captureButton != null) _captureButton.Enabled = true;
                    if (_cameraComboBox != null) _cameraComboBox.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting camera: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopCamera()
        {
            try
            {
                _cameraService.StopCamera();
                _isCameraRunning = false;
                if (_startButton != null) _startButton.Text = "Start Camera";
                if (_captureButton != null) _captureButton.Enabled = false;
                if (_cameraComboBox != null) _cameraComboBox.Enabled = true;
                if (_previewBox != null)
                {
                    _previewBox.Image?.Dispose();
                    _previewBox.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping camera: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CaptureButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _capturedImage = _cameraService.CaptureImage();
                if (_capturedImage != null)
                {
                    if (_saveButton != null) _saveButton.Enabled = true;
                    // Enable the send to chat button
                    foreach (Control control in this.Controls)
                    {
                        if (control is Panel panel)
                        {
                            foreach (Control panelControl in panel.Controls)
                            {
                                if (panelControl is Button button && button.Text == "Send to Chat")
                                {
                                    button.Enabled = true;
                                    break;
                                }
                            }
                        }
                    }
                    MessageBox.Show("Image captured successfully!", "Capture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error capturing image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (_capturedImage == null)
            {
                MessageBox.Show("No image to save. Please capture an image first.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "JPEG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp",
                Title = "Save Captured Image",
                FileName = $"RoastMyCode_Capture_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ImageFormat format = saveDialog.FilterIndex switch
                    {
                        1 => ImageFormat.Jpeg,
                        2 => ImageFormat.Png,
                        3 => ImageFormat.Bmp,
                        _ => ImageFormat.Jpeg
                    };

                    _capturedImage.Save(saveDialog.FileName, format);
                    MessageBox.Show($"Image saved successfully to:\n{saveDialog.FileName}", "Save Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SendToChatButton_Click(object? sender, EventArgs e)
        {
            if (_capturedImage != null)
            {
                string message = "📷 Photo captured";
                ImageCaptured?.Invoke(this, (message, _capturedImage));
                MessageBox.Show("Image sent to chat!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please capture an image first.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SendFileButton_Click(object? sender, EventArgs e)
        {
            try
            {
                using OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files (*.*)|*.*",
                    Title = "Select Image to Send to Chat"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var bitmap = new Bitmap(openFileDialog.FileName);
                        string message = $"📷 Image from file: {Path.GetFileName(openFileDialog.FileName)}";
                        ImageCaptured?.Invoke(this, (message, bitmap));
                        MessageBox.Show($"Image sent to chat!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            StopCamera();
            this.Close();
        }

        private void CameraService_CameraError(object? sender, string error)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => CameraService_CameraError(sender, error)));
                return;
            }

            MessageBox.Show($"Camera Error: {error}", "Camera Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopCamera();
            _cameraService.CameraError -= CameraService_CameraError;
            _capturedImage?.Dispose();
            base.OnFormClosing(e);
        }
    }
} 
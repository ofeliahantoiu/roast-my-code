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
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
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

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(230, 20),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _closeButton.Click += CloseButton_Click;

            controlPanel.Controls.AddRange(new Control[] { _captureButton, _saveButton, _closeButton });

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
                _cameraComboBox.Items.Clear();
                _cameraComboBox.Items.AddRange(cameras.ToArray());

                if (_cameraComboBox.Items.Count > 0)
                {
                    _cameraComboBox.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No cameras found on this system.", "No Cameras", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (_cameraComboBox?.SelectedItem == null)
            {
                MessageBox.Show("Please select a camera first.", "No Camera Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string selectedCamera = _cameraComboBox.SelectedItem.ToString()!;
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
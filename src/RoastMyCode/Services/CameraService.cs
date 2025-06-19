using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace RoastMyCode.Services
{
    public interface ICameraService
    {
        List<string> GetAvailableCameras();
        void StartCamera(string cameraName, PictureBox pictureBox);
        void StopCamera();
        Bitmap? CaptureImage();
        bool IsCameraRunning { get; }
        event EventHandler<string> CameraError;
    }

    public class CameraService : ICameraService, IDisposable
    {
        private VideoCapture? _videoCapture;
        private PictureBox? _previewBox;
        private bool _isRunning = false;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _captureTask;

        public bool IsCameraRunning => _isRunning;

        public event EventHandler<string>? CameraError;

        public List<string> GetAvailableCameras()
        {
            try
            {
                var cameras = new List<string>();
                for (int i = 0; i < 10; i++) // Check first 10 camera indices
                {
                    using var capture = new VideoCapture(i);
                    if (capture.IsOpened())
                    {
                        cameras.Add($"Camera {i}");
                        capture.Release();
                    }
                }
                return cameras;
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, $"Error getting camera list: {ex.Message}");
                return new List<string>();
            }
        }

        public void StartCamera(string cameraName, PictureBox pictureBox)
        {
            try
            {
                StopCamera();

                // Extract camera index from name (e.g., "Camera 0" -> 0)
                if (!int.TryParse(cameraName.Split(' ').Last(), out int cameraIndex))
                {
                    CameraError?.Invoke(this, $"Invalid camera name: {cameraName}");
                    return;
                }

                _videoCapture = new VideoCapture(cameraIndex);
                if (!_videoCapture.IsOpened())
                {
                    CameraError?.Invoke(this, $"Failed to open camera: {cameraName}");
                    return;
                }

                _previewBox = pictureBox;
                _cancellationTokenSource = new CancellationTokenSource();
                _captureTask = Task.Run(() => CaptureLoop(_cancellationTokenSource.Token));
                _isRunning = true;
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, $"Error starting camera: {ex.Message}");
            }
        }

        public void StopCamera()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _captureTask?.Wait(1000); // Wait up to 1 second for task to complete
                
                _videoCapture?.Release();
                _videoCapture?.Dispose();
                _videoCapture = null;
                
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                
                _isRunning = false;
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, $"Error stopping camera: {ex.Message}");
            }
        }

        public Bitmap? CaptureImage()
        {
            try
            {
                if (_previewBox?.Image == null)
                {
                    CameraError?.Invoke(this, "No image available to capture");
                    return null;
                }

                return new Bitmap(_previewBox.Image);
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, $"Error capturing image: {ex.Message}");
                return null;
            }
        }

        private async Task CaptureLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _videoCapture != null && _videoCapture.IsOpened())
                {
                    using var frame = new Mat();
                    if (_videoCapture.Read(frame))
                    {
                        if (!frame.Empty())
                        {
                            var bitmap = MatToBitmap(frame);
                            if (_previewBox != null && _previewBox.InvokeRequired)
                            {
                                _previewBox.Invoke(new Action(() => UpdatePreview(bitmap)));
                            }
                            else if (_previewBox != null)
                            {
                                UpdatePreview(bitmap);
                            }
                        }
                    }
                    
                    await Task.Delay(33, cancellationToken); // ~30 FPS
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping camera
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, $"Error in capture loop: {ex.Message}");
            }
        }

        private Bitmap MatToBitmap(Mat mat)
        {
            try
            {
                if (mat == null || mat.Empty())
                {
                    return new Bitmap(1, 1); // Return a minimal valid bitmap
                }

                var bytes = mat.ToBytes();
                if (bytes == null || bytes.Length == 0)
                {
                    return new Bitmap(1, 1); // Return a minimal valid bitmap
                }

                using var stream = new MemoryStream(bytes);
                var bitmap = new Bitmap(stream);
                
                // Validate the created bitmap
                if (bitmap.Width <= 0 || bitmap.Height <= 0)
                {
                    bitmap.Dispose();
                    return new Bitmap(1, 1); // Return a minimal valid bitmap
                }

                return bitmap;
            }
            catch (Exception)
            {
                // Return a minimal valid bitmap if conversion fails
                return new Bitmap(1, 1);
            }
        }

        private void UpdatePreview(Bitmap bitmap)
        {
            try
            {
                if (_previewBox != null)
                {
                    _previewBox.Image?.Dispose();
                    _previewBox.Image = bitmap;
                }
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, $"Error updating preview: {ex.Message}");
            }
        }

        public void Dispose()
        {
            StopCamera();
            _previewBox?.Image?.Dispose();
        }
    }
} 
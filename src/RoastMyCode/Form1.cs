using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using NAudio.Wave;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using RoastMyCode.Services;
using static RoastMyCode.Services.LanguageDetector;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private readonly IConfiguration _configuration;
        private readonly IAIService _aiService;
        private readonly ICameraService _cameraService;
        private IWavePlayer? _waveOutDevice;
        private AudioFileReader? _audioFileReader;
        private readonly List<ChatMessage> _conversationHistory;
        private readonly FileUploadOptions _fileUploadOptions;
        private bool _isDarkMode = true;
        private Font _currentFont = new Font("Segoe UI", 10);
        private Dictionary<string, string> _uploadedFiles = new();
        private string[] _codeExtensions = Array.Empty<string>();
        private long _currentTotalSizeBytes = 0;
        private readonly SpeechSynthesizer _speechSynthesizer = new();
        private string _selectedVoice = "Male";
        private string _lastAIMessage = string.Empty; // Store the last AI message for manual playback

        private Panel chatAreaPanel = null!;
        private PictureBox pbThemeToggle = null!;
        private ComboBox cmbFontStyle = null!;
        private ComboBox cmbFontSize = null!;
        private ComboBox cmbRoastLevel = null!;
        private ComboBox cmbVoiceType = null!;
        private Panel titleLogoPanel = null!;
        private RichTextBox rtInput = null!;
        private PictureBox? pbCameraIcon;
        private PictureBox? pbMicIcon;
        private PictureBox? pbUploadIcon = null!;
        private PictureBox pbSendIcon = null!;
        private PictureBox pbGradientBackground = null!;
        private Panel inputPanel = null!;

        private readonly Dictionary<string, string> _languageMap = new(StringComparer.OrdinalIgnoreCase)
        {
            [".cs"] = "C#", [".js"] = "JavaScript", [".ts"] = "TypeScript",
            [".py"] = "Python", [".java"] = "Java", [".cpp"] = "C++",
            [".c"] = "C", [".h"] = "C/C++ Header", [".hpp"] = "C++ Header",
            [".php"] = "PHP", [".rb"] = "Ruby", [".go"] = "Go", [".rs"] = "Rust",
            [".swift"] = "Swift", [".kt"] = "Kotlin", [".dart"] = "Dart",
            [".sh"] = "Shell Script", [".ps1"] = "PowerShell", [".bat"] = "Batch",
            [".cmd"] = "Command Script", [".html"] = "HTML", [".css"] = "CSS",
            [".xml"] = "XML", [".json"] = "JSON", [".yaml"] = "YAML",
            [".yml"] = "YAML", [".md"] = "Markdown", [".txt"] = "Text",
            ["dockerfile"] = "Dockerfile", [".dockerignore"] = "Docker Ignore",
            [".gitignore"] = "Git Ignore", ["makefile"] = "Makefile",
            ["readme"] = "Readme", ["license"] = "License"
        };

        public Form1(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _aiService = new AIService(_configuration);
                _cameraService = new CameraService();
                _conversationHistory = new List<ChatMessage>();

                _fileUploadOptions = (FileUploadOptions?)serviceProvider.GetService(typeof(FileUploadOptions))
                    ?? throw new InvalidOperationException("Failed to resolve FileUploadOptions from service provider");    

                _fileUploadOptions.AllowedExtensions ??= _languageMap.Keys.Where(ext => ext.StartsWith(".")).ToArray();
                _fileUploadOptions.MaxFileSizeMB = Math.Max(1, _fileUploadOptions.MaxFileSizeMB);
                _fileUploadOptions.MaxTotalSizeMB = Math.Max(1, _fileUploadOptions.MaxTotalSizeMB);

                _codeExtensions = _languageMap.Keys.Where(k => k.StartsWith(".")).ToArray();

                _conversationHistory.Add(new ChatMessage
                {
                    Role = "assistant",
                    Content = "Welcome to Roast My Code. I read your code, roast it out loud, and snap your reaction, don't worry, just your face, not your soul. Now show me what disaster you've cooked up."
                });

                InitializeComponent();
                InitializeModernUI();
                InitializeVoiceTypeComboBox();
                ApplyTheme();
                LoadConversationHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Form1: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private string DetectLanguage(string fileName, string? content = null)
        {
            string fileNameLower = Path.GetFileName(fileName).ToLowerInvariant();
            if (_languageMap.ContainsKey(fileNameLower)) return _languageMap[fileNameLower];

            foreach (var pattern in _languageMap.Keys.Where(k => k.EndsWith("/")))
            {
                if (fileName.Replace("\\", "/").Contains(pattern)) return _languageMap[pattern];
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!string.IsNullOrEmpty(extension) && _languageMap.ContainsKey(extension)) return _languageMap[extension];

            if (!string.IsNullOrEmpty(content))
            {
                string detectedLanguage = LanguageDetector.DetectLanguage(content);
                if (detectedLanguage != "text")
                {
                    return detectedLanguage;
                }

                if (content.StartsWith("#!"))
                {
                    if (content.Contains("python")) return "Python Script";
                    if (content.Contains("bash")) return "Bash Script";
                    if (content.Contains("sh")) return "Shell Script";
                    if (content.Contains("node")) return "Node.js Script";
                    if (content.Contains("ruby")) return "Ruby Script";
                    if (content.Contains("perl")) return "Perl Script";
                }
                if (content.TrimStart().StartsWith("<?xml")) return "XML";
                if (content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("["))
                {
                    try { _ = JsonDocument.Parse(content); return "JSON"; } catch { }
                }
            }

            return "Unknown";
        }

        private void ChatAreaPanel_Resize(object? sender, EventArgs e)
        {
            int panelWidth = chatAreaPanel.ClientSize.Width;
            if (panelWidth <= 0) return;

            int desiredBubbleWidth = (int)(panelWidth * 0.70) - chatAreaPanel.Padding.Horizontal -
                                     (chatAreaPanel.Controls.Count > 0 ? chatAreaPanel.Controls[0].Margin.Horizontal : 0);
            if (desiredBubbleWidth < 1) desiredBubbleWidth = 1;

            foreach (Control control in chatAreaPanel.Controls)
            {
                if (control is ChatMessageBubble bubble)
                {
                    bubble.Width = desiredBubbleWidth;
                    bubble.MaximumSize = new Size(bubble.Width, 0);
                    bubble.Left = bubble.Role == "user"
                        ? panelWidth - chatAreaPanel.Padding.Right - bubble.Width
                        : chatAreaPanel.Padding.Left;
                    bubble.PerformLayout();
                }
            }
            chatAreaPanel.Invalidate(true);
            chatAreaPanel.PerformLayout();
        }

        private void LoadImageFromAssets(PictureBox pictureBox, string imageName)
        {
            try
            {
                string assetsPath = Path.Combine(AppContext.BaseDirectory, "assets", imageName);
                if (!File.Exists(assetsPath))
                    throw new FileNotFoundException($"Image not found: {assetsPath}");

                pictureBox.Image = Image.FromFile(assetsPath);
                this.FormClosed += (s, e) => pictureBox.Image?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image {imageName}: {ex.Message}", "Image Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (pbGradientBackground != null)
            {
                pbGradientBackground.Height = this.ClientSize.Height / 2;
                pbGradientBackground.SendToBack();
            }
        }

        private void PlaySoundEffect()
        {
            try
            {
                string soundFilePath = Path.Combine(AppContext.BaseDirectory, "assets", "sound1.mp3");
                if (!File.Exists(soundFilePath))
                {
                    MessageBox.Show($"Sound file not found at: {soundFilePath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _waveOutDevice = new WaveOutEvent();
                _audioFileReader = new AudioFileReader(soundFilePath);
                _waveOutDevice.PlaybackStopped += OnPlaybackStopped;
                _waveOutDevice.Init(_audioFileReader);
                _waveOutDevice.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing sound: {ex.Message}", "Sound Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            _audioFileReader?.Dispose();
            _audioFileReader = null;
            _waveOutDevice?.Dispose();
            _waveOutDevice = null;

            if (e.Exception != null)
            {
                MessageBox.Show($"Playback error: {e.Exception.Message}", "Sound Playback Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Speak(string text, string? voice)
        {
            _speechSynthesizer.SelectVoiceByHints(voice == "Female" ? VoiceGender.Female : VoiceGender.Male);
            _speechSynthesizer.SpeakAsync(text);
        }

        private void CmbVoiceType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _selectedVoice = cmbVoiceType.SelectedItem?.ToString() ?? "Male";
        }

        private void InitializeVoiceTypeComboBox()
        {
            cmbVoiceType.Items.Add("Male");
            cmbVoiceType.Items.Add("Female");
            cmbVoiceType.SelectedIndex = 0;
            cmbVoiceType.SelectedIndexChanged += CmbVoiceType_SelectedIndexChanged;
        }

        private async Task HandleSendMessage()
        {
            if (rtInput.Text == "Type your message here..." || string.IsNullOrWhiteSpace(rtInput.Text))
            {
                return;
            }

            string userMessage = rtInput.Text;
            
            // Add user message with single layout suspension
            chatAreaPanel.SuspendLayout();
            try
            {
                SendMessage();
                PositionChatBubbles();
            }
            finally
            {
                chatAreaPanel.ResumeLayout(true);
            }

            rtInput.Enabled = false;
            pbSendIcon.Enabled = false;

            try
            {
                string selectedLevel = (cmbRoastLevel.SelectedIndex > 0 ? cmbRoastLevel.SelectedItem?.ToString() : "Savage") ?? "Savage";
                string aiResponse = await _aiService.GenerateRoast(userMessage, selectedLevel, _conversationHistory);

                // Store the AI response for manual playback
                _lastAIMessage = aiResponse;
                
                // Add AI response with single layout suspension
                chatAreaPanel.SuspendLayout();
                try
                {
                    AddChatMessage(aiResponse, "assistant");
                    _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = aiResponse });
                    PositionChatBubbles();
                }
                finally
                {
                    chatAreaPanel.ResumeLayout(true);
                }
            }
            catch (Exception ex)
            {
                chatAreaPanel.SuspendLayout();
                try
                {
                    AddChatMessage($"Error: {ex.Message}", "system");
                    PositionChatBubbles();
                }
                finally
                {
                    chatAreaPanel.ResumeLayout(true);
                }
            }
            finally
            {
                rtInput.Enabled = true;
                pbSendIcon.Enabled = true;
                rtInput.Focus();
            }
        }

        private async Task HandleImageCapture((string message, Image image) data)
        {
            try
            {
                // Ensure we're on the UI thread
                if (InvokeRequired)
                {
                    await Task.Run(() => Invoke(new Action(async () => await HandleImageCapture(data))));
                    return;
                }

                // Disable input while processing
                rtInput.Enabled = false;
                pbSendIcon.Enabled = false;

                try
                {
                    // Add user message with photo indicator
                    string photoMessage = "📷 Photo captured";
                    chatAreaPanel.SuspendLayout();
                    try
                    {
                        AddChatMessage(photoMessage, "user");
                        _conversationHistory.Add(new ChatMessage { Content = photoMessage, Role = "user" });
                        PositionChatBubbles();
                    }
                    finally
                    {
                        chatAreaPanel.ResumeLayout(true);
                    }
                    
                    // Automatically roast the image
                    string selectedLevel = (cmbRoastLevel.SelectedIndex > 0 ? cmbRoastLevel.SelectedItem?.ToString() : "Savage") ?? "Savage";
                    string aiResponse = await _aiService.RoastImage(data.image, selectedLevel, _conversationHistory);
                    
                    // Store the AI response for manual playback
                    _lastAIMessage = aiResponse;
                    
                    // Add AI response
                    chatAreaPanel.SuspendLayout();
                    try
                    {
                        AddChatMessage(aiResponse, "assistant");
                        _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = aiResponse });
                        PositionChatBubbles();
                    }
                    finally
                    {
                        chatAreaPanel.ResumeLayout(true);
                    }
                }
                catch (Exception ex)
                {
                    chatAreaPanel.SuspendLayout();
                    try
                    {
                        AddChatMessage($"Error roasting image: {ex.Message}", "system");
                        PositionChatBubbles();
                    }
                    finally
                    {
                        chatAreaPanel.ResumeLayout(true);
                    }
                }
            }
            catch (Exception ex)
            {
                chatAreaPanel.SuspendLayout();
                try
                {
                    AddChatMessage($"Error processing image: {ex.Message}", "system");
                    PositionChatBubbles();
                }
                finally
                {
                    chatAreaPanel.ResumeLayout(true);
                }
            }
            finally
            {
                // Re-enable input
                rtInput.Enabled = true;
                pbSendIcon.Enabled = true;
                rtInput.Focus();
            }
        }

        private void BtnSend_Click(object? sender, EventArgs e)
        {
            _ = HandleSendMessage();
        }

        private void BtnDownloadConversation_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = $"Chat_History_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Conversation As"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var lines = new List<string>();

                    lines.Add($"Chat History - {DateTime.Now}");
                    lines.Add(new string('-', 80));

                    foreach (var message in _conversationHistory)
                    {
                        string timestamp = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]";
                        string role = message.Role.ToLower() switch
                        {
                            "user" => "You",
                            "assistant" => "Assistant",
                            _ => message.Role
                        };

                        lines.Add($"{timestamp} {role}:\n{message.Content.Trim()}");
                        lines.Add(new string('-', 80));
                    }

                    File.WriteAllLines(saveFileDialog.FileName, lines);

                    MessageBox.Show("Conversation saved successfully.", "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save conversation:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PbCameraIcon_Click(object? sender, EventArgs e)
        {
            try
            {
                var cameraForm = new CameraForm(_cameraService);
                cameraForm.ImageCaptured += async (s, data) => 
                {
                    // Ensure we're on the UI thread and handle all operations properly
                    if (InvokeRequired)
                    {
                        Invoke(new Action(async () => await HandleImageCapture(data)));
                        return;
                    }
                    
                    await HandleImageCapture(data);
                };
                cameraForm.Show(this); // Changed from ShowDialog() to Show() to make it non-modal
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening camera: {ex.Message}", "Camera Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
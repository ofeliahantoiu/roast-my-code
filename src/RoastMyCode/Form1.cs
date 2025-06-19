using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
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
        private readonly List<ChatMessage> _conversationHistory;
        private readonly FileUploadOptions _fileUploadOptions;
        private bool _isDarkMode = true;
        private Font _currentFont = new Font("Segoe UI", 10);
        private Dictionary<string, string> _uploadedFiles = new();
        private string[] _codeExtensions = Array.Empty<string>();
        private long _currentTotalSizeBytes = 0;
        private readonly SpeechSynthesizer _speechSynthesizer = new();

        private Panel chatAreaPanel = null!;
        private PictureBox pbThemeToggle = null!;
        private ComboBox cmbFontStyle = null!;
        private ComboBox cmbFontSize = null!;
        private ComboBox cmbRoastLevel = null!;
        private ComboBox cmbAnimation = null!;
        private Panel titleLogoPanel = null!;
        private RichTextBox rtInput = null!;
        private PictureBox? pbCameraIcon;
        private PictureBox? pbMicIcon;
        private PictureBox? pbUploadIcon = null!;
        private PictureBox pbSendIcon = null!;
        private PictureBox pbGradientBackground = null!;
        private Panel inputPanel = null!;
        private TextCounterDisplay textCounter = null!;

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
                    Content = "Glad you asked. Besides fixing your code and your life? Here's what I tolerate:\n\n• Reports - Like \"What's the last report we exported?\"\n• Your organization - \"How many people are using our software?\"\n• Features - \"How do I change the colors of my report?\""
                });

                InitializeComponent();
                InitializeModernUI();
                ApplyTheme();
                LoadConversationHistory();
                
                // Initialize the animation service
                Services.AnimationService.Instance.RegisterTarget(this);
                Services.AnimationService.Instance.RegisterAnimatedControl(chatAreaPanel);
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

            if (string.IsNullOrEmpty(extension) && !string.IsNullOrEmpty(content))
            {
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

        private async void rtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (rtInput.Text != "Type your message here..." && !string.IsNullOrWhiteSpace(rtInput.Text))
                {
                    SendMessage();
                    try
                    {
                        string selectedLevel = (cmbRoastLevel.SelectedIndex > 0 ? cmbRoastLevel.SelectedItem?.ToString() : "Savage") ?? "Savage";
                        string aiResponse = await _aiService.GenerateRoast(rtInput.Text, selectedLevel, _conversationHistory);

                        AddChatMessage(aiResponse, "assistant");
                        _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = aiResponse });
                    }
                    catch (Exception ex)
                    {
                        AddChatMessage($"Error: {ex.Message}", "system");
                    }
                    finally
                    {
                        rtInput.Enabled = true;
                        pbSendIcon.Enabled = true;
                    }
                }
            }
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(rtInput.Text) || rtInput.Text == "Type your message here...") return;

            SendMessage();

            try
            {
                string selectedLevel = (cmbRoastLevel.SelectedIndex > 0 ? cmbRoastLevel.SelectedItem?.ToString() : "Savage") ?? "Savage";
                string aiResponse = await _aiService.GenerateRoast(rtInput.Text, selectedLevel, _conversationHistory);

                AddChatMessage(aiResponse, "assistant");
                _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = aiResponse });
            }
            catch (Exception ex)
            {
                AddChatMessage($"Error: {ex.Message}", "system");
            }
            finally
            {
                rtInput.Enabled = true;
                pbSendIcon.Enabled = true;
            }
        }
    }
}
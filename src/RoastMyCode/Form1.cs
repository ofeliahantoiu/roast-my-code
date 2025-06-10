using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Diagnostics;
using RoastMyCode.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Speech.Synthesis;

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
        private Dictionary<string, string> _uploadedFiles = new Dictionary<string, string>();   
        private string[] _codeExtensions = Array.Empty<string>(); 
        private long _currentTotalSizeBytes = 0; 
        private readonly SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        private readonly Dictionary<string, string> _languageMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // File extensions
            [".cs"] = "C#",
            [".js"] = "JavaScript",
            [".ts"] = "TypeScript",
            [".py"] = "Python",
            [".java"] = "Java",
            [".cpp"] = "C++",
            [".c"] = "C",
            [".h"] = "C/C++ Header",
            [".hpp"] = "C++ Header",
            [".php"] = "PHP",
            [".rb"] = "Ruby",
            [".go"] = "Go",
            [".rs"] = "Rust",
            [".swift"] = "Swift",
            [".kt"] = "Kotlin",
            [".dart"] = "Dart",
            [".sh"] = "Shell Script",
            [".ps1"] = "PowerShell",
            [".bat"] = "Batch",
            [".cmd"] = "Command Script",
            [".html"] = "HTML",
            [".css"] = "CSS",
            [".xml"] = "XML",
            [".json"] = "JSON",
            [".yaml"] = "YAML",
            [".yml"] = "YAML",
            [".md"] = "Markdown",
            [".txt"] = "Text",
            
            ["dockerfile"] = "Dockerfile",
            [".dockerignore"] = "Docker Ignore",
            [".gitignore"] = "Git Ignore",
            ["makefile"] = "Makefile",
            ["readme"] = "Readme",
            ["license"] = "License"
        };
        private Panel chatAreaPanel = null!;
        private PictureBox pbThemeToggle = null!;
        private ComboBox cmbFontStyle = null!;
        private ComboBox cmbFontSize = null!;
        private ComboBox cmbRoastLevel = null!;
        private Panel titleLogoPanel = null!;
        private RichTextBox rtInput = null!;
        private PictureBox? pbCameraIcon;
        private PictureBox? pbMicIcon;
        private PictureBox? pbUploadIcon = null!;
        private PictureBox pbSendIcon = null!;
        private PictureBox pbGradientBackground = null!;
        private Panel inputPanel = null!;


        public Form1(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            try
            {
                if (configuration == null)
                    throw new ArgumentNullException(nameof(configuration));
                if (serviceProvider == null)
                    throw new ArgumentNullException(nameof(serviceProvider));

                _configuration = configuration;
                _aiService = new AIService(_configuration);
                _conversationHistory = new List<ChatMessage>();
                
                _fileUploadOptions = serviceProvider.GetService<FileUploadOptions>() ?? 
                    throw new InvalidOperationException("Failed to resolve FileUploadOptions from service provider");
                
                if (_fileUploadOptions.AllowedExtensions == null || _fileUploadOptions.AllowedExtensions.Length == 0)
                {
                    _fileUploadOptions.AllowedExtensions = _languageMap.Keys
                        .Where(ext => ext.StartsWith("."))
                        .ToArray();
                }
                
                if (_fileUploadOptions.MaxFileSizeMB <= 0)
                {
                    _fileUploadOptions.MaxFileSizeMB = 10; 
                }
                
                if (_fileUploadOptions.MaxTotalSizeMB <= 0)
                {
                    _fileUploadOptions.MaxTotalSizeMB = 50;
                }
                
                _codeExtensions = _languageMap.Keys.Where(k => k.StartsWith(".")).ToArray();

                _conversationHistory.Add(new ChatMessage {
                    Role = "assistant",
                    Content = "Glad you asked. Besides fixing your code and your life? Here's what I tolerate:\n\n• Reports - Like \"What's the last report we exported?\"\n• Your organization - \"How many people are using our software?\"\n• Features - \"How do I change the colors of my report?\""
                });

                InitializeComponent(); 
                InitializeModernUI();
                ApplyTheme();

                LoadConversationHistory();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error initializing Form1: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                MessageBox.Show(errorMessage, "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; 
            }
        }
        
        private string DetectLanguage(string fileName, string? content = null)
        {
            string fileNameLower = Path.GetFileName(fileName).ToLowerInvariant();
            if (_languageMap.ContainsKey(fileNameLower))
            {
                return _languageMap[fileNameLower];
            }
            
            foreach (var pattern in _languageMap.Keys.Where(k => k.EndsWith("/")))
            {
                if (fileName.Replace("\\", "/").Contains(pattern))
                {
                    return _languageMap[pattern];
                }
            }
            
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!string.IsNullOrEmpty(extension) && _languageMap.ContainsKey(extension))
            {
                return _languageMap[extension];
            }
            
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
                    try
                    {
                        _ = System.Text.Json.JsonDocument.Parse(content);
                        return "JSON";
                    }
                    catch { }
                }
            }
            
            return "Unknown";
        }

        private void InitializeModernUI()
        {
            this.Text = "Roast My Code";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.ForeColor = Color.White;
            this.MinimumSize = new Size(800, 600);

            this.Controls.Clear();

            pbGradientBackground = new PictureBox
            {
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height / 2),
                Location = new Point(0, this.ClientSize.Height / 2),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };

            LoadImageFromAssets(pbGradientBackground, "gradient.png");
            this.Controls.Add(pbGradientBackground);
            pbGradientBackground.SendToBack();

            chatAreaPanel = new Panel
            {
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 200 - 120),
                Location = new Point(0, 200),
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };
            this.Controls.Add(chatAreaPanel);   

            topPanel = new Panel
            {
                Size = new Size(this.ClientSize.Width, 200),
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };
            this.Controls.Add(topPanel);

            bottomPanel = new Panel
            {
                Size = new Size(this.ClientSize.Width, 120),
                Location = new Point(0, this.ClientSize.Height - 120),
                BackColor = Color.Transparent
            };
            this.Controls.Add(bottomPanel);

            this.Resize += (s, e) =>
            {
                pbGradientBackground.Size = new Size(this.ClientSize.Width, this.ClientSize.Height / 2);
                pbGradientBackground.Location = new Point(0, this.ClientSize.Height / 2);

                topPanel.Size = new Size(this.ClientSize.Width, 200);

                chatAreaPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 200 - 120);
                chatAreaPanel.Location = new Point(0, 200);

                bottomPanel.Size = new Size(this.ClientSize.Width, 120);
                bottomPanel.Location = new Point(0, this.ClientSize.Height - 120);
            };

            titleLogoPanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.None,
                AutoSize = true,
                Padding = new Padding(0, 30, 0, 0)
            };
            topPanel.Controls.Add(titleLogoPanel);

            pbLogo = new PictureBox
            {
                Size = new Size(32, 32),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 0, 5), 
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            titleLogoPanel.Controls.Add(pbLogo);

            lblTitle = new Label
            {
                Text = "Roast My Code",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Margin = new Padding(0)
            };
            titleLogoPanel.Controls.Add(lblTitle);

            pbLogo.Location = new Point(
                (titleLogoPanel.Width - pbLogo.Width) / 2,
                0
            );
            lblTitle.Location = new Point(
                (titleLogoPanel.Width - lblTitle.Width) / 2,
                pbLogo.Bottom + pbLogo.Margin.Bottom
            );

            int verticalOffset = 40; 
            titleLogoPanel.Location = new Point(
                 (topPanel.Width - titleLogoPanel.Width) / 2,
                 ((topPanel.Height - titleLogoPanel.Height) / 2) + verticalOffset
             );

            topPanel.SizeChanged += (s, e) => {
                 titleLogoPanel.PerformLayout(); 

                 titleLogoPanel.Location = new Point(
                     (topPanel.Width - titleLogoPanel.Width) / 2,
                     ((topPanel.Height - titleLogoPanel.Height) / 2) + verticalOffset
                 );
             };

            FlowLayoutPanel leftControlsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Padding = new Padding(10, 10, 0, 0), 
                BackColor = Color.Transparent
            };
            topPanel.Controls.Add(leftControlsPanel);

            pbThemeToggle = new PictureBox
            {
                Size = new Size(24, 24), 
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Margin = new Padding(0, 0, 5, 0)
            };
            pbThemeToggle.Click += (s, e) => ToggleTheme();
            leftControlsPanel.Controls.Add(pbThemeToggle);
            UpdateThemeIcon();

            cmbFontStyle = new ComboBox
            {
                Size = new Size(80, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.Gray,
                Margin = new Padding(0, 0, 5, 0)
            };
            cmbFontStyle.Items.AddRange(new string[] { "Style", "Segoe UI", "Arial", "Calibri", "Consolas", "Verdana" });
            cmbFontStyle.SelectedIndex = 0;
            cmbFontStyle.DrawItem += (sender, e) => {
                using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
                
                if (e.Index >= 0 && e.Font != null)
                {
                    string text = cmbFontStyle.Items[e.Index].ToString() ?? string.Empty;
                    Color textColor = e.Index == 0 ? Color.Gray : Color.White;
                    using (var brush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                    }
                }
            };

            cmbFontStyle.MeasureItem += (sender, e) => {
                e.ItemHeight = 24;
            };

            cmbFontStyle.DrawMode = DrawMode.OwnerDrawFixed;
            cmbFontStyle.DrawItem += (sender, e) => {
                using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(65, 65, 65)))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }

                if (e.Index >= 0 && e.Font != null)
                {
                    string text = cmbFontStyle.Items[e.Index].ToString() ?? string.Empty;
                    Color textColor = e.Index == 0 ? Color.Gray : Color.White;
                    using (var brush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                    }
                }
            };
            cmbFontStyle.SelectedIndexChanged += (s, e) => {
                if (cmbFontStyle.SelectedIndex == 0)
                    cmbFontStyle.ForeColor = Color.Gray;
                else
                    cmbFontStyle.ForeColor = Color.White;
                UpdateFont();
            };
            leftControlsPanel.Controls.Add(cmbFontStyle);

            cmbFontSize = new ComboBox
            {
                Size = new Size(60, 24), 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.Gray,
                Margin = new Padding(0, 0, 5, 0),
                DrawMode = DrawMode.OwnerDrawFixed 
            };
            cmbFontSize.Items.AddRange(new string[] { "Size", "8", "9", "10", "11", "12", "14", "16", "18", "20" });
            cmbFontSize.SelectedIndex = 0;
            cmbFontSize.DrawItem += (sender, e) => {
                using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
                
                if (e.Index >= 0 && e.Font != null)
                {
                    string text = cmbFontSize.Items[e.Index].ToString() ?? string.Empty;
                    Color textColor = e.Index == 0 ? Color.Gray : Color.White;
                    using (var brush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                    }
                }
            };

            cmbFontSize.MeasureItem += (sender, e) => {
                e.ItemHeight = 24; 
            };

            cmbFontSize.DrawMode = DrawMode.OwnerDrawFixed;
            cmbFontSize.DrawItem += (sender, e) => {
                using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48))) 
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(65, 65, 65)))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }

                if (e.Index >= 0 && e.Font != null)
                {
                    string text = cmbFontSize.Items[e.Index].ToString() ?? string.Empty;
                    Color textColor = e.Index == 0 ? Color.Gray : Color.White;
                    using (var brush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                    }
                }
            };
            cmbFontSize.SelectedIndexChanged += (s, e) => {
                if (cmbFontSize.SelectedIndex == 0)
                    cmbFontSize.ForeColor = Color.Gray;
                else
                    cmbFontSize.ForeColor = Color.White;
                UpdateFont();
            };
            leftControlsPanel.Controls.Add(cmbFontSize);

            cmbRoastLevel = new ComboBox
            {
                Size = new Size(70, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.Gray,
                Margin = new Padding(0, 0, 0, 0)
            };
            cmbRoastLevel.Items.AddRange(new string[] { "Level", "Light", "Savage", "Brutal" });
            cmbRoastLevel.SelectedIndex = 0;
            cmbRoastLevel.DrawItem += (sender, e) => {
                using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
                
                if (e.Index >= 0 && e.Font != null)
                {
                    string text = cmbRoastLevel.Items[e.Index].ToString() ?? string.Empty;
                    Color textColor = e.Index == 0 ? Color.Gray : Color.White;
                    using (var brush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                    }
                }
            };

            cmbRoastLevel.MeasureItem += (sender, e) => {
                e.ItemHeight = 24;
            };

            cmbRoastLevel.DrawMode = DrawMode.OwnerDrawFixed;
            cmbRoastLevel.DrawItem += (sender, e) => {
                using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(65, 65, 65)))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }

                if (e.Index >= 0 && e.Font != null)
                {
                    string text = cmbRoastLevel.Items[e.Index].ToString() ?? string.Empty;
                    Color textColor = e.Index == 0 ? Color.Gray : Color.White;
                    using (var brush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                    }
                }
            };
            cmbRoastLevel.SelectedIndexChanged += (s, e) => {
                if (cmbRoastLevel.SelectedIndex == 0)
                    cmbRoastLevel.ForeColor = Color.Gray;
                else
                    cmbRoastLevel.ForeColor = Color.White;
            };
            leftControlsPanel.Controls.Add(cmbRoastLevel);

            int inputPanelWidth = 800;
            int inputPanelHeight = 44;
            inputPanel = new Panel
            {
                Width = inputPanelWidth,
                Height = inputPanelHeight,
                BackColor = Color.FromArgb(70, 70, 70),
                Location = new Point((bottomPanel.Width - inputPanelWidth) / 2, (bottomPanel.Height - inputPanelHeight) / 2)
            };
            bottomPanel.Controls.Add(inputPanel);

            bottomPanel.Resize += (s, e) =>
            {
                inputPanel.Location = new Point((bottomPanel.Width - inputPanel.Width) / 2, (bottomPanel.Height - inputPanel.Height) / 2);
            };

            Panel sendButtonPanel = new Panel
            {
                Width = 40,
                Height = inputPanelHeight,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Location = new Point(inputPanelWidth - 40, 0)
            };

            pbSendIcon = new PictureBox
            {
                Size = new Size(32, 32),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(8, (inputPanelHeight - 32) / 2),
                Visible = true
            };
            LoadImageFromAssets(pbSendIcon, _isDarkMode ? "send.png" : "message.png");
            pbSendIcon.Click += BtnSend_Click;
            sendButtonPanel.Controls.Add(pbSendIcon);

            Panel leftIconsPanel = new Panel
            {
                Width = 100, 
                Height = inputPanelHeight,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0),
                Location = new Point(0, 0)
            };

            pbUploadIcon = new PictureBox
            {
                Size = new Size(26, 26),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(10, (inputPanelHeight - 26) / 2),
                Visible = true
            };
            LoadImageFromAssets(pbUploadIcon, _isDarkMode ? "uploadlight.png" : "uploaddark.png");
            pbUploadIcon.Click += PbCameraIcon_Click;

            pbCameraIcon = new PictureBox
            {
                Size = new Size(26, 26),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(40, (inputPanelHeight - 26) / 2),
                Visible = true
            };
            LoadImageFromAssets(pbCameraIcon, _isDarkMode ? "cameralight.png" : "cameradark.png");

            pbMicIcon = new PictureBox
            {
                Size = new Size(26, 26),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(70, (inputPanelHeight - 26) / 2),
                Visible = true
            };
            LoadImageFromAssets(pbMicIcon, _isDarkMode ? "microphonelight.png" : "microphonedark.png");
            pbMicIcon.Click += (s, e) => SpeakMessage();    

            leftIconsPanel.Controls.Add(pbUploadIcon);
            leftIconsPanel.Controls.Add(pbCameraIcon);
            leftIconsPanel.Controls.Add(pbMicIcon);
            leftIconsPanel.BringToFront();
            
            leftIconsPanel.BackColor = Color.FromArgb(50, 50, 50);

            rtInput = new RichTextBox
            {
                Width = inputPanelWidth - 120,
                Height = inputPanelHeight - 10,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                Padding = new Padding(10, 5, 10, 5),
                Location = new Point(110, 5),
                Text = "Type your message here..."
            };

            rtInput.GotFocus += (s, e) => {
                if (rtInput.Text == "Type your message here...")
                {
                    rtInput.Text = "";
                    rtInput.ForeColor = Color.White;
                }
            };

            rtInput.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(rtInput.Text))
                {
                    rtInput.Text = "Type your message here...";
                    rtInput.ForeColor = Color.FromArgb(150, 150, 150);
                }
            };

            inputPanel.Controls.Add(leftIconsPanel);
            inputPanel.Controls.Add(sendButtonPanel);
            inputPanel.Controls.Add(rtInput);
            leftIconsPanel.BringToFront();
            sendButtonPanel.BringToFront();

            rtInput.TextChanged += (s, e) =>
            {
                if (rtInput.Text.Length > 0 && rtInput.Text != "Type your message here...")
                {
                    rtInput.SelectionStart = rtInput.Text.Length;
                    rtInput.ScrollToCaret();
                }
            };

            chatAreaPanel.Resize += ChatAreaPanel_Resize;
            pbGradientBackground.SendToBack();
            chatAreaPanel.BringToFront();
            topPanel.BringToFront();
            bottomPanel.BringToFront();
            inputPanel.BringToFront();
        }

        private void ChatAreaPanel_Resize(object? sender, EventArgs e)
        {
            int panelWidth = chatAreaPanel.ClientSize.Width;
            if (panelWidth <= 0) return;

            int desiredBubbleWidth = (int)(panelWidth * 0.70) - chatAreaPanel.Padding.Horizontal - (chatAreaPanel.Controls.Count > 0 ? chatAreaPanel.Controls[0].Margin.Horizontal : 0); 
            if (desiredBubbleWidth < 1) desiredBubbleWidth = 1;

            foreach (Control control in chatAreaPanel.Controls)
            {
                if (control is ChatMessageBubble bubble)
                {
                    bubble.Width = desiredBubbleWidth;
                    bubble.MaximumSize = new Size(bubble.Width, 0);

                    if (bubble.Role == "user")
                    {
                        bubble.Left = panelWidth - chatAreaPanel.Padding.Right - bubble.Width;
                    }
                    else
                    {
                        bubble.Left = chatAreaPanel.Padding.Left;
                    }

                    bubble.PerformLayout();
                }
            }
            chatAreaPanel.Invalidate(true);
            chatAreaPanel.PerformLayout();
        }

        private void LoadConversationHistory()
        {
            chatAreaPanel.Controls.Clear();
            
            foreach (var message in _conversationHistory)
            {
                AddChatMessage(message.Content, message.Role);
            }

            if (chatAreaPanel.Controls.Count > 0)
            {
                 chatAreaPanel.ScrollControlIntoView(chatAreaPanel.Controls[chatAreaPanel.Controls.Count - 1]);
            }
        }

        private void LoadImageFromAssets(PictureBox pictureBox, string imageName)
        {
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation)!;
            string projectDirectory = Directory.GetParent(assemblyDirectory!)!.Parent!.Parent!.FullName; 
            string assetsPath = Path.Combine(projectDirectory, "assets", imageName);

            if (File.Exists(assetsPath))
            {
                try
                {
                    pictureBox.Image = Image.FromFile(assetsPath);
                     this.FormClosed += (s, e) => { if (pictureBox.Image != null) { pictureBox.Image.Dispose(); } };
                     System.Diagnostics.Debug.WriteLine($"Successfully loaded image: {assetsPath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image {imageName} from {assetsPath}: {ex.Message}");
                    MessageBox.Show($"Error loading image {imageName} from {assetsPath}: {ex.Message}", "Image Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Image file not found at: {assetsPath}");
                MessageBox.Show($"Image file not found at: {assetsPath}", "Image Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void SpeakMessage()
        {
            try
            {
                ChatMessageBubble? lastAIBubble = null;
                foreach (Control control in chatAreaPanel.Controls)
                {
                    if (control is ChatMessageBubble bubble && bubble.Role == "assistant")
                    {
                        lastAIBubble = bubble;
                    }
                }

                if (lastAIBubble != null)
                {
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Neutral);
                    _speechSynthesizer.Volume = 100;
                    _speechSynthesizer.Rate = 0;
                    _speechSynthesizer.SpeakAsync(lastAIBubble.MessageText);
                }
                else
                {
                    MessageBox.Show("No AI messages to read.", "No Messages", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error speaking message: {ex.Message}", "TTS Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ApplyTheme()
        {
            Color textColor = _isDarkMode ? Color.White : Color.Black;
            Color backColor = _isDarkMode ? Color.FromArgb(45, 45, 45) : Color.White;
            Color editorBackColor = _isDarkMode ? Color.FromArgb(50, 50, 50) : Color.White;
            Color buttonBackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);

            this.BackColor = backColor;
            this.ForeColor = textColor;

            foreach (Control control in this.Controls)
            {
                ApplyThemeToControl(control, _isDarkMode, textColor, backColor, editorBackColor, buttonBackColor);
            }

            UpdateLogoIcon();
        }

        private void UpdateLogoIcon()
        {
            LoadImageFromAssets(pbLogo, _isDarkMode ? "twinklewhite.png" : "twinkleblack.png");
            LoadImageFromAssets(pbLogo, _isDarkMode ? "twinklewhite.png" : "twinkleblack.png");
        }

        private void ApplyThemeToControl(Control control, bool isDarkMode, Color textColor, Color backColor, Color editorBackColor, Color buttonBackColor)
        {
            if (control is Label titleLabel && titleLabel.Text == "Roast My Code")
            {
                titleLabel.ForeColor = textColor;
                return;
            }

            if (control is ChatMessageBubble bubble)
            {
                if (bubble.Role == "user")
                {
                    bubble.BackColor = Color.FromArgb(70, 70, 70);
                    bubble.ForeColor = Color.White;
                }
                else
                {
                    bubble.ForeColor = isDarkMode ? Color.White : Color.Black;
                }
                bubble.Invalidate();
                return;
            }

            control.BackColor = backColor;
            control.ForeColor = textColor;

            if (control is ComboBox comboBox)
            {
                comboBox.BackColor = isDarkMode ? Color.FromArgb(45, 45, 48) : Color.FromArgb(240, 240, 240);
                comboBox.ForeColor = textColor;
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.Invalidate();
            }
            else if (control is TextBox textBox)
            {
                if (!string.IsNullOrWhiteSpace(rtInput.Text))
                {
                    textBox.ForeColor = textColor;
                }
                else
                {
                    textBox.BackColor = editorBackColor;
                    textBox.ForeColor = textColor;
                }
                textBox.Invalidate();
            }
            else if (control is RichTextBox richTextBox)
            {
                 richTextBox.BackColor = isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240); 
                 
                 if (richTextBox is RoundedRichTextBox rtInput)
                 {
                     rtInput.BorderColor = isDarkMode ? Color.FromArgb(100, 100, 100) : Color.FromArgb(180, 180, 180);
                     rtInput.BorderWidth = 1;
                     
                     if (rtInput.Text != "Type your message here...")
                     {
                         rtInput.ForeColor = isDarkMode ? Color.White : Color.Black;
                     }
                 }
                 else
                 {
                     richTextBox.ForeColor = textColor;
                 }
                 
                 richTextBox.Invalidate();
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.ForeColor = textColor;
                checkBox.Invalidate();
            }
            else if (control is Label label)
            {
                 if (label.Text != "Roast My Code")
                 {
                     label.ForeColor = textColor;
                     label.Invalidate();
                 }
            }
             else if (control is PictureBox pb)
            {
                pb.BackColor = backColor;
            }
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child, isDarkMode, textColor, backColor, editorBackColor, buttonBackColor);
            }
        }

        private void ApplyFontToControl(Control control, Font font)
        {
            if (control is TextBox || control is RichTextBox || control is Label ||
                control is Button || control is ComboBox || control is ChatMessageBubble)
            {
                control.Font = font;
                control.Invalidate();
            }

            foreach (Control child in control.Controls)
            {
                ApplyFontToControl(child, font);
            }
        }

        private void AdjustButtonSizes()
        {
            if (pbLogo != null && lblTitle != null)
            {
                lblTitle.Location = new Point(pbLogo.Right + lblTitle.Margin.Left, (pbLogo.Height - lblTitle.Height) / 2);
            }
        }

        private void UpdateFont()
        {
            if (cmbFontStyle.SelectedIndex > 0 || cmbFontSize.SelectedIndex > 0)
            {
                string fontFamily = cmbFontStyle.SelectedIndex > 0 ? 
                                    cmbFontStyle.SelectedItem.ToString()! : 
                                    _currentFont.FontFamily.Name;

                float fontSize = cmbFontSize.SelectedIndex > 0 ? 
                                 float.Parse(cmbFontSize.SelectedItem.ToString()!) : 
                                 _currentFont.Size;

                if (fontSize <= 0) fontSize = 8;

                _currentFont = new Font(fontFamily, fontSize);

                if (chatAreaPanel != null)
                {
                    ApplyFontToControl(chatAreaPanel, _currentFont);
                }
                if (rtInput != null)
                {
                    rtInput.Font = _currentFont;
                    rtInput.PerformLayout();
                }
            }
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(rtInput.Text) || rtInput.Text == "Type your message here...")
                return;

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

        private void SendMessage()
        {
            AddChatMessage(rtInput.Text, "user");
            _conversationHistory.Add(new ChatMessage { Content = rtInput.Text, Role = "user" });
            rtInput.Text = "Type your message here...";
            rtInput.ForeColor = Color.FromArgb(150, 150, 150);
            rtInput.SelectionStart = 0;

            if (chatAreaPanel.VerticalScroll.Visible)
                chatAreaPanel.ScrollControlIntoView(chatAreaPanel.Controls[chatAreaPanel.Controls.Count - 1]);
        }

        private void AddChatMessage(string message, string role)
        {
            ChatMessageBubble bubble = new ChatMessageBubble
            {
                MessageText = message,
                Role = role,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = _currentFont,
                Margin = new Padding(10, 5, 10, 5)
            };

            chatAreaPanel.Controls.Add(bubble);
            chatAreaPanel.Controls.SetChildIndex(bubble, chatAreaPanel.Controls.Count - 1); // Ensure order
            PositionChatBubbles();

            chatAreaPanel.ScrollControlIntoView(bubble);
        }

        private void ChatAreaPanel_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (e.Control is ChatMessageBubble bubble)
            {
                PositionChatBubbles();
                chatAreaPanel.ScrollControlIntoView(bubble);
            }
        }
            
        private void PositionChatBubbles()
        {
            int panelWidth = chatAreaPanel.ClientSize.Width;
            if (panelWidth <= 0) return;

            int availableWidth = panelWidth - chatAreaPanel.Padding.Horizontal;
            int desiredBubbleWidth = (int)(availableWidth * 0.70);
            if (desiredBubbleWidth < 100) desiredBubbleWidth = 100;

            int currentY = chatAreaPanel.Padding.Top;

            foreach (Control control in chatAreaPanel.Controls)
            {
                if (control is ChatMessageBubble bubble)
                {
                    bubble.Width = desiredBubbleWidth;
                    bubble.MaximumSize = new Size(bubble.Width, 0);

                    bubble.PerformLayout();

                    if (bubble.Role == "user")
                    {
                        bubble.Left = panelWidth - chatAreaPanel.Padding.Right - bubble.Width - 10;
                    }
                    else
                    {
                        bubble.Left = chatAreaPanel.Padding.Left + 10;
                    }

                    bubble.Top = currentY;

                    currentY += bubble.Height + bubble.Margin.Vertical;
                }
            }
            chatAreaPanel.AutoScrollMinSize = new Size(0, currentY + 50);
            chatAreaPanel.Invalidate(true);

            if (chatAreaPanel.Controls.Count > 0)
            {
                chatAreaPanel.ScrollControlIntoView(chatAreaPanel.Controls[chatAreaPanel.Controls.Count - 1]);
                chatAreaPanel.VerticalScroll.Value = chatAreaPanel.VerticalScroll.Maximum;
            }
        }

        private void ChatAreaPanel_Scroll(object? sender, ScrollEventArgs e)
        {
            if (chatAreaPanel.Controls.Count > 0)
            {
                chatAreaPanel.ScrollControlIntoView(chatAreaPanel.Controls[chatAreaPanel.Controls.Count - 1]);
            }
        }

        private void ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;
            ApplyTheme();
            UpdateThemeIcon();
            if (pbSendIcon != null) LoadImageFromAssets(pbSendIcon, _isDarkMode ? "send.png" : "message.png");
            if (pbCameraIcon != null) LoadImageFromAssets(pbCameraIcon, _isDarkMode ? "cameralight.png" : "cameradark.png");
            if (pbMicIcon != null) LoadImageFromAssets(pbMicIcon, _isDarkMode ? "microphonelight.png" : "microphonedark.png");
            if (pbUploadIcon != null) LoadImageFromAssets(pbUploadIcon, _isDarkMode ? "uploadlight.png" : "uploaddark.png");
        }

        private void UpdateThemeIcon()
        {
            if (pbThemeToggle != null) LoadImageFromAssets(pbThemeToggle, _isDarkMode ? "brightness.png" : "moon.png");
        }

        private void ShowDownloadButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowDownloadButton));
                return;
            }

            var existingButton = this.Controls.OfType<Button>().FirstOrDefault(b => b.Name == "btnDownloadBundle");
            existingButton?.Dispose();

            var downloadButton = new Button
            {
                Name = "btnDownloadBundle",
                Text = "Download Code Bundle",
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Padding = new Padding(15, 5, 15, 5),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.ClientSize.Width - 220, 10)
            };

            downloadButton.Click += (s, e) => DownloadCodeBundle();
            
            downloadButton.MouseEnter += (s, e) => {
                downloadButton.BackColor = Color.FromArgb(0, 100, 180);
            };
            downloadButton.MouseLeave += (s, e) => {
                downloadButton.BackColor = Color.FromArgb(0, 120, 215);
            };

            this.Controls.Add(downloadButton);
            downloadButton.BringToFront();
        }
        
        private void HideDownloadButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(HideDownloadButton));
                return;
            }

            var downloadButton = this.Controls.OfType<Button>().FirstOrDefault(b => b.Name == "btnDownloadBundle");
            if (downloadButton != null)
            {
                this.Controls.Remove(downloadButton);
                downloadButton.Dispose();
            }
        }

        private void ResetUploads()
        {
            _uploadedFiles.Clear();
            _currentTotalSizeBytes = 0;
            HideDownloadButton();
        }
        
        private void DownloadCodeBundle()
        {
            if (_uploadedFiles.Count == 0)
            {
                AddChatMessage("No files available to download.", "system");
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "ZIP Archive|*.zip";
                saveFileDialog.Title = "Save Code Bundle";
                saveFileDialog.FileName = $"code_bundle_{DateTime.Now:yyyyMMddHHmmss}.zip";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        Directory.CreateDirectory(tempDir);

                        try
                        {
                            foreach (var file in _uploadedFiles)
                            {
                                string filePath = Path.Combine(tempDir, file.Key);
                                File.WriteAllText(filePath, file.Value);
                            }

                            if (File.Exists(saveFileDialog.FileName))
                            {
                                File.Delete(saveFileDialog.FileName);
                            }
                            
                            ZipFile.CreateFromDirectory(tempDir, saveFileDialog.FileName);
                            
                            AddChatMessage($"Code bundle saved successfully: {Path.GetFileName(saveFileDialog.FileName)}", "system");
                        }
                        finally
                        {
                            try { Directory.Delete(tempDir, true); }
                            catch { /* Ignore cleanup errors */ }
                        }
                    }
                    catch (Exception ex)
                    {
                        AddChatMessage($"Error creating code bundle: {ex.Message}", "system");
                    }
                }
            }
        }

        private (string content, string error) ProcessZipFile(string zipPath)
        {
            string tempDir = string.Empty;
            string result = string.Empty;
            var processedFiles = new List<string>();
            var fileLanguages = new Dictionary<string, string>();
            var zipFileInfo = new FileInfo(zipPath);
            
            if (zipFileInfo.Length > _fileUploadOptions.MaxFileSizeBytes)
            {
                return (string.Empty, $"ZIP file '{Path.GetFileName(zipPath)}' exceeds maximum size of {_fileUploadOptions.MaxFileSizeMB}MB");
            }

            long remainingSpace = _fileUploadOptions.MaxTotalSizeBytes - _currentTotalSizeBytes;
            if (zipFileInfo.Length > remainingSpace)
            {
                return (string.Empty, $"Extracting this ZIP would exceed the total size limit of {_fileUploadOptions.MaxTotalSizeMB}MB");
            }

            try
            {
                tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempDir);
                   
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.FullName.EndsWith("/")) 
                        {
                            string fileName = Path.GetFileName(entry.FullName);
                            if (string.IsNullOrEmpty(fileName)) continue;
                            
                            string ext = Path.GetExtension(entry.Name).ToLowerInvariant();
                            if (!_fileUploadOptions.AllowedExtensions.Contains(ext) && !_languageMap.ContainsKey(fileName.ToLowerInvariant()))
                            {
                                return (string.Empty, $"ZIP contains disallowed file type: {entry.Name}");
                            }
                            
                            if (entry.Length > _fileUploadOptions.MaxFileSizeBytes)
                            {
                                return (string.Empty, $"ZIP contains file '{entry.Name}' that exceeds maximum size of {_fileUploadOptions.MaxFileSizeMB}MB");
                            }
                            
                            if (entry.Length > remainingSpace)
                            {
                                return (string.Empty, $"Extracting this ZIP would exceed the total size limit of {_fileUploadOptions.MaxTotalSizeMB}MB");
                            }
                            
                            remainingSpace -= entry.Length;
                        }
                    }
                    
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("/")) continue; 
                        
                        string entryPath = Path.Combine(tempDir, entry.FullName);
                        string? directoryPath = Path.GetDirectoryName(entryPath);
                        
                        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        
                        entry.ExtractToFile(entryPath, true);
                    }
                }

                var codeFiles = Directory.GetFiles(tempDir, "*.*", SearchOption.AllDirectories)
                    .Where(f => {
                        string ext = Path.GetExtension(f).ToLowerInvariant();
                        string fileName = Path.GetFileName(f).ToLowerInvariant();
                        return _fileUploadOptions.AllowedExtensions.Contains(ext) || _languageMap.ContainsKey(fileName);
                    })
                    .OrderBy(f => f)
                    .ToList();

                if (codeFiles.Count == 0)
                {
                    return ($"No supported code files found in {Path.GetFileName(zipPath)}", string.Empty);
                }

                foreach (string file in codeFiles)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        string relativePath = file.Substring(tempDir.Length).TrimStart(Path.DirectorySeparatorChar);
                        string language = DetectLanguage(relativePath, content);
                        fileLanguages[relativePath] = language;
                        
                        _uploadedFiles[relativePath] = content;
                        _currentTotalSizeBytes += new FileInfo(file).Length;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing {file}: {ex.Message}");
                    }
                }

                var possibleMains = codeFiles.Where(f => 
                    f.IndexOf("program", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.IndexOf("main", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.IndexOf("app", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.IndexOf("index", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                var filesToProcess = possibleMains.Concat(codeFiles.Except(possibleMains));

                foreach (string file in filesToProcess)
                {
                    try
                    {
                        string relativePath = file.Substring(tempDir.Length).TrimStart(Path.DirectorySeparatorChar);
                        if (fileLanguages.TryGetValue(relativePath, out string? language) && 
                            _uploadedFiles.TryGetValue(relativePath, out string? content))
                        {
                            result += $"=== {relativePath} ({language}) ===\n{content}\n\n";
                            processedFiles.Add(relativePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        result += $"Error reading {Path.GetFileName(file)}: {ex.Message}\n\n";
                    }
                }

                
                if (processedFiles.Count == 0)
                {
                    return (string.Empty, "No valid code files could be extracted from the ZIP");
                }
                
                return (result, string.Empty);
            }
            catch (Exception ex)
            {
                foreach (var file in processedFiles)
                {
                    _uploadedFiles.Remove(file);
                }
                _currentTotalSizeBytes -= processedFiles.Sum(f => new FileInfo(Path.Combine(tempDir, f)).Length);
                
                return (string.Empty, $"Error processing ZIP file: {ex.Message}");
            }
            finally
            {
                try 
                { 
                    if (!string.IsNullOrEmpty(tempDir) && Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cleaning up temp directory: {ex.Message}");
                }
            }
        }

        private bool IsFileTypeAllowed(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _fileUploadOptions.AllowedExtensions.Contains(extension) || 
                   _languageMap.ContainsKey(Path.GetFileName(fileName).ToLowerInvariant());
        }

        private bool IsFileSizeWithinLimit(string filePath, out string error)
        {
            error = string.Empty;
            var fileInfo = new FileInfo(filePath);
            
            if (fileInfo.Length > _fileUploadOptions.MaxFileSizeBytes)
            {
                error = $"File '{Path.GetFileName(filePath)}' exceeds maximum size of {_fileUploadOptions.MaxFileSizeMB}MB";
                return false;
            }

            if (_currentTotalSizeBytes + fileInfo.Length > _fileUploadOptions.MaxTotalSizeBytes)
            {
                error = $"Adding this file would exceed the total size limit of {_fileUploadOptions.MaxTotalSizeMB}MB";
                return false;
            }

            return true;
        }

        private bool ValidateFiles(IEnumerable<string> filePaths, out List<string> validFiles, out List<string> errors)
        {
            validFiles = new List<string>();
            errors = new List<string>();
            long totalSize = 0;

            foreach (var filePath in filePaths)
            {
                try
                {
                    if (!IsFileTypeAllowed(filePath))
                    {
                        errors.Add($"File type not allowed: {Path.GetFileName(filePath)}");
                        continue;
                    }

                    if (!IsFileSizeWithinLimit(filePath, out string sizeError))
                    {
                        errors.Add(sizeError);
                        continue;
                    }

                    var fileInfo = new FileInfo(filePath);
                    totalSize += fileInfo.Length;
                    validFiles.Add(filePath);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            if (errors.Count == 0)
            {
                _currentTotalSizeBytes += totalSize;
            }

            return errors.Count == 0;
        }

        private async void PbCameraIcon_Click(object? sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = true;
                    string filter = "Supported Files|*" + string.Join(";*", _fileUploadOptions.AllowedExtensions) + 
                                 "|ZIP Archives|*.zip|All Files|*.*";
                    openFileDialog.Filter = filter;
                    openFileDialog.Title = $"Select Files to Upload (Max {_fileUploadOptions.MaxFileSizeMB}MB per file, {_fileUploadOptions.MaxTotalSizeMB}MB total)";
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        List<string> fileContents = new List<string>();
                        var filesToProcess = new List<string>(openFileDialog.FileNames);
                        
                        if (!ValidateFiles(filesToProcess, out var validFiles, out var validationErrors))
                        {
                            foreach (var error in validationErrors.Take(5))
                            {
                                AddChatMessage(error, "system");
                            }
                            
                            if (validationErrors.Count > 5)
                            {
                                AddChatMessage($"... and {validationErrors.Count - 5} more files had issues", "system");
                            }
                            
                            if (validFiles.Count == 0)
                            {
                                return;
                            }
                            
                            filesToProcess = validFiles;
                        }
                        else
                        {
                            ResetUploads();
                        }
                        
                        foreach (string fileName in filesToProcess)
                        {
                            try 
                            {
                                if (Path.GetExtension(fileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                                {
                                    var (content, error) = ProcessZipFile(fileName);
                                    if (!string.IsNullOrEmpty(error))
                                    {
                                        fileContents.Add($"Error processing {Path.GetFileName(fileName)}: {error}");
                                    }
                                    else if (!string.IsNullOrEmpty(content))
                                    {
                                        fileContents.Add($"=== Contents of {Path.GetFileName(fileName)} ===\n{content}");
                                    }
                                }
                                else
                                {
                                    string content = File.ReadAllText(fileName);
                                    string displayName = Path.GetFileName(fileName);
                                    string language = DetectLanguage(fileName, content);
                                    
                                    if (!_uploadedFiles.ContainsKey(displayName))
                                    {
                                        _uploadedFiles[displayName] = content;
                                        fileContents.Add($"=== {displayName} ({language}) ===\n{content}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                fileContents.Add($"Error processing {Path.GetFileName(fileName)}: {ex.Message}");
                            }
                        }

                        if (fileContents.Count > 0)
                        {
                            string combinedContent = string.Join("\n\n", fileContents);
                            AddChatMessage(combinedContent, "user");
                            _conversationHistory.Add(new ChatMessage { Content = combinedContent, Role = "user" });
                            
                            if (_uploadedFiles.Count > 0)
                            {
                                ShowDownloadButton();
                                
                                // Trigger AI response
                                string aiResponse = await _aiService.ProcessFiles(_uploadedFiles, cmbRoastLevel.SelectedItem?.ToString() ?? "light");
                                AddChatMessage(aiResponse, "assistant");
                                _conversationHistory.Add(new ChatMessage { Content = aiResponse, Role = "assistant" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddChatMessage($"Error during file upload: {ex.Message}", "system");
            }
        }

    }   
    public class RoundedRichTextBox : RichTextBox
    {
        private int cornerRadius = 20;
        private Color borderColor = Color.Transparent;
        private int borderWidth = 0;

        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        public int BorderWidth
        {
            get => borderWidth;
            set
            {
                borderWidth = value;
                Invalidate();
            }
        }

        public RoundedRichTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                int radius = CornerRadius;
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseAllFigures();

                this.Region = new Region(path);

                if (borderWidth > 0)
                {
                    using (Pen pen = new Pen(BorderColor, BorderWidth))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        public class RoundedPanel : Panel
        {
            public int CornerRadius { get; set; } = 18;
            public Color BorderColor { get; set; } = Color.Gray;
            public int BorderWidth { get; set; } = 1;

            public RoundedPanel()
            {
                this.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
                this.BackColor = Color.White;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = GetRoundedRect(rect, CornerRadius))
                using (var brush = new SolidBrush(this.BackColor))
                using (var pen = new Pen(BorderColor, BorderWidth))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            }

            private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
            {
                var path = new GraphicsPath();
                int d = radius * 2;
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                return path;
            }
        }
    }
}   

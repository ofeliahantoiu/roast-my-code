using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using RoastMyCode.Services;
using Microsoft.Extensions.Configuration;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private readonly AIService _aiService;
        private List<ChatMessage> _conversationHistory;
        private bool _isDarkMode = true;
        private Font _currentFont = new Font("Segoe UI", 10);
        private Panel chatAreaPanel = null!;
        private PictureBox pbThemeToggle = null!;
        private ComboBox cmbFontStyle = null!;
        private ComboBox cmbFontSize = null!;
        private ComboBox cmbRoastLevel = null!;
        private Panel titleLogoPanel = null!;
        private RichTextBox rtInput = null!;
        private PictureBox pbCameraIcon = null!;
        private PictureBox pbMicIcon = null!;
        private PictureBox pbSendIcon = null!;
        private PictureBox pbGradientBackground = null!;

        public Form1(IConfiguration configuration)
        {
            InitializeComponent();
            _aiService = new AIService(configuration);
            _conversationHistory = new List<ChatMessage>();

            _conversationHistory.Add(new ChatMessage {
                Role = "assistant",
                Content = "Glad you asked. Besides fixing your code and your life? Here's what I tolerate:\n\n• Reports - Like \"What's the last report we exported?\"\n• Your organization - \"How many people are using our software?\"\n• Features - \"How do I change the colors of my report?\""
            });

            InitializeModernUI();
            ApplyTheme();

            LoadConversationHistory();
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

            // Add gradient background
            pbGradientBackground = new PictureBox
            {
                Dock = DockStyle.Bottom,
                Height = this.ClientSize.Height / 2,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };
            LoadImageFromAssets(pbGradientBackground, "gradient.png");
            this.Controls.Add(pbGradientBackground);
            pbGradientBackground.SendToBack();

            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                BackColor = Color.FromArgb(45, 45, 45),
                Padding = new Padding(10, 20, 10, 10)
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
                Font = new Font("Segoe UI", 14, FontStyle.Regular), // Changed font size to 14
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
                using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48))) // Dark background
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

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(55, 55, 55),
                Padding = new Padding(20)
            };

            chatAreaPanel = new Panel
            {
                AutoScroll = true,
                BackColor = Color.FromArgb(45, 45, 45),
                Padding = new Padding(10),
                AutoScrollMinSize = new Size(0, 0)
            };
            chatAreaPanel.Scroll += ChatAreaPanel_Scroll;

            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(chatAreaPanel); 

            topPanel.BringToFront();

            PositionChatAreaPanel();

            this.Resize += Form1_Resize;

            int inputPanelWidth = 800;
            int inputPanelHeight = 44;
            Panel inputPanel = new Panel
            {
                Width = inputPanelWidth,
                Height = inputPanelHeight,
                BackColor = Color.FromArgb(70, 70, 70),
                Dock = DockStyle.None,
                Margin = new Padding(0)
            };
            bottomPanel.Controls.Add(inputPanel);

            inputPanel.Location = new Point(
                (bottomPanel.ClientSize.Width - inputPanelWidth) / 2,
                (bottomPanel.ClientSize.Height - inputPanelHeight) / 2
            );

            this.Resize += (s, e) => {
                inputPanel.Location = new Point(
                    (bottomPanel.ClientSize.Width - inputPanelWidth) / 2,
                    (bottomPanel.ClientSize.Height - inputPanelHeight) / 2
                );
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
                Width = 60, 
                Height = inputPanelHeight,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0),
                Location = new Point(0, 0)
            };

            pbCameraIcon = new PictureBox
            {
                Size = new Size(32, 32),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(0, (inputPanelHeight - 32) / 2),
                Visible = true
            };
            LoadImageFromAssets(pbCameraIcon, _isDarkMode ? "cameralight.png" : "cameradark.png");
            pbCameraIcon.Click += PbCameraIcon_Click;

            pbMicIcon = new PictureBox
            {
                Size = new Size(26, 26),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(35, (inputPanelHeight - 26) / 2),
                Visible = true
            };
            LoadImageFromAssets(pbMicIcon, _isDarkMode ? "microphonelight.png" : "microphonedark.png");

            leftIconsPanel.Controls.AddRange(new Control[] { pbCameraIcon, pbMicIcon });

            rtInput = new RichTextBox
            {
                Width = inputPanelWidth - 100,
                Height = inputPanelHeight - 10,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                Padding = new Padding(10, 5, 10, 5),
                Location = new Point(60, 5),
                Text = "Type your message here..."
            };

            // Add focus handlers
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

            rtInput.TextChanged += (s, e) => {
                if (rtInput.Text.Length > 0 && rtInput.Text != "Type your message here...")
                {
                    rtInput.SelectionStart = rtInput.Text.Length;
                    rtInput.ScrollToCaret();
                }
            };

            chatFlowPanel.Resize += ChatFlowPanel_Resize;
        }
        private void ChatFlowPanel_Resize(object? sender, EventArgs e)
        {
            int panelWidth = chatFlowPanel.ClientSize.Width;
            if (panelWidth <= 0) return;

            int desiredBubbleWidth = (int)(panelWidth * 0.70) - chatFlowPanel.Padding.Horizontal - (chatFlowPanel.Controls.Count > 0 ? chatFlowPanel.Controls[0].Margin.Horizontal : 0); 
            if (desiredBubbleWidth < 1) desiredBubbleWidth = 1;

            foreach (Control control in chatFlowPanel.Controls)
            {
                if (control is ChatMessageBubble bubble)
                {
                    bubble.Width = desiredBubbleWidth;
                    bubble.MaximumSize = new Size(bubble.Width, 0);

                    if (bubble.Role == "user")
                    {
                        bubble.Left = panelWidth - chatFlowPanel.Padding.Right - bubble.Width;
                    }
                    else
                    {
                        bubble.Left = chatFlowPanel.Padding.Left;
                    }

                    bubble.PerformLayout();
                }
            }
            chatFlowPanel.Invalidate(true);
            chatFlowPanel.PerformLayout();
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
            PositionChatBubbles();

            if (chatAreaPanel.VerticalScroll.Visible)
            {
                chatAreaPanel.ScrollControlIntoView(bubble);
            }
        }

        private void ChatFlowPanel_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (e.Control is ChatMessageBubble bubble)
            {
                bubble.PerformLayout();
            }
        }

        private void ChatAreaPanel_Resize(object? sender, EventArgs e)
        {
            PositionChatBubbles();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (pbGradientBackground != null)
            {
                pbGradientBackground.Height = this.ClientSize.Height / 2;
            }
            PositionChatAreaPanel();
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
            LoadImageFromAssets(pbSendIcon, _isDarkMode ? "send.png" : "message.png");
            LoadImageFromAssets(pbCameraIcon, _isDarkMode ? "cameralight.png" : "cameradark.png");
            LoadImageFromAssets(pbMicIcon, _isDarkMode ? "microphonelight.png" : "microphonedark.png");
        }

        private void UpdateThemeIcon()
        {
            LoadImageFromAssets(pbThemeToggle, _isDarkMode ? "brightness.png" : "moon.png");
        }

        private void PbCameraIcon_Click(object? sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = true;
                    openFileDialog.Filter = "Code Files|*.cs;*.js;*.ts;*.py;*.java;*.cpp;*.c;*.h;*.hpp;*.php;*.rb;*.go;*.rs;*.swift;*.kt;*.dart;*.sh;*.ps1;*.bat;*.cmd;*.html;*.css;*.xml;*.json;*.yaml;*.yml;*.md;*.txt|All Files|*.*";
                    openFileDialog.Title = "Select Code Files to Upload";
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        List<string> fileContents = new List<string>();
                        foreach (string fileName in openFileDialog.FileNames)
                        {
                            try 
                            {
                                string content = File.ReadAllText(fileName);
                                string displayName = Path.GetFileName(fileName);
                                fileContents.Add($"=== {displayName} ===\n{content}");
                            }
                            catch (Exception ex)
                            {
                                fileContents.Add($"Error reading {Path.GetFileName(fileName)}: {ex.Message}");
                            }
                        }

                        if (fileContents.Count > 0)
                        {
                            string combinedContent = string.Join("\n\n", fileContents);
                            AddChatMessage(combinedContent, "user");
                            _conversationHistory.Add(new ChatMessage { Content = combinedContent, Role = "user" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddChatMessage($"Error during file upload: {ex.Message}", "system");
            }
        }

        private void PositionChatAreaPanel()
        {
            if (topPanel != null && bottomPanel != null && chatAreaPanel != null && titleLogoPanel != null)
            {
                int x = 0;
                int y = topPanel.Top + titleLogoPanel.Top + titleLogoPanel.Height + 30;
                int width = this.ClientSize.Width;
                int height = bottomPanel.Top - y;

                if (height < 0) height = 0;

                chatAreaPanel.Location = new Point(x, y);
                chatAreaPanel.Size = new Size(width, height);

                PositionChatBubbles();
            }
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
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
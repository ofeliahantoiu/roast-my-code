using System;
using System.Drawing;
using System.Windows.Forms;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private void InitializeModernUI()
        {
            this.Text = "Roast My Code";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.ForeColor = Color.White;
            this.MinimumSize = new Size(800, 600);

            this.Controls.Clear();
            
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

            // Animation dropdown
            cmbAnimation = new ComboBox
            {
                Size = new Size(90, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.Gray,
                Margin = new Padding(5, 0, 0, 0)
            };
            cmbAnimation.Items.AddRange(new string[] { "Animation", "On", "Off" });
            cmbAnimation.SelectedIndex = 0;
            cmbAnimation.DrawItem += (sender, e) => {
                using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
                
                if (e.Index >= 0 && e.Font != null)
                {
                    string text = cmbAnimation.Items[e.Index].ToString() ?? string.Empty;
                    Color textColor = e.Index == 0 ? Color.Gray : Color.White;
                    using (var brush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                    }
                }
            };

            cmbAnimation.MeasureItem += (sender, e) => {
                e.ItemHeight = 24;
            };

            cmbAnimation.DrawMode = DrawMode.OwnerDrawFixed;
            cmbAnimation.DrawItem += (sender, e) => {
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
                    string text = cmbAnimation.Items[e.Index].ToString() ?? string.Empty;
                    Color textColor = e.Index == 0 ? Color.Gray : Color.White;
                    using (var brush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                    }
                }
            };
            cmbAnimation.SelectedIndexChanged += (s, e) => {
                if (cmbAnimation.SelectedIndex == 0)
                {
                    cmbAnimation.ForeColor = Color.Gray;
                    Services.AnimationService.Instance.AnimationsEnabled = true; // Default to enabled
                }
                else
                {
                    cmbAnimation.ForeColor = Color.White;
                    // Enable animations if "On" is selected (index 1), disable if "Off" is selected (index 2)
                    Services.AnimationService.Instance.AnimationsEnabled = cmbAnimation.SelectedIndex == 1;
                }
            };
            leftControlsPanel.Controls.Add(cmbAnimation);

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
            rtInput.KeyDown += async (s, e) =>
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

            // Add text counter display below the input panel
            textCounter = new TextCounterDisplay
            {
                Size = new Size(120, 20),
                Location = new Point(leftIconsPanel.Width + 5, rtInput.Bottom + 5),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                BackColor = Color.Transparent,
                IsDarkMode = _isDarkMode
            };
            
            // Connect the counter to the rtInput for live updates
            textCounter.ConnectToTextBox(rtInput);
            bottomPanel.Controls.Add(textCounter);

            rtInput.TextChanged += (s, e) =>
            {
                if (rtInput.Text.Length > 0 && rtInput.Text != "Type your message here...")
                {
                    rtInput.SelectionStart = rtInput.Text.Length;
                    rtInput.ScrollToCaret();
                }
            };

            chatAreaPanel.Resize += ChatAreaPanel_Resize;
            chatAreaPanel.BringToFront();
            topPanel.BringToFront();
            bottomPanel.BringToFront();
            inputPanel.BringToFront();
        }
    }
}
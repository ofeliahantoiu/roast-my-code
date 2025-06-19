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
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 250 - 120),
                Location = new Point(0, 250),
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(chatAreaPanel);   

            topPanel = new Panel
            {
                Size = new Size(this.ClientSize.Width, 250),
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };
            this.Controls.Add(topPanel);

            btnDownloadConversation = new Button
            {
                Text = "Download Conversation",
                Location = new Point(400, 10),
                Size = new Size(180, 30),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            btnDownloadConversation.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            btnDownloadConversation.Click += BtnDownloadConversation_Click;

            topPanel.Controls.Add(btnDownloadConversation);

            bottomPanel = new Panel
            {
                Size = new Size(this.ClientSize.Width, 120),
                Location = new Point(0, this.ClientSize.Height - 120),
                BackColor = Color.Transparent
            };
            this.Controls.Add(bottomPanel);

            pbGradientBackground = new PictureBox
            {
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height / 2),
                Location = new Point(0, this.ClientSize.Height / 2),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            this.Controls.Add(pbGradientBackground);

            this.Resize += (s, e) =>
            {
                pbGradientBackground.Size = new Size(this.ClientSize.Width, this.ClientSize.Height / 2);
                pbGradientBackground.Location = new Point(0, this.ClientSize.Height / 2);

                topPanel.Size = new Size(this.ClientSize.Width, 250);

                chatAreaPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 250 - 120);
                chatAreaPanel.Location = new Point(0, 250);

                bottomPanel.Size = new Size(this.ClientSize.Width, 120);
                bottomPanel.Location = new Point(0, this.ClientSize.Height - 120);
            };

            titleLogoPanel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                MinimumSize = new Size(200, 0)
            };
            topPanel.Controls.Add(titleLogoPanel);

            pbLogo = new PictureBox
            {
                Size = new Size(48, 48),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Margin = new Padding(0, 0, 0, 15)
            };
            titleLogoPanel.Controls.Add(pbLogo);

            lblTitle = new Label
            {
                Text = "Roast My Code",
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                MinimumSize = new Size(180, 0)
            };
            titleLogoPanel.Controls.Add(lblTitle);

            titleLogoPanel.PerformLayout();

            pbLogo.Location = new Point((titleLogoPanel.Width - pbLogo.Width) / 2, 0);
            lblTitle.Location = new Point((titleLogoPanel.Width - lblTitle.Width) / 2, pbLogo.Bottom + pbLogo.Margin.Bottom);

            titleLogoPanel.Location = new Point(
                (topPanel.Width - titleLogoPanel.PreferredSize.Width) / 2,
                140
            );

            topPanel.Resize += (s, e) =>
            {
                titleLogoPanel.PerformLayout();
                pbLogo.Location = new Point((titleLogoPanel.Width - pbLogo.Width) / 2, 0);
                lblTitle.Location = new Point((titleLogoPanel.Width - lblTitle.Width) / 2, pbLogo.Bottom + pbLogo.Margin.Bottom);
                titleLogoPanel.Location = new Point(
                    (topPanel.Width - titleLogoPanel.PreferredSize.Width) / 2,
                    140
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

            cmbVoiceType = new ComboBox
            {
                Size = new Size(80, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.Gray,
                Margin = new Padding(5, 0, 0, 0),
                DrawMode = DrawMode.OwnerDrawFixed
            };

            cmbVoiceType.DrawItem += (sender, e) => {
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
                    string text = cmbVoiceType.Items[e.Index].ToString() ?? string.Empty;
                    Color textColor = e.Index == 0 ? Color.Gray : Color.White;
                    using (var brush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
                    }
                }
            };

            cmbVoiceType.MeasureItem += (sender, e) => {
                e.ItemHeight = 24;
            };

            cmbVoiceType.SelectedIndexChanged += (s, e) => {
                if (cmbVoiceType.SelectedIndex == 0)
                    cmbVoiceType.ForeColor = Color.Gray;
                else
                    cmbVoiceType.ForeColor = Color.White;
            };

            leftControlsPanel.Controls.Add(cmbVoiceType);

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
            pbUploadIcon.Click += PbUploadIcon_Click;
            
            ToolTip uploadToolTip = new ToolTip
            {
                AutoPopDelay = 3000,
                InitialDelay = 200,
                ReshowDelay = 200,
                ShowAlways = true,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            uploadToolTip.SetToolTip(pbUploadIcon, "Upload code. Regret instantly.");
            this.FormClosed += (s, e) => uploadToolTip.Dispose();
            this.components?.Add(uploadToolTip);

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
            pbCameraIcon.Click += PbCameraIcon_Click;
            
            ToolTip cameraToolTip = new ToolTip
            {
                AutoPopDelay = 3000,
                InitialDelay = 200,
                ReshowDelay = 200,
                ShowAlways = true,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            cameraToolTip.SetToolTip(pbCameraIcon, "Snap your reaction. Cry later.");
            
            this.FormClosed += (s, e) => cameraToolTip.Dispose();
            this.components?.Add(cameraToolTip);

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
            LoadImageFromAssets(pbMicIcon, _isDarkMode ? "volumeWhite.png" : "volume.png");
            pbMicIcon.Click += (s, e) => SpeakMessage();

            leftIconsPanel.Controls.Add(pbUploadIcon);
            leftIconsPanel.Controls.Add(pbCameraIcon);
            leftIconsPanel.Controls.Add(pbMicIcon);
            leftIconsPanel.BringToFront();

            leftIconsPanel.BackColor = Color.FromArgb(50, 50, 50);

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(pbMicIcon, "Click to hear the last AI response with sound effects.");

            this.FormClosed += (s, e) => toolTip.Dispose();
            this.components?.Add(toolTip);

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
                    await HandleSendMessage();
                }
            };

            rtInput.GotFocus += (s, e) => {
                if (rtInput.Text == "Type your message here...")
                {
                    rtInput.Text = "";
                    rtInput.ForeColor = _isDarkMode ? Color.White : Color.Black;
                }
            };

            rtInput.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(rtInput.Text))
                {
                    rtInput.Text = "Type your message here...";
                    rtInput.ForeColor = _isDarkMode ? Color.FromArgb(150, 150, 150) : Color.FromArgb(100, 100, 100);
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
            chatAreaPanel.BringToFront();
            topPanel.BringToFront();
            bottomPanel.BringToFront();
            inputPanel.BringToFront();
        }
    }
}
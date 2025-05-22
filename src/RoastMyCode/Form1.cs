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
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.MinimumSize = new Size(800, 600);

            this.Controls.Clear();

            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10)
            };

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(50, 50, 50),
                Padding = new Padding(10)
            };

            chatAreaPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10),
                AutoScrollMinSize = new Size(0, 0)
            };
            chatAreaPanel.Scroll += ChatAreaPanel_Scroll;

            this.Controls.Add(chatAreaPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);

            Panel headerContentPanel = new Panel
            {
                Anchor = AnchorStyles.None,
                AutoSize = true,
                Padding = new Padding(0)
            };
            topPanel.Controls.Add(headerContentPanel);

            pbLogo = new PictureBox
            {
                Size = new Size(40, 40),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            LoadImageFromAssets(pbLogo, "twinkle.png");

            lblTitle = new Label
            {
                Text = "Roast My Code",
                Font = new Font("Segoe UI", 24, FontStyle.Regular),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Margin = new Padding(5, 0, 0, 0)
            };

            headerContentPanel.Controls.Add(pbLogo);
            headerContentPanel.Controls.Add(lblTitle);

            pbLogo.Location = new Point(0, 0);
            lblTitle.Location = new Point(pbLogo.Width + pbLogo.Margin.Left + pbLogo.Margin.Right, (pbLogo.Height - lblTitle.Height) / 2);

            headerContentPanel.Location = new Point(
                (topPanel.Width - headerContentPanel.Width) / 2,
                (topPanel.Height - headerContentPanel.Height) / 2
            );
            headerContentPanel.Anchor = AnchorStyles.None;
            topPanel.SizeChanged += (s, e) =>
            {
                headerContentPanel.Location = new Point(
                    (topPanel.Width - headerContentPanel.Width) / 2,
                    (topPanel.Height - headerContentPanel.Height) / 2
                );
            };

            Panel inputContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            bottomPanel.Controls.Add(inputContentPanel);

            pbSendIcon = new PictureBox
            {
                Size = new Size(50, 50),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Margin = new Padding(10, 0, 0, 0),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            LoadImageFromAssets(pbSendIcon, "send.png");

            pbMicIcon = new PictureBox
            {
                Size = new Size(30, 30),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Margin = new Padding(0, 0, 10, 0),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            LoadImageFromAssets(pbMicIcon, "mic.png");

            pbCameraIcon = new PictureBox
            {
                Size = new Size(30, 30),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Margin = new Padding(0, 0, 10, 0),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            LoadImageFromAssets(pbCameraIcon, "camera.png");

            inputContentPanel.Controls.Clear();

            FlowLayoutPanel leftIconsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 12, 0, 10),
                Margin = new Padding(0)
            };
            leftIconsPanel.Controls.Add(pbCameraIcon);
            leftIconsPanel.Controls.Add(pbMicIcon);

            rtInput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Padding = new Padding(10, 5, 10, 5),
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = false
            };

            rtInput.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    BtnSend_Click(sender, e);
                }
            };

            // Add controls to the inputContentPanel in the desired order
            inputContentPanel.Controls.Add(rtInput); // RichTextBox takes the middle space
            inputContentPanel.Controls.Add(pbSendIcon); // Send icon is on the right
            inputContentPanel.Controls.Add(leftIconsPanel); // Left icons panel is on the left

            chatFlowPanel.Controls.Add(leftIconsPanel); // Left icons panel docked left

            chatPanel.Controls.Clear();
            chatPanel.Controls.Add(chatFlowPanel);

            pbSendIcon.Click += BtnSend_Click;

            // Handle Resize event of chatFlowPanel to adjust AI bubble widths
            chatFlowPanel.Resize += ChatFlowPanel_Resize;
        }

        private void ChatFlowPanel_Resize(object? sender, EventArgs e)
        {
            // When the FlowLayoutPanel resizes, we need to update the width and position of the bubbles.
            int panelWidth = chatFlowPanel.ClientSize.Width;

            // Ensure panelWidth is positive before using it
            if (panelWidth <= 0) return;

            // Calculate the desired bubble width (70% of panel width minus horizontal padding/margins)
            int desiredBubbleWidth = (int)(panelWidth * 0.70) - chatFlowPanel.Padding.Horizontal - (chatFlowPanel.Controls.Count > 0 ? chatFlowPanel.Controls[0].Margin.Horizontal : 0); // Account for potential bubble margin
            if (desiredBubbleWidth < 1) desiredBubbleWidth = 1; // Ensure minimum width

            // Manually position and size controls
            foreach (Control control in chatFlowPanel.Controls)
            {
                if (control is ChatMessageBubble bubble)
                {
                    // Set the bubble's width and maximum size
                    bubble.Width = desiredBubbleWidth;
                    bubble.MaximumSize = new Size(bubble.Width, 0);

                    // Set the bubble's horizontal position based on role
                    if (bubble.Role == "user")
                    {
                        // Align right: set Left to panel width minus right padding minus bubble width
                        bubble.Left = panelWidth - chatFlowPanel.Padding.Right - bubble.Width;
                    }
                    else // AI/System message: align left
                    {
                        // Align left: set Left to panel left padding
                        bubble.Left = chatFlowPanel.Padding.Left;
                    }

                    // Trigger layout update for the bubble
                    bubble.PerformLayout();
                }
            }
            // Ensure the panel invalidates to redraw controls with updated sizes
            chatFlowPanel.Invalidate(true);
            // Update AutoScrollMinSize to reflect the total height of manually placed controls
            // The FlowLayoutPanel will calculate AutoScrollMinSize automatically based on control positions

            // Request the FlowLayoutPanel to re-layout based on manual changes
            chatFlowPanel.PerformLayout();
        }

        private void LoadConversationHistory()
        {
            // Clear existing controls
            chatFlowPanel.Controls.Clear();
            
            // Add initial message
            AddChatMessage("Glad you asked. Besides fixing your code and your life? Here's what I tolerate:\n\n• Reports - Like \"What's the last report we exported?\"\n• Your organization - \"How many people are using our software?\"\n• Features - \"How do I change the colors of my report?\"", "assistant");
            
            // Add any additional messages from conversation history
            foreach (var message in _conversationHistory)
            {
                AddChatMessage(message.Content, message.Role);
            }
        }

        private void LoadImageFromAssets(PictureBox pictureBox, string imageName)
        {
            // Get the directory of the currently executing assembly
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation)!;

            // Construct the path to the assets folder relative to the assembly directory.
            // This assumes the assets folder is in the project root, which is a couple of directories up
            // from the typical bin\[Debug|Release]\[TargetFramework] directory.
            // This path construction might need adjustment based on your exact project structure and build output.
            string projectDirectory = Directory.GetParent(assemblyDirectory!)!.Parent!.Parent!.FullName; // Adjust the number of .Parent! based on your structure
            string assetsPath = Path.Combine(projectDirectory, "assets", imageName);

            if (File.Exists(assetsPath))
            {
                try
                {
                    // Use Image.FromFile to load the image
                    pictureBox.Image = Image.FromFile(assetsPath);
                     // Dispose of the image later when the form is closed to release the file handle
                     this.FormClosed += (s, e) => { if (pictureBox.Image != null) { pictureBox.Image.Dispose(); } };
                }
                catch (Exception ex)
                {
                    // Handle potential errors during image loading
                    MessageBox.Show($"Error loading image {imageName} from {assetsPath}: {ex.Message}", "Image Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Handle case where image file is not found
                MessageBox.Show($"Image file not found at: {assetsPath}", "Image Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(28, 13, 29),
                    Color.FromArgb(92, 30, 30),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }
        }

        private void ApplyTheme()
        {
            Color backColor = _isDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            Color textColor = _isDarkMode ? Color.White : Color.Black;
            Color editorBackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            Color buttonBackColor = _isDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            Color accentColor = Color.FromArgb(0, 122, 204);

            this.BackColor = backColor;
            this.ForeColor = textColor;

            ApplyThemeToControl(this, _isDarkMode, textColor, backColor, editorBackColor, buttonBackColor, accentColor);

            _currentFont = new Font("Segoe UI", 10);
            ApplyFontToControl(this, _currentFont);
            AdjustButtonSizes();
            this.PerformLayout();
        }

        private void ApplyThemeToControl(Control control, bool isDarkMode, Color textColor, Color backColor, Color editorBackColor, Color buttonBackColor, Color accentColor)
        {
            if (control is Label titleLabel && titleLabel.Text == "Roast My Code")
            {
                titleLabel.ForeColor = accentColor;
                return;
            }

            control.BackColor = backColor;
            control.ForeColor = textColor;

            if (control is TextBox textBox)
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
                richTextBox.BackColor = editorBackColor;
                richTextBox.ForeColor = textColor;
                richTextBox.Invalidate();
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.BackColor = editorBackColor;
                comboBox.ForeColor = textColor;
                comboBox.Invalidate();
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.ForeColor = textColor;
                checkBox.Invalidate();
            }
            else if (control is Label label)
            {
                label.ForeColor = textColor;
                label.Invalidate();
            }
            else if (control is PictureBox pb)
            {
                // PictureBoxes typically don't have backgrounds/forecolors in the same way
                // as other controls, but you might adjust backcolor if not transparent.
                // pb.BackColor = backColor;
            }

            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child, isDarkMode, textColor, backColor, editorBackColor, buttonBackColor, accentColor);
            }
        }

        private void ApplyFontToControl(Control control, Font font)
        {
            if (control is TextBox || control is RichTextBox)
            {
                control.Font = font;
                control.Invalidate();
                control.PerformLayout();
            }
            else if (control is Label || control is Button || control is ComboBox)
            {
                control.Font = font;
                control.Invalidate();
                control.PerformLayout();
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

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            string userMessageText = rtInput.Text;
            if (!string.IsNullOrWhiteSpace(userMessageText))
            {
                AddChatMessage(userMessageText, "user");
                _conversationHistory.Add(new ChatMessage { Role = "user", Content = userMessageText });
                rtInput.Clear();

                rtInput.Enabled = false;
                pbSendIcon.Enabled = false;

                try
                {
                    string selectedLevel = "Savage";
                    string aiResponse = await _aiService.GenerateRoast(userMessageText, selectedLevel, _conversationHistory);

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

        private void AddChatMessage(string message, string role)
        {
            // Create a new ChatMessageBubble
            ChatMessageBubble bubble = new ChatMessageBubble
            {
                MessageText = message,
                Role = role,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Segoe UI", 12),
                Margin = new Padding(10, 5, 10, 5) // Add proper vertical margin between bubbles
            };

            // Add the bubble to the chat area panel
            chatAreaPanel.Controls.Add(bubble);

            // Position the new bubble and update layout
            PositionChatBubbles();

            // Scroll to the latest message
            if (chatAreaPanel.VerticalScroll.Visible)
            {
                chatAreaPanel.ScrollControlIntoView(bubble);
            }
        }

        private void ChatFlowPanel_ControlAdded(object? sender, ControlEventArgs e)
        {
            // Handle any necessary logic when a new control is added to the chat panel
            if (e.Control is ChatMessageBubble bubble)
            {
                // Ensure the new bubble is properly sized and positioned
                bubble.PerformLayout();
            }
        }

        private void ChatAreaPanel_Resize(object? sender, EventArgs e)
        {
            // When the chat area panel resizes, update the position and size of chat bubbles
            PositionChatBubbles();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            // Handle form resize
            if (chatAreaPanel != null)
            {
                chatAreaPanel.PerformLayout();
                PositionChatBubbles(); // Re-position bubbles after resize
            }
        }

        private void PositionChatBubbles()
        {
            // This method will be responsible for positioning and sizing all chat bubbles
            // within the chatAreaPanel to create the chat conversation layout.

            int panelWidth = chatAreaPanel.ClientSize.Width;
            if (panelWidth <= 0) return; // Ensure valid panel width

            // Calculate available width for bubbles (panel width minus padding)
            int availableWidth = panelWidth - chatAreaPanel.Padding.Horizontal;

            // Calculate the desired bubble width (70% of available width)
            int desiredBubbleWidth = (int)(availableWidth * 0.70);
            if (desiredBubbleWidth < 100) desiredBubbleWidth = 100; // Ensure minimum width

            int currentY = chatAreaPanel.Padding.Top; // Start positioning below top padding

            // Iterate through all controls (ChatMessageBubbles) in the chat area panel
            foreach (Control control in chatAreaPanel.Controls)
            {
                if (control is ChatMessageBubble bubble)
                {
                    // Set the bubble's width and maximum size based on calculation
                    bubble.Width = desiredBubbleWidth;
                    bubble.MaximumSize = new Size(bubble.Width, 0); // Lock maximum width

                    // Trigger layout on the bubble to calculate its required height based on text wrapping
                    bubble.PerformLayout();

                    // Calculate and set the bubble's horizontal position based on role
                    if (bubble.Role == "user")
                    {
                        // User message: align to the right with proper padding
                        bubble.Left = panelWidth - chatAreaPanel.Padding.Right - bubble.Width - 10;
                    }
                    else // Assistant or System message: align to the left
                    {
                        // Align to the left with proper padding
                        bubble.Left = chatAreaPanel.Padding.Left + 10;
                    }

                    // Set the bubble's vertical position
                    bubble.Top = currentY;

                    // Update the vertical position for the next bubble, including margin
                    currentY += bubble.Height + bubble.Margin.Vertical;
                }
            }

            // Update the AutoScrollMinSize based on content height
            chatAreaPanel.AutoScrollMinSize = new Size(0, currentY + 50); // Add small padding for scrolling

            // Invalidate the panel to ensure it redraws with updated control positions
            chatAreaPanel.Invalidate(true);

            // Always scroll to the latest message
            if (chatAreaPanel.Controls.Count > 0)
            {
                // Scroll to the latest message
                chatAreaPanel.ScrollControlIntoView(chatAreaPanel.Controls[chatAreaPanel.Controls.Count - 1]);
                
                // Force scroll to bottom
                chatAreaPanel.VerticalScroll.Value = chatAreaPanel.VerticalScroll.Maximum;
            }
        }

        private void ChatAreaPanel_Scroll(object? sender, ScrollEventArgs e)
        {
            // When the panel scrolls, ensure we're always at the bottom
            if (chatAreaPanel.VerticalScroll.Visible)
            {
                chatAreaPanel.VerticalScroll.Value = chatAreaPanel.VerticalScroll.Maximum;
            }
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}

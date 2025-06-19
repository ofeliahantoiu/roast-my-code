using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {        
        private void SendMessage()
        {
            // Validate message before sending
            if (string.IsNullOrWhiteSpace(rtInput.Text)) return;

            AddChatMessage(rtInput.Text, "user");
            _conversationHistory.Add(new ChatMessage { Content = rtInput.Text, Role = "user" });
            rtInput.Text = string.Empty;

            // Scroll to bottom using BeginInvoke to avoid layout lag
            this.BeginInvoke(new Action(() => {
                if (chatAreaPanel.Controls.Count > 0)
                    chatAreaPanel.ScrollControlIntoView(chatAreaPanel.Controls[chatAreaPanel.Controls.Count - 1]);
            }));
        }

        private async void AddChatMessage(string message, string role)
        {
            ChatMessageBubble bubble = new ChatMessageBubble
            {
                MessageText = message,
                Role = role,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = _currentFont,
                Margin = new Padding(10, 15, 10, 15) // Increased vertical spacing (top/bottom) to 15px
            };

            chatAreaPanel.Controls.Add(bubble);
            chatAreaPanel.Controls.SetChildIndex(bubble, chatAreaPanel.Controls.Count - 1); // Ensure order
            PositionChatBubbles();

            chatAreaPanel.ScrollControlIntoView(bubble);
            
            // Trigger shake animation when it's an assistant message (a roast)
            if (role == "assistant")
            {
                // Get the selected roast level or default to "Savage"
                string selectedLevel = (cmbRoastLevel.SelectedIndex > 0 ? cmbRoastLevel.SelectedItem?.ToString() : "Savage") ?? "Savage";
                
                // Only trigger animation if it's enabled in the animation dropdown (index 1 is "On")
                if (cmbAnimation.SelectedIndex == 1) 
                {
                    try
                    {
                        // Register key UI elements for animation
                        Services.AnimationService.Instance.RegisterAnimatedControl(bubble);
                        Services.AnimationService.Instance.RegisterAnimatedControl(chatAreaPanel);
                        
                        // Add more UI elements to affect based on intensity
                        if (selectedLevel == "Savage" || selectedLevel == "Brutal")
                        {
                            Services.AnimationService.Instance.RegisterAnimatedControl(inputPanel);
                        }
                        
                        // For brutal roasts, add even more shake targets
                        if (selectedLevel == "Brutal")
                        {
                            Services.AnimationService.Instance.RegisterTarget(this);
                        }
                        
                        // Trigger shake animation with intensity based on the roast level
                        await Services.AnimationService.Instance.ShakeAsync(selectedLevel);
                    }
                    catch (Exception ex)
                    {
                        // Just log or ignore animation errors so they don't disrupt the app
                        Debug.WriteLine($"Animation error: {ex.Message}");
                    }
                }
            }
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

                    // Add additional vertical spacing between bubbles (5% of panel height)
                    int additionalSpacing = (int)(chatAreaPanel.Height * 0.05);
                    currentY += bubble.Height + bubble.Margin.Vertical + additionalSpacing;
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

        private void ChatAreaPanel_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (e.Control is ChatMessageBubble bubble)
            {
                PositionChatBubbles();
                chatAreaPanel.ScrollControlIntoView(bubble);
            }
        }
    }
}
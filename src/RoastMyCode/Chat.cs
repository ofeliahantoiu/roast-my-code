using System;
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
        }

        private void AddChatMessage(string message, string role)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddChatMessage(message, role)));
                return;
            }

            // Calculate the Y position for the new message
            int newMessageY = chatAreaPanel.Padding.Top;
            if (chatAreaPanel.Controls.Count > 0)
            {
                Control lastControl = chatAreaPanel.Controls[chatAreaPanel.Controls.Count - 1];
                newMessageY = lastControl.Bottom + lastControl.Margin.Bottom;
            }

            ChatMessageBubble bubble = new ChatMessageBubble
            {
                MessageText = message,
                Role = role,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = _currentFont,
                Margin = new Padding(12)
            };

            // Set initial position
            bubble.Location = new Point(
                role == "user" 
                    ? chatAreaPanel.ClientSize.Width - chatAreaPanel.Padding.Right - bubble.PreferredSize.Width - bubble.Margin.Right
                    : chatAreaPanel.Padding.Left + bubble.Margin.Left,
                newMessageY
            );

            chatAreaPanel.Controls.Add(bubble);
            
            // Update bubble width and position after adding
            int desiredWidth = (int)(chatAreaPanel.ClientSize.Width * 0.70) - chatAreaPanel.Padding.Horizontal;
            bubble.MaximumSize = new Size(desiredWidth, 0);
            bubble.Width = desiredWidth;
            bubble.PerformLayout();

            // Adjust final horizontal position after layout
            bubble.Left = role == "user"
                ? chatAreaPanel.ClientSize.Width - chatAreaPanel.Padding.Right - bubble.Width - bubble.Margin.Right
                : chatAreaPanel.Padding.Left + bubble.Margin.Left;

            // Update scroll area
            chatAreaPanel.AutoScrollMinSize = new Size(0, bubble.Bottom + chatAreaPanel.Padding.Bottom);
            
            // Scroll to the new message
            chatAreaPanel.ScrollControlIntoView(bubble);
        }

        private void LoadConversationHistory()
        {
            chatAreaPanel.SuspendLayout();
            chatAreaPanel.Controls.Clear();
            
            // Load messages in chronological order (oldest first)
            foreach (var message in _conversationHistory)
            {
                AddChatMessage(message.Content, message.Role);
            }

            chatAreaPanel.ResumeLayout(true);
            
            if (chatAreaPanel.Controls.Count > 0)
            {
                // Scroll to the most recent message (at the bottom)
                chatAreaPanel.ScrollControlIntoView(chatAreaPanel.Controls[chatAreaPanel.Controls.Count - 1]);
            }
        }

        private void PositionChatBubbles()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(PositionChatBubbles));
                return;
            }

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
                    // Set width and recalculate height
                    bubble.MaximumSize = new Size(desiredBubbleWidth, 0);
                    bubble.Width = desiredBubbleWidth;
                    bubble.PerformLayout();

                    // Position horizontally based on role
                    bubble.Left = bubble.Role == "user" 
                        ? panelWidth - chatAreaPanel.Padding.Right - bubble.Width - bubble.Margin.Right
                        : chatAreaPanel.Padding.Left + bubble.Margin.Left;

                    // Position vertically with proper spacing
                    bubble.Top = currentY;
                    currentY = bubble.Bottom + bubble.Margin.Bottom;
                }
            }

            // Update scroll area
            chatAreaPanel.AutoScrollMinSize = new Size(0, currentY + chatAreaPanel.Padding.Bottom);
            chatAreaPanel.PerformLayout();
        }

        private void ChatAreaPanel_Scroll(object? sender, ScrollEventArgs e)
        {
            if (chatAreaPanel.Controls.Count > 0)
            {
                chatAreaPanel.ScrollControlIntoView(chatAreaPanel.Controls[chatAreaPanel.Controls.Count - 1]);
            }
        }
    }
}
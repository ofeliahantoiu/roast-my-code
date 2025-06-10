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
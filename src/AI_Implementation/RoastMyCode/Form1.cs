using System;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using RoastMyCode.Services;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private readonly AIService _aiService;
        private List<ChatMessage> _conversationHistory;

        public Form1()
        {
            InitializeComponent();
            _aiService = new AIService();
            _conversationHistory = new List<ChatMessage>();
            InitializeRoastLevels();
            InitializeChatInterface();
        }

        private void InitializeRoastLevels()
        {
            cmbRoastLevel.Items.AddRange(new string[] { "Light", "Savage", "Brutal" });
            cmbRoastLevel.SelectedIndex = 0;
        }

        private void InitializeChatInterface()
        {
            // Assuming you have these controls in your form:
            // txtCodeInput: TextBox for code input
            // txtOutput: TextBox for AI responses
            // btnRoast: Button for sending messages
            // cmbRoastLevel: ComboBox for roast level

            txtCodeInput.Multiline = true;
            txtCodeInput.Height = 100;
            txtCodeInput.PlaceholderText = "Paste your code here or type your message...";

            txtOutput.Multiline = true;
            txtOutput.ReadOnly = true;
            txtOutput.ScrollBars = ScrollBars.Vertical;

            btnRoast.Text = "Send";
        }

        private async void btnRoast_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodeInput.Text))
            {
                MessageBox.Show("Please enter some code or a message!", "Empty Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnRoast.Enabled = false;
            string userInput = txtCodeInput.Text;
            
            // Add user message to conversation
            _conversationHistory.Add(new ChatMessage { Role = "user", Content = userInput });
            
            // Display user message
            AppendToChat("You", userInput);
            txtCodeInput.Clear();

            try
            {
                string selectedLevel = cmbRoastLevel.SelectedItem?.ToString() ?? "Light";
                string response = await _aiService.GenerateRoast(userInput, selectedLevel, _conversationHistory);
                
                // Add AI response to conversation
                _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = response });
                
                // Display AI response
                AppendToChat("Code Roaster", response);
            }
            catch (Exception ex)
            {
                AppendToChat("Error", $"An error occurred: {ex.Message}");
            }
            finally
            {
                btnRoast.Enabled = true;
            }
        }

        private void AppendToChat(string sender, string message)
        {
            txtOutput.AppendText($"{sender}: {message}{Environment.NewLine}{Environment.NewLine}");
            txtOutput.ScrollToCaret();
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}

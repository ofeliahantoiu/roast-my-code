using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using RoastMyCode.Services;
using RoastMyCode.Controls;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private readonly AIService _aiService;
        private List<ChatMessage> _conversationHistory;
        private MonacoEditorControl _codeEditor;
        private TextBox txtOutput;
        private Button btnRoast;
        private ComboBox cmbRoastLevel;
        private Label lblStatus;

        public Form1()
        {
            InitializeComponent();
            _aiService = new AIService();
            _conversationHistory = new List<ChatMessage>();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Form settings
            this.Text = "Roast My Code";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            // Title label
            Label lblTitle = new Label
            {
                Text = "Roast My Code",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            // Roast level label and combo box
            Label lblRoastLevel = new Label
            {
                Text = "Select Roast Level:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 80),
                AutoSize = true
            };

            cmbRoastLevel = new ComboBox
            {
                Location = new Point(20, 110),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbRoastLevel.Items.AddRange(new string[] { "Light", "Savage", "Brutal" });
            cmbRoastLevel.SelectedIndex = 0;

            // Code editor
            Label lblCodeInput = new Label
            {
                Text = "Paste Your Code:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 150),
                AutoSize = true
            };

            _codeEditor = new MonacoEditorControl
            {
                Location = new Point(20, 180),
                Size = new Size(940, 300),
                Language = "csharp",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                MinimumSize = new Size(200, 100)
            };

            // Roast button
            btnRoast = new Button
            {
                Text = "Roast My Code!",
                Location = new Point(20, 490),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRoast.FlatAppearance.BorderSize = 0;
            btnRoast.Click += btnRoast_Click;

            // Status label
            lblStatus = new Label
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 9),
                Location = new Point(240, 500),
                AutoSize = true,
                ForeColor = Color.FromArgb(0, 122, 204)
            };

            // Output
            Label lblOutput = new Label
            {
                Text = "Roast Result:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 540),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            txtOutput = new TextBox
            {
                Location = new Point(20, 570),
                Size = new Size(940, 150),
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                MinimumSize = new Size(200, 100)
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblTitle, lblRoastLevel, cmbRoastLevel,
                lblCodeInput, _codeEditor, btnRoast,
                lblStatus, lblOutput, txtOutput
            });
        }

        private async void btnRoast_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_codeEditor.Code))
            {
                MessageBox.Show("Please enter some code first!", "Empty Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnRoast.Enabled = false;
            lblStatus.Text = "Roasting your code...";
            string userInput = _codeEditor.Code;

            try
            {
                string selectedLevel = cmbRoastLevel.SelectedItem?.ToString() ?? "Light";
                string response = await _aiService.GenerateRoast(userInput, selectedLevel, _conversationHistory);
                
                _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = response });
                txtOutput.Text = response;
                lblStatus.Text = "Roast complete!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error occurred";
            }
            finally
            {
                btnRoast.Enabled = true;
            }
        }
    }

    public class ChatMessage
    {
        public required string Role { get; set; }
        public required string Content { get; set; }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using RoastMyCode.Services;
using RoastMyCode.Controls;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private readonly AIService _aiService;
        private List<ChatMessage> _conversationHistory;
        private CodeEditorControl _codeEditor = null!;
        private TextBox txtOutput = null!;
        private Button btnRoast = null!;
        private Button btnCopy = null!;
        private Button btnSave = null!;
        private ComboBox cmbRoastLevel = null!;
        private Label lblStatus = null!;

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

            _codeEditor = new CodeEditorControl
            {
                Location = new Point(20, 180),
                Size = new Size(940, 300),
                Language = "C#",
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

            // Copy button
            btnCopy = new Button
            {
                Text = "Copy Code",
                Location = new Point(240, 490),
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCopy.FlatAppearance.BorderSize = 1;
            btnCopy.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            btnCopy.Click += btnCopy_Click;

            // Save button
            btnSave = new Button
            {
                Text = "Save Code",
                Location = new Point(380, 490),
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 1;
            btnSave.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            btnSave.Click += btnSave_Click;

            // Status label
            lblStatus = new Label
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 9),
                Location = new Point(520, 500),
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
                lblCodeInput, _codeEditor, btnRoast, btnCopy, btnSave,
                lblStatus, lblOutput, txtOutput
            });
        }

        private async void btnRoast_Click(object? sender, EventArgs e)
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

        private void btnCopy_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_codeEditor.Code))
            {
                MessageBox.Show("No code to copy!", "Empty Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Clipboard.SetText(_codeEditor.Code);
                lblStatus.Text = "Code copied to clipboard!";
                
                // Reset status after 2 seconds
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 2000;
                timer.Tick += (s, args) =>
                {
                    lblStatus.Text = "Ready";
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error copying code";
            }
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_codeEditor.Code))
            {
                MessageBox.Show("No code to save!", "Empty Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.DefaultExt = "cs";
                saveDialog.Title = "Save Code";
                saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(saveDialog.FileName, _codeEditor.Code);
                        lblStatus.Text = "Code saved successfully!";
                        
                        // Reset status after 2 seconds
                        var timer = new System.Windows.Forms.Timer();
                        timer.Interval = 2000;
                        timer.Tick += (s, args) =>
                        {
                            lblStatus.Text = "Ready";
                            timer.Stop();
                            timer.Dispose();
                        };
                        timer.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblStatus.Text = "Error saving code";
                    }
                }
            }
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}

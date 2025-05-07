using System;
using System.Drawing;
using System.Windows.Forms;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private readonly OpenAIService _openAIService;
        private Button btnCopy;
        private Button btnSave;

        public Form1()
        {
            InitializeComponent();
            _openAIService = new OpenAIService();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Form settings
            this.Text = "Roast My Code";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Input label
            Label lblInput = new Label
            {
                Text = "Paste your code:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Code input textbox
            TextBox txtCodeInput = new TextBox
            {
                Name = "txtCodeInput",
                Multiline = true,
                Location = new Point(20, 50),
                Size = new Size(740, 200),
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10F)
            };

            // Roast level label
            Label lblRoastLevel = new Label
            {
                Text = "Select roast level:",
                Location = new Point(20, 270),
                AutoSize = true
            };

            // Roast level combobox
            ComboBox cmbRoastLevel = new ComboBox
            {
                Name = "cmbRoastLevel",
                Location = new Point(20, 300),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRoastLevel.Items.AddRange(new string[] { "Light", "Savage", "Helpful" });
            cmbRoastLevel.SelectedIndex = 0;

            // Roast button
            Button btnRoast = new Button
            {
                Name = "btnRoast",
                Text = "Roast My Code!",
                Location = new Point(240, 300),
                Size = new Size(150, 30)
            };
            btnRoast.Click += async (s, e) => await RoastCodeAsync(txtCodeInput, cmbRoastLevel);

            // Output textbox
            TextBox txtOutput = new TextBox
            {
                Name = "txtOutput",
                Multiline = true,
                ReadOnly = true,
                Location = new Point(20, 350),
                Size = new Size(740, 150),
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F)
            };

            // Copy button
            btnCopy = new Button
            {
                Text = "Copy Response",
                Location = new Point(20, 510),
                Size = new Size(120, 30),
                Enabled = false
            };
            btnCopy.Click += (s, e) => CopyResponse(txtOutput);

            // Save button
            btnSave = new Button
            {
                Text = "Save Response",
                Location = new Point(150, 510),
                Size = new Size(120, 30),
                Enabled = false
            };
            btnSave.Click += (s, e) => SaveResponse(txtOutput);

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblInput, txtCodeInput, lblRoastLevel, cmbRoastLevel,
                btnRoast, txtOutput, btnCopy, btnSave
            });
        }

        private async Task RoastCodeAsync(TextBox txtCodeInput, ComboBox cmbRoastLevel)
        {
            if (string.IsNullOrWhiteSpace(txtCodeInput.Text))
            {
                MessageBox.Show("Please paste some code first!", "No Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnRoast.Enabled = false;
                btnRoast.Text = "Roasting...";
                Application.DoEvents();

                string roast = await _openAIService.GetRoastAsync(txtCodeInput.Text, cmbRoastLevel.Text);
                txtOutput.Text = roast;
                btnCopy.Enabled = true;
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRoast.Enabled = true;
                btnRoast.Text = "Roast My Code!";
            }
        }

        private void CopyResponse(TextBox txtOutput)
        {
            if (!string.IsNullOrEmpty(txtOutput.Text))
            {
                Clipboard.SetText(txtOutput.Text);
                MessageBox.Show("Response copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SaveResponse(TextBox txtOutput)
        {
            if (string.IsNullOrEmpty(txtOutput.Text))
                return;

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.RestoreDirectory = true;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.IO.File.WriteAllText(saveDialog.FileName, txtOutput.Text);
                        MessageBox.Show("Response saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
} 
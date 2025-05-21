using System;
using System.Drawing;
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
        private RichTextBox _codeEditor = null!;
        private TextBox txtOutput = null!;
        private Button btnRoast = null!;
        private Button btnCopy = null!;
        private Button btnSave = null!;
        private ComboBox cmbRoastLevel = null!;
        private Label lblStatus = null!;
        private CheckBox chkDarkMode = null!;
        private ComboBox cmbFontFamily = null!;
        private ComboBox cmbFontSize = null!;
        private bool _isDarkMode = true;
        private Font _currentFont = new Font("Segoe UI", 10);

        public Form1(IConfiguration configuration)
        {
            InitializeComponent();
            _aiService = new AIService(configuration);
            _conversationHistory = new List<ChatMessage>();
            InitializeCustomComponents();
            ApplyTheme();
        }

        private void InitializeCustomComponents()
        {
            // Form settings
            this.Text = "Roast My Code";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.MinimumSize = new Size(800, 600);

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

            // Create a panel for the top controls
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(20, 15, 20, 15)
            };

            // Create a flow layout panel for the top controls
            FlowLayoutPanel topFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0)
            };

            // Theme toggle
            chkDarkMode = new CheckBox
            {
                Text = "Dark Mode",
                Checked = true,
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 5, 20, 5)
            };
            chkDarkMode.CheckedChanged += chkDarkMode_CheckedChanged;

            // Font customization
            Label lblFont = new Label
            {
                Text = "Font:",
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 5, 5, 5)
            };

            cmbFontFamily = new ComboBox
            {
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 2, 20, 5)
            };
            cmbFontFamily.Items.AddRange(new string[] { 
                "Consolas",      // Windows default monospace
                "Courier New",   // Universal monospace
                "Lucida Console", // Windows monospace
                "Segoe UI Mono", // Windows modern monospace
                "Terminal"       // Classic Windows monospace
            });
            cmbFontFamily.SelectedItem = "Consolas";
            cmbFontFamily.SelectedIndexChanged += FontSettings_Changed;

            Label lblSize = new Label
            {
                Text = "Size:",
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 5, 5, 0)
            };

            cmbFontSize = new ComboBox
            {
                Size = new Size(60, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 2, 20, 0)
            };
            cmbFontSize.Items.AddRange(new string[] { "12", "14", "16" });
            cmbFontSize.SelectedItem = "14";
            cmbFontSize.SelectedIndexChanged += FontSettings_Changed;

            // Roast level controls
            Label lblRoast = new Label
            {
                Text = "Roast Level:",
                AutoSize = true,
                Font = _currentFont,
                ForeColor = Color.White,
                Margin = new Padding(0, 5, 5, 5)
            };

            cmbRoastLevel = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = _currentFont,
                Size = new Size(120, 25),
                Margin = new Padding(0, 2, 20, 5),
                Visible = true
            };
            cmbRoastLevel.Items.AddRange(new string[] { "Light", "Savage", "Brutal" });
            cmbRoastLevel.SelectedIndex = 0;

            // Add controls to flow panel
            topFlowPanel.Controls.AddRange(new Control[] {
                chkDarkMode, lblFont, cmbFontFamily, lblSize, cmbFontSize,
                lblRoast, cmbRoastLevel
            });

            // Add flow panel to top panel
            topPanel.Controls.Add(topFlowPanel);

            // Create a panel for the main content
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0)
            };

            // Create split container for editor and output
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = (int)(this.Height * 0.6),
                FixedPanel = FixedPanel.None
            };

            // Top: Code editor + label
            Panel editorPanel = new Panel 
            { 
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            Label lblCode = new Label
            {
                Text = "Paste Your Code:",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = _currentFont,
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                Padding = new Padding(5, 0, 0, 0),
                Margin = new Padding(0)
            };

            _codeEditor = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Both,
                Font = _currentFont,
                AcceptsTab = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White,
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Add controls in correct order
            editorPanel.Controls.Add(_codeEditor);
            editorPanel.Controls.Add(lblCode);
            lblCode.BringToFront();

            // Ensure editor panel is properly docked
            splitContainer.Panel1.Controls.Add(editorPanel);
            editorPanel.Dock = DockStyle.Fill;

            // Bottom: Output box
            Panel outputPanel = new Panel 
            { 
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            Label lblOutput = new Label
            {
                Text = "Roast Result:",
                Dock = DockStyle.Top,
                Height = 30,
                Font = _currentFont,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            txtOutput = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Font = _currentFont,
                BackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White,
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Both,
                WordWrap = true,
                AcceptsReturn = true,
                AcceptsTab = true,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            outputPanel.Controls.Add(lblOutput);
            outputPanel.Controls.Add(txtOutput);
            txtOutput.BringToFront();

            // Add panels to splitter
            splitContainer.Panel1.Controls.Add(editorPanel);
            splitContainer.Panel2.Controls.Add(outputPanel);
            mainPanel.Controls.Add(splitContainer);

            // Add event handler for text changes to ensure proper scrolling
            _codeEditor.TextChanged += (s, e) =>
            {
                _codeEditor.SelectionStart = 0;
                _codeEditor.ScrollToCaret();
            };

            // Button panel
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            btnRoast = new Button
            {
                Text = "Roast My Code!",
                Location = new Point(0, 5),
                Size = new Size(200, 40),
                Font = _currentFont,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRoast.FlatAppearance.BorderSize = 0;
            btnRoast.Click += btnRoast_Click;

            btnCopy = new Button
            {
                Text = "Copy Code",
                Location = new Point(220, 5),
                Size = new Size(120, 40),
                Font = _currentFont,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCopy.FlatAppearance.BorderSize = 1;
            btnCopy.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            btnCopy.Click += btnCopy_Click;

            btnSave = new Button
            {
                Text = "Save Code",
                Location = new Point(360, 5),
                Size = new Size(120, 40),
                Font = _currentFont,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 1;
            btnSave.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            btnSave.Click += btnSave_Click;

            lblStatus = new Label
            {
                Text = "Ready",
                Font = _currentFont,
                Location = new Point(500, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(0, 122, 204)
            };

            buttonPanel.Controls.AddRange(new Control[] { btnRoast, btnCopy, btnSave, lblStatus });
            mainPanel.Controls.Add(buttonPanel);

            // Add panels to form
            this.Controls.AddRange(new Control[] { lblTitle, topPanel, mainPanel });
        }

        private void chkDarkMode_CheckedChanged(object? sender, EventArgs e)
        {
            _isDarkMode = chkDarkMode.Checked;
            ApplyTheme();
        }

        private void FontSettings_Changed(object? sender, EventArgs e)
        {
            ApplyFontSettings();
        }

        private void ApplyFontSettings()
        {
            if (cmbFontFamily.SelectedItem == null || cmbFontSize.SelectedItem == null)
                return;

            try
            {
                string fontFamily = cmbFontFamily.SelectedItem.ToString()!;
                float fontSize = float.Parse(cmbFontSize.SelectedItem.ToString()!);
                
                // Create new font with proper style
                _currentFont = new Font(fontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Point);

                // Apply font to all controls
                ApplyFontToControl(this, _currentFont);

                // Adjust button sizes based on new font
                AdjustButtonSizes();

                // Force layout recalculation
                this.PerformLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying font: {ex.Message}", "Font Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Reset to default font
                _currentFont = new Font("Consolas", 14);
                cmbFontFamily.SelectedItem = "Consolas";
                cmbFontSize.SelectedItem = "14";
            }
        }

        private void ApplyFontToControl(Control control, Font font)
        {
            // Apply font to the control itself
            if (control is TextBox || control is RichTextBox)
            {
                control.Font = font;
                control.Invalidate();  // Force redraw
                control.PerformLayout(); // Force layout recalculation
            }
            else if (control is Label || control is Button || control is ComboBox)
            {
                control.Font = font;
                control.Invalidate();  // Force redraw
                control.PerformLayout(); // Force layout recalculation
            }

            // Recursively apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyFontToControl(child, font);
            }
        }

        private void AdjustButtonSizes()
        {
            // Calculate text sizes for buttons
            using (Graphics g = CreateGraphics())
            {
                SizeF roastSize = g.MeasureString(btnRoast.Text, btnRoast.Font);
                SizeF copySize = g.MeasureString(btnCopy.Text, btnCopy.Font);
                SizeF saveSize = g.MeasureString(btnSave.Text, btnSave.Font);

                // Set minimum widths and add padding
                int padding = 20;
                btnRoast.Width = Math.Max(200, (int)roastSize.Width + padding);
                btnCopy.Width = Math.Max(120, (int)copySize.Width + padding);
                btnSave.Width = Math.Max(120, (int)saveSize.Width + padding);

                // Adjust button panel height if needed
                int buttonHeight = Math.Max(40, (int)Math.Max(Math.Max(roastSize.Height, copySize.Height), saveSize.Height) + 20);
                btnRoast.Height = buttonHeight;
                btnCopy.Height = buttonHeight;
                btnSave.Height = buttonHeight;

                // Update button locations
                btnCopy.Location = new Point(btnRoast.Right + 10, btnRoast.Top);
                btnSave.Location = new Point(btnCopy.Right + 10, btnCopy.Top);
                lblStatus.Location = new Point(btnSave.Right + 20, btnSave.Top + (buttonHeight - lblStatus.Height) / 2);
            }
        }

        private void ApplyTheme()
        {
            Color backColor = _isDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            Color textColor = _isDarkMode ? Color.White : Color.Black;
            Color editorBackColor = _isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            Color buttonBackColor = _isDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            Color accentColor = Color.FromArgb(0, 122, 204);

            // Set form colors
            this.BackColor = backColor;
            this.ForeColor = textColor;

            // Apply theme to all controls recursively
            ApplyThemeToControl(this, _isDarkMode, textColor, backColor, editorBackColor, buttonBackColor, accentColor);

            // Ensure code editor and output maintain their fonts
            _codeEditor.Font = _currentFont;
            txtOutput.Font = _currentFont;
            btnRoast.Font = _currentFont;
            btnCopy.Font = _currentFont;
            btnSave.Font = _currentFont;
            lblStatus.Font = _currentFont;
        }

        private void ApplyThemeToControl(Control control, bool isDarkMode, Color textColor, Color backColor, Color editorBackColor, Color buttonBackColor, Color accentColor)
        {
            // Skip the title label as it should always be accent color
            if (control is Label titleLabel && titleLabel.Text == "Roast My Code")
            {
                titleLabel.ForeColor = accentColor;
                return;
            }

            // Set base colors
            control.BackColor = backColor;
            control.ForeColor = textColor;

            // Handle specific control types
            if (control is TextBox textBox)
            {
                textBox.BackColor = editorBackColor;
                textBox.ForeColor = textColor;
                textBox.Invalidate();
            }
            else if (control is RichTextBox richTextBox)
            {
                richTextBox.BackColor = editorBackColor;
                richTextBox.ForeColor = textColor;
                richTextBox.Invalidate();
            }
            else if (control is Button button)
            {
                if (button == btnRoast)
                {
                    button.BackColor = accentColor;
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = buttonBackColor;
                    button.ForeColor = textColor;
                }
                button.Invalidate();
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

            // Recursively apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child, isDarkMode, textColor, backColor, editorBackColor, buttonBackColor, accentColor);
            }
        }

        private async void btnRoast_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_codeEditor.Text))
            {
                MessageBox.Show("Please enter some code first!", "Empty Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnRoast.Enabled = false;
            lblStatus.Text = "Roasting your code...";
            string userInput = _codeEditor.Text;

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
            if (string.IsNullOrWhiteSpace(_codeEditor.Text))
            {
                MessageBox.Show("No code to copy!", "Empty Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Clipboard.SetText(_codeEditor.Text);
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
            if (string.IsNullOrWhiteSpace(_codeEditor.Text))
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
                        File.WriteAllText(saveDialog.FileName, _codeEditor.Text);
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

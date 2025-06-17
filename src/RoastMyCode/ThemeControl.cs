using System;
using System.Drawing;
using System.Windows.Forms;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {        
        private void ApplyTheme()
        {
            Color textColor = _isDarkMode ? Color.White : Color.Black;
            Color backColor = _isDarkMode ? Color.FromArgb(45, 45, 45) : Color.White;
            Color editorBackColor = _isDarkMode ? Color.FromArgb(50, 50, 50) : Color.White;
            Color buttonBackColor = _isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);

            this.BackColor = backColor;
            this.ForeColor = textColor;

            foreach (Control control in this.Controls)
            {
                ApplyThemeToControl(control, _isDarkMode, textColor, backColor, editorBackColor, buttonBackColor);
            }

            UpdateLogoIcon();
        }

        private void UpdateLogoIcon()
        {
            LoadImageFromAssets(pbLogo, _isDarkMode ? "twinklewhite.png" : "twinkleblack.png");
            LoadImageFromAssets(pbLogo, _isDarkMode ? "twinklewhite.png" : "twinkleblack.png");
        }

        private void ApplyThemeToControl(Control control, bool isDarkMode, Color textColor, Color backColor, Color editorBackColor, Color buttonBackColor)
        {
            if (control is Label titleLabel && titleLabel.Text == "Roast My Code")
            {
                titleLabel.ForeColor = textColor;
                return;
            }

            if (control is ChatMessageBubble bubble)
            {
                if (bubble.Role == "user")
                {
                    bubble.BackColor = Color.FromArgb(70, 70, 70);
                    bubble.ForeColor = Color.White;
                }
                else
                {
                    bubble.ForeColor = isDarkMode ? Color.White : Color.Black;
                }
                bubble.Invalidate();
                return;
            }

            control.BackColor = backColor;
            control.ForeColor = textColor;

            if (control is ComboBox comboBox)
            {
                comboBox.BackColor = isDarkMode ? Color.FromArgb(45, 45, 48) : Color.FromArgb(240, 240, 240);
                comboBox.ForeColor = textColor;
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.Invalidate();
            }
            else if (control is TextBox textBox)
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
                 richTextBox.BackColor = isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240); 
                 
                 if (richTextBox is RoundedRichTextBox rtInput)
                 {
                     rtInput.BorderColor = isDarkMode ? Color.FromArgb(100, 100, 100) : Color.FromArgb(180, 180, 180);
                     rtInput.BorderWidth = 1;
                     
                     rtInput.ForeColor = isDarkMode ? Color.White : Color.Black;
                 }
                 else
                 {
                     richTextBox.ForeColor = textColor;
                 }
                 
                 richTextBox.Invalidate();
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.ForeColor = textColor;
                checkBox.Invalidate();
            }
            else if (control is Label label)
            {
                 if (label.Text != "Roast My Code")
                 {
                     label.ForeColor = textColor;
                     label.Invalidate();
                 }
            }
             else if (control is PictureBox pb)
            {
                pb.BackColor = backColor;
            }
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child, isDarkMode, textColor, backColor, editorBackColor, buttonBackColor);
            }
        }

        private void ApplyFontToControl(Control control, Font font)
        {
            if (control is TextBox || control is RichTextBox || control is Label ||
                control is Button || control is ComboBox || control is ChatMessageBubble)
            {
                control.Font = font;
                control.Invalidate();
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

        private void UpdateFont()
        {
            if (cmbFontStyle.SelectedIndex > 0 || cmbFontSize.SelectedIndex > 0)
            {
                string fontFamily = cmbFontStyle.SelectedIndex > 0 ? 
                                    cmbFontStyle.SelectedItem.ToString()! : 
                                    _currentFont.FontFamily.Name;

                float fontSize = cmbFontSize.SelectedIndex > 0 ? 
                                 float.Parse(cmbFontSize.SelectedItem.ToString()!) : 
                                 _currentFont.Size;

                if (fontSize <= 0) fontSize = 8;

                _currentFont = new Font(fontFamily, fontSize);

                if (chatAreaPanel != null)
                {
                    ApplyFontToControl(chatAreaPanel, _currentFont);
                }
                if (rtInput != null)
                {
                    rtInput.Font = _currentFont;
                    rtInput.PerformLayout();
                }
            }
        }

        private void ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;
            ApplyTheme();
            UpdateThemeIcon();
            if (pbSendIcon != null) LoadImageFromAssets(pbSendIcon, _isDarkMode ? "send.png" : "message.png");
            if (pbCameraIcon != null) LoadImageFromAssets(pbCameraIcon, _isDarkMode ? "cameralight.png" : "cameradark.png");
            if (pbMicIcon != null) LoadImageFromAssets(pbMicIcon, _isDarkMode ? "volumeWhite.png" : "volume.png");
            if (pbUploadIcon != null) LoadImageFromAssets(pbUploadIcon, _isDarkMode ? "uploadlight.png" : "uploaddark.png");
        }

        private void UpdateThemeIcon()
        {
            if (pbThemeToggle != null) LoadImageFromAssets(pbThemeToggle, _isDarkMode ? "brightness.png" : "moon.png");
        }
    }
}
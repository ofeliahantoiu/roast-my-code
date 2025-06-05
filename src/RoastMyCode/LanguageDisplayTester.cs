using System;
using System.Windows.Forms;

namespace RoastMyCode
{
    /// <summary>
    /// A simple form to test the CurrentLanguageDisplay animation
    /// </summary>
    public class LanguageDisplayTester : Form
    {
        private CurrentLanguageDisplay languageDisplay;
        private ComboBox cmbLanguages;
        private CheckBox chkDarkMode;
        private Button btnAnimate;
        
        public LanguageDisplayTester()
        {
            this.Text = "Language Display Animation Tester";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            this.ForeColor = System.Drawing.Color.White;
            
            // Create language display control
            languageDisplay = new CurrentLanguageDisplay
            {
                Size = new System.Drawing.Size(200, 80),
                Location = new System.Drawing.Point(150, 50),
                IsDarkMode = true,
                Language = "C#"
            };
            this.Controls.Add(languageDisplay);
            
            // Create language dropdown
            cmbLanguages = new ComboBox
            {
                Location = new System.Drawing.Point(150, 150),
                Size = new System.Drawing.Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLanguages.Items.AddRange(new string[] { 
                "C#", "JavaScript", "TypeScript", "Python", "Java", "C++", "C", 
                "PHP", "Ruby", "Go", "Rust", "Swift", "HTML", "CSS", "JSON" 
            });
            cmbLanguages.SelectedIndex = 0;
            this.Controls.Add(cmbLanguages);
            
            // Create dark mode toggle
            chkDarkMode = new CheckBox
            {
                Text = "Dark Mode",
                Location = new System.Drawing.Point(150, 190),
                Size = new System.Drawing.Size(200, 30),
                Checked = true
            };
            chkDarkMode.CheckedChanged += (s, e) => {
                languageDisplay.IsDarkMode = chkDarkMode.Checked;
                this.BackColor = chkDarkMode.Checked ? 
                    System.Drawing.Color.FromArgb(45, 45, 45) : 
                    System.Drawing.Color.White;
                this.ForeColor = chkDarkMode.Checked ? 
                    System.Drawing.Color.White : 
                    System.Drawing.Color.Black;
            };
            this.Controls.Add(chkDarkMode);
            
            // Create animate button
            btnAnimate = new Button
            {
                Text = "Change Language & Animate",
                Location = new System.Drawing.Point(150, 230),
                Size = new System.Drawing.Size(200, 40)
            };
            btnAnimate.Click += (s, e) => {
                if (cmbLanguages.SelectedItem != null)
                {
                    string newLanguage = cmbLanguages.SelectedItem.ToString();
                    if (languageDisplay.Language != newLanguage)
                    {
                        languageDisplay.Language = newLanguage;
                        languageDisplay.AnimateLanguageChange();
                    }
                }
            };
            this.Controls.Add(btnAnimate);
        }
        
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LanguageDisplayTester());
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RoastMyCode
{
    /// <summary>
    /// Test form to demonstrate and test the enhanced animations
    /// </summary>
    public class AnimationTester : Form
    {
        private FlowLayoutPanel _chatPanel;
        private ComboBox _animationTypeComboBox;
        private ComboBox _easingTypeComboBox;
        private ComboBox _roleComboBox;
        private TrackBar _severityTrackBar;
        private Button _testButton;
        private CheckBox _darkModeCheckBox;
        private WebcamControl _webcamControl;
        private Label _severityLabel;
        private TextBox _messageTextBox;
        
        public AnimationTester()
        {
            InitializeComponent();
            PopulateComboBoxes();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Animation Tester";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            
            // Create layout panels
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            
            // Chat panel for displaying animated chat bubbles
            _chatPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
                BackColor = Color.FromArgb(20, 20, 20),
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Controls panel
            Panel controlsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(40, 40, 40)
            };
            
            // Add panels to main layout
            mainLayout.Controls.Add(_chatPanel, 0, 0);
            mainLayout.Controls.Add(controlsPanel, 1, 0);
            
            // Create controls for the control panel
            TableLayoutPanel controlsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(5)
            };
            controlsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            controlsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            
            // Add controls to the layout
            int row = 0;
            
            // Animation Type
            Label animationTypeLabel = new Label { Text = "Animation Type:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            _animationTypeComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            controlsLayout.Controls.Add(animationTypeLabel, 0, row);
            controlsLayout.Controls.Add(_animationTypeComboBox, 1, row++);
            
            // Easing Type
            Label easingTypeLabel = new Label { Text = "Easing Type:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            _easingTypeComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            controlsLayout.Controls.Add(easingTypeLabel, 0, row);
            controlsLayout.Controls.Add(_easingTypeComboBox, 1, row++);
            
            // Role
            Label roleLabel = new Label { Text = "Role:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            _roleComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            controlsLayout.Controls.Add(roleLabel, 0, row);
            controlsLayout.Controls.Add(_roleComboBox, 1, row++);
            
            // Severity
            Label severityLabel = new Label { Text = "Roast Severity:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            _severityTrackBar = new TrackBar { Dock = DockStyle.Fill, Minimum = 1, Maximum = 5, Value = 3, TickFrequency = 1, TickStyle = TickStyle.Both };
            controlsLayout.Controls.Add(severityLabel, 0, row);
            controlsLayout.Controls.Add(_severityTrackBar, 1, row++);
            
            // Severity Label
            _severityLabel = new Label { Text = "Current: 3", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            controlsLayout.Controls.Add(_severityLabel, 1, row++);
            
            // Dark Mode
            Label darkModeLabel = new Label { Text = "Dark Mode:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            _darkModeCheckBox = new CheckBox { Dock = DockStyle.Fill, Checked = true, Text = "Enabled" };
            controlsLayout.Controls.Add(darkModeLabel, 0, row);
            controlsLayout.Controls.Add(_darkModeCheckBox, 1, row++);
            
            // Message Text
            Label messageLabel = new Label { Text = "Message:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            controlsLayout.Controls.Add(messageLabel, 0, row);
            
            _messageTextBox = new TextBox 
            { 
                Dock = DockStyle.Fill, 
                Multiline = true, 
                Height = 100,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Text = "This is a test message to demonstrate the animation effects."
            };
            controlsLayout.Controls.Add(_messageTextBox, 0, row);
            controlsLayout.SetColumnSpan(_messageTextBox, 2);
            controlsLayout.SetRowSpan(_messageTextBox, 2);
            row += 2;
            
            // Test Button
            _testButton = new Button { Text = "Test Animation", Dock = DockStyle.Fill, BackColor = Color.FromArgb(0, 120, 215), FlatStyle = FlatStyle.Flat };
            _testButton.FlatAppearance.BorderSize = 0;
            controlsLayout.Controls.Add(_testButton, 0, row++);
            controlsLayout.SetColumnSpan(_testButton, 2);
            
            // Webcam Control
            _webcamControl = new WebcamControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };
            controlsLayout.Controls.Add(_webcamControl, 0, row);
            controlsLayout.SetColumnSpan(_webcamControl, 2);
            controlsLayout.SetRowSpan(_webcamControl, 2);
            
            // Add the controls layout to the controls panel
            controlsPanel.Controls.Add(controlsLayout);
            
            // Add the main layout to the form
            this.Controls.Add(mainLayout);
            
            // Wire up events
            _testButton.Click += TestButton_Click;
            _darkModeCheckBox.CheckedChanged += DarkMode_CheckedChanged;
            _severityTrackBar.ValueChanged += SeverityTrackBar_ValueChanged;
            
            // Initialize webcam
            _webcamControl.InitializeWebcam();
        }
        
        private void PopulateComboBoxes()
        {
            // Animation types
            _animationTypeComboBox.Items.AddRange(new object[] { 
                "FadeIn", "SlideIn", "FadeSlideIn", "BounceIn", "GrowIn", "PopIn" 
            });
            _animationTypeComboBox.SelectedIndex = 0;
            
            // Easing types
            _easingTypeComboBox.Items.AddRange(new object[] { 
                "EaseLinear", "EaseInQuad", "EaseOutQuad", "EaseInOutQuad", 
                "EaseInCubic", "EaseOutCubic", "EaseInOutCubic", "EaseOutBack", "EaseOutBounce"
            });
            _easingTypeComboBox.SelectedIndex = 2;
            
            // Roles
            _roleComboBox.Items.AddRange(new object[] { "user", "assistant", "system" });
            _roleComboBox.SelectedIndex = 1;
        }
        
        private void TestButton_Click(object sender, EventArgs e)
        {
            string animationType = _animationTypeComboBox.SelectedItem.ToString();
            string easingType = _easingTypeComboBox.SelectedItem.ToString();
            string role = _roleComboBox.SelectedItem.ToString();
            bool isDarkMode = _darkModeCheckBox.Checked;
            int severity = _severityTrackBar.Value;
            string message = _messageTextBox.Text;
            
            if (string.IsNullOrEmpty(message))
                message = "This is a test message to demonstrate the animation effects.";
                
            // Create and add the animated chat bubble
            AnimatedChatBubble bubble = new AnimatedChatBubble
            {
                MessageText = message,
                Role = role,
                AnimationType = animationType,
                EasingType = easingType,
                AnimationDuration = 1000,
                AutoSize = true,
                Width = (int)(_chatPanel.Width * 0.85),
                MaximumSize = new Size((int)(_chatPanel.Width * 0.85), 0),
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(10, 5, 10, 5),
                IsDarkMode = isDarkMode
            };
            
            _chatPanel.Controls.Add(bubble);
            _chatPanel.ScrollControlIntoView(bubble);
            bubble.StartAnimation();
            
            // Apply webcam effect if webcam is active
            if (_webcamControl.IsWebcamActive)
            {
                string effect = "None";
                switch (severity)
                {
                    case 1: effect = "Sepia"; break;
                    case 2: effect = "Grayscale"; break;
                    case 3: effect = "Pixelate"; break;
                    case 4: effect = "Invert"; break;
                    case 5: effect = "Clown"; break;
                }
                _webcamControl.StartAnimatedEffect(severity, effect);
            }
        }
        
        private void DarkMode_CheckedChanged(object sender, EventArgs e)
        {
            bool isDarkMode = _darkModeCheckBox.Checked;
            
            // Update form colors
            this.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 240, 240);
            this.ForeColor = isDarkMode ? Color.White : Color.Black;
            
            // Update chat panel
            _chatPanel.BackColor = isDarkMode ? Color.FromArgb(20, 20, 20) : Color.FromArgb(250, 250, 250);
            
            // Update existing chat bubbles
            foreach (Control control in _chatPanel.Controls)
            {
                if (control is AnimatedChatBubble bubble)
                {
                    bubble.IsDarkMode = isDarkMode;
                }
            }
        }
        
        private void SeverityTrackBar_ValueChanged(object sender, EventArgs e)
        {
            _severityLabel.Text = $"Current: {_severityTrackBar.Value}";
        }
    }
}

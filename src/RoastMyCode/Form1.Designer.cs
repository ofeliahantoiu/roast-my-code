namespace RoastMyCode
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.FlowLayoutPanel chatFlowPanel;
        private System.Windows.Forms.Panel inputContentPanel;
        private System.Windows.Forms.RichTextBox rtInput;
        private System.Windows.Forms.PictureBox pbSendIcon;
        private System.Windows.Forms.PictureBox pbMicIcon;
        private System.Windows.Forms.PictureBox pbCameraIcon;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Panel chatPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.chatFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.inputContentPanel = new System.Windows.Forms.Panel();
            this.rtInput = new System.Windows.Forms.RichTextBox();
            this.pbSendIcon = new System.Windows.Forms.PictureBox();
            this.pbMicIcon = new System.Windows.Forms.PictureBox();
            this.pbCameraIcon = new System.Windows.Forms.PictureBox();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.topPanel = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.chatPanel = new System.Windows.Forms.Panel();

            ((System.ComponentModel.ISupportInitialize)(this.pbSendIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbMicIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCameraIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();

            this.SuspendLayout();

            // topPanel
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Height = 100;
            this.topPanel.BackColor = System.Drawing.Color.Transparent;
            this.topPanel.Controls.Add(this.pbLogo);
            this.topPanel.Controls.Add(this.lblTitle);

            // lblTitle
            this.lblTitle.Text = "Roast My Code";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 24);
            this.lblTitle.AutoSize = true;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(120, 30);

            // pbLogo
            this.pbLogo.Size = new System.Drawing.Size(80, 80);
            this.pbLogo.Location = new System.Drawing.Point(20, 10);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            // bottomPanel
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Height = 120;
            this.bottomPanel.Controls.Add(this.inputContentPanel);

            // inputContentPanel
            this.inputContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputContentPanel.Padding = new System.Windows.Forms.Padding(10);
            this.inputContentPanel.Controls.Add(this.pbCameraIcon);
            this.inputContentPanel.Controls.Add(this.pbMicIcon);
            this.inputContentPanel.Controls.Add(this.rtInput);
            this.inputContentPanel.Controls.Add(this.pbSendIcon);

            // pbCameraIcon
            this.pbCameraIcon.Location = new System.Drawing.Point(10, 10);
            this.pbCameraIcon.Size = new System.Drawing.Size(40, 40);
            this.pbCameraIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            // pbMicIcon
            this.pbMicIcon.Location = new System.Drawing.Point(60, 10);
            this.pbMicIcon.Size = new System.Drawing.Size(40, 40);
            this.pbMicIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            // pbSendIcon
            this.pbSendIcon.Location = new System.Drawing.Point(700, 10);
            this.pbSendIcon.Size = new System.Drawing.Size(50, 50);
            this.pbSendIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbSendIcon.Cursor = System.Windows.Forms.Cursors.Hand;

            // rtInput
            this.rtInput.Location = new System.Drawing.Point(110, 10);
            this.rtInput.Size = new System.Drawing.Size(580, 80);
            this.rtInput.Font = new System.Drawing.Font("Segoe UI", 12);
            this.rtInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // chatFlowPanel
            this.chatFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.chatFlowPanel.WrapContents = false;
            this.chatFlowPanel.AutoScroll = true;
            this.chatFlowPanel.Padding = new System.Windows.Forms.Padding(20);
            this.chatFlowPanel.BackColor = System.Drawing.Color.Transparent;
            this.chatFlowPanel.Resize += new System.EventHandler(this.ChatFlowPanel_Resize);

            // chatPanel
            this.chatPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatPanel.Controls.Add(this.chatFlowPanel);

            // Form1
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.chatPanel);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.bottomPanel);
            this.Name = "Form1";
            this.Text = "Roast My Code";

            ((System.ComponentModel.ISupportInitialize)(this.pbSendIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbMicIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCameraIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
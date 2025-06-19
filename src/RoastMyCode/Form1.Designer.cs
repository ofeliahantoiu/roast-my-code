namespace RoastMyCode
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Button btnDownloadConversation;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.topPanel = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.btnDownloadConversation = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.SuspendLayout();

            // pbLogo
            this.pbLogo.Size = new System.Drawing.Size(80, 80);
            this.pbLogo.Location = new System.Drawing.Point(20, 10);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            // lblTitle
            this.lblTitle.Text = "Roast My Code";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 24);
            this.lblTitle.AutoSize = true;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(120, 30);

            // btnDownloadConversation
            this.btnDownloadConversation.Text = "Download Conversation";
            this.btnDownloadConversation.Location = new System.Drawing.Point(400, 30);
            this.btnDownloadConversation.Size = new System.Drawing.Size(150, 30);
            this.btnDownloadConversation.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.btnDownloadConversation.ForeColor = System.Drawing.Color.White;
            this.btnDownloadConversation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownloadConversation.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.btnDownloadConversation.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDownloadConversation.Click += new System.EventHandler(this.BtnDownloadConversation_Click);
            this.btnDownloadConversation.Visible = true;
            this.btnDownloadConversation.Enabled = true;
            this.btnDownloadConversation.BringToFront();

            // topPanel
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Height = 100;
            this.topPanel.BackColor = System.Drawing.Color.Transparent;
            this.topPanel.Controls.Add(this.btnDownloadConversation);
            this.topPanel.Controls.Add(this.pbLogo);
            this.topPanel.Controls.Add(this.lblTitle);

            // bottomPanel
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Height = 120;

            // Form1
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.chatAreaPanel);
            this.Controls.Add(this.bottomPanel);
            this.Name = "Form1";
            this.Text = "Roast My Code";

            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

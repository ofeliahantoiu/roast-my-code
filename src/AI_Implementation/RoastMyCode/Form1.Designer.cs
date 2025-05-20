namespace RoastMyCode
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblCodeInput = new System.Windows.Forms.Label();
            this.txtCodeInput = new System.Windows.Forms.TextBox();
            this.lblRoastLevel = new System.Windows.Forms.Label();
            this.cmbRoastLevel = new System.Windows.Forms.ComboBox();
            this.btnRoast = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            this.lblCodeInput.AutoSize = true;
            this.lblCodeInput.Location = new System.Drawing.Point(12, 15);
            this.lblCodeInput.Name = "lblCodeInput";
            this.lblCodeInput.Size = new System.Drawing.Size(89, 15);
            this.lblCodeInput.TabIndex = 0;
            this.lblCodeInput.Text = "Paste your code:";
            this.txtCodeInput.Location = new System.Drawing.Point(12, 33);
            this.txtCodeInput.Multiline = true;
            this.txtCodeInput.Name = "txtCodeInput";
            this.txtCodeInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCodeInput.Size = new System.Drawing.Size(776, 200);
            this.txtCodeInput.TabIndex = 1;
            this.lblRoastLevel.AutoSize = true;
            this.lblRoastLevel.Location = new System.Drawing.Point(12, 246);
            this.lblRoastLevel.Name = "lblRoastLevel";
            this.lblRoastLevel.Size = new System.Drawing.Size(95, 15);
            this.lblRoastLevel.TabIndex = 2;
            this.lblRoastLevel.Text = "Select roast level:";
            this.cmbRoastLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRoastLevel.FormattingEnabled = true;
            this.cmbRoastLevel.Location = new System.Drawing.Point(12, 264);
            this.cmbRoastLevel.Name = "cmbRoastLevel";
            this.cmbRoastLevel.Size = new System.Drawing.Size(200, 23);
            this.cmbRoastLevel.TabIndex = 3;
            this.btnRoast.Location = new System.Drawing.Point(218, 264);
            this.btnRoast.Name = "btnRoast";
            this.btnRoast.Size = new System.Drawing.Size(100, 23);
            this.btnRoast.TabIndex = 4;
            this.btnRoast.Text = "Roast My Code";
            this.btnRoast.UseVisualStyleBackColor = true;
            this.btnRoast.Click += new System.EventHandler(this.btnRoast_Click);
            this.txtOutput.Location = new System.Drawing.Point(12, 293);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(776, 200);
            this.txtOutput.TabIndex = 5;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 505);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.btnRoast);
            this.Controls.Add(this.cmbRoastLevel);
            this.Controls.Add(this.lblRoastLevel);
            this.Controls.Add(this.txtCodeInput);
            this.Controls.Add(this.lblCodeInput);
            this.Name = "Form1";
            this.Text = "Roast My Code";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblCodeInput;
        private System.Windows.Forms.TextBox txtCodeInput;
        private System.Windows.Forms.Label lblRoastLevel;
        private System.Windows.Forms.ComboBox cmbRoastLevel;
        private System.Windows.Forms.Button btnRoast;
        private System.Windows.Forms.TextBox txtOutput;
    }
}


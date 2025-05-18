using System;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using RoastMyCode.Services;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private readonly AIService _aiService;

        public Form1()
        {
            MessageBox.Show("App started");
            InitializeComponent();
            _aiService = new AIService();
            InitializeRoastLevels();
        }

        private void InitializeRoastLevels()
        {
            cmbRoastLevel.Items.AddRange(new string[] { "Light", "Savage", "Brutal" });
            cmbRoastLevel.SelectedIndex = 0;
        }

        private async void btnRoast_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodeInput.Text))
            {
                MessageBox.Show("Please paste some code first!", "No Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnRoast.Enabled = false;
            txtOutput.Text = "Analyzing your code...";

            try
            {
                string selectedLevel = cmbRoastLevel.SelectedItem?.ToString() ?? "Light";
                string response = await _aiService.GenerateRoast(txtCodeInput.Text, selectedLevel);
                txtOutput.Text = response;
            }
            catch (Exception ex)
            {
                txtOutput.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnRoast.Enabled = true;
            }
        }
    }
}

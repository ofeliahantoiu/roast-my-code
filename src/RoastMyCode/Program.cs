using System;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RoastMyCode
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1(configuration));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start application: {ex.Message}", 
                    "Startup Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
}

using System;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

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

                // Set up services
                var services = new ServiceCollection();
                
                // Configure FileUploadOptions
                var fileUploadSection = configuration.GetSection("FileUpload");
                var fileUploadOptions = new FileUploadOptions();
                fileUploadSection.Bind(fileUploadOptions);
                services.AddSingleton(fileUploadOptions);
                
                var serviceProvider = services.BuildServiceProvider();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                ApplicationConfiguration.Initialize();
                
                // Create form with configuration and services
                using var scope = serviceProvider.CreateScope();
                var form = new Form1(configuration, scope.ServiceProvider);
                Application.Run(form);
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

using System;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;

namespace RoastMyCode
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(configuration));
        }
    }
}

using Microsoft.Extensions.Configuration;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Windows;


namespace KTruckGui
{
    public partial class App : System.Windows.Application
    {
        public static IConfiguration? Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Console.WriteLine("Application is starting...");

            QuestPDF.Settings.License = LicenseType.Community;
            // Initialize configuration from appsettings.json
            string connectionString = Environment.GetEnvironmentVariable("DieselShopDb") ?? string.Empty;

            // Verify that the connection string is loaded properly
            if (string.IsNullOrEmpty(connectionString))
            {
                System.Windows.MessageBox.Show("Connection string could not be found in environment variables. Please verify the configuration.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(); // Shutdown the app if critical configuration is missing
            }
            else
            {
                Console.WriteLine("Connection string successfully loaded.");
            }
        }
    }
}

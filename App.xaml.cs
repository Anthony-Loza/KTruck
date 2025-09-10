using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;
using System.IO;
using System;
using System.Windows;

namespace KTruckGui
{
    public partial class App : System.Windows.Application  // <-- fully qualified
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Console.WriteLine("Application is starting...");
            QuestPDF.Settings.License = LicenseType.Community;

            string connectionString =
                Environment.GetEnvironmentVariable("DieselShopDb")
                ?? Configuration.GetConnectionString("DieselShopDb")
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                System.Windows.MessageBox.Show(
                    "Connection string could not be found. " +
                    "Set the DieselShopDb environment variable or add it to appsettings.json.",
                    "Configuration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
                return;
            }

            await AppHost.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        public static IHost AppHost { get; private set; } = null!;
        public static IConfiguration Configuration => AppHost.Services.GetRequiredService<IConfiguration>();

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.SetBasePath(AppContext.BaseDirectory);
                    cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    cfg.AddEnvironmentVariables();
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddTransient<MainWindow>();
                })
                .Build();
        }
       


        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
            base.OnExit(e);
        }
    }
}

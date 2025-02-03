using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using KTruckGui.Models;
using Microsoft.VisualBasic; // Add this line at the top of your file


namespace KTruckGui
{
    public partial class MainWindow : Window
    {

        private static readonly string repoOwner = "Anthony-Loza";
        private static readonly string repoName = "KTruckGuiC";
        private static readonly string currentVersion = GetCurrentVersion();
        public MainWindow()
        {
            InitializeComponent(); // Call to the auto-generated method
            LoadDashboardData();
            Dispatcher.BeginInvoke(new Action(async () => await CheckForUpdatesAsync()), DispatcherPriority.Background);
        }
        private static string GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";
        }
        private async Task CheckForUpdatesAsync()
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
                string url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";

                string response = await client.GetStringAsync(url);
                using JsonDocument json = JsonDocument.Parse(response);
                string latestVersionTag = json.RootElement.GetProperty("tag_name").GetString();

                // Extract only the numeric portion (remove 'ktruck/v')
                string latestVersion = latestVersionTag.Replace("ktruck/v", "");

                if (latestVersion != currentVersion)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show($"A new version ({latestVersion}) is available. Would you like to update?", "Update Available", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        string downloadUrl = json.RootElement.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();
                        await DownloadAndReplaceExecutable(downloadUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
            }
        }


        private async Task DownloadAndReplaceExecutable(string url)
        {
            try
            {
                string tempFilePath = Path.Combine(Path.GetTempPath(), "KTruckGui_New.exe");

                using HttpClient client = new HttpClient();
                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var httpStream = await response.Content.ReadAsStreamAsync();

                byte[] buffer = new byte[8192];
                int bytesRead;
                long totalBytes = response.Content.Headers.ContentLength ?? 1;
                long totalRead = 0;

                while ((bytesRead = await httpStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                }

                System.Windows.MessageBox.Show("Update downloaded. Restarting application...", "Update Successful");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C timeout 3 & move /Y \"{tempFilePath}\" \"{AppContext.BaseDirectory}KTruckGui.exe\" & start \"\" \"{AppContext.BaseDirectory}KTruckGui.exe\"",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);

                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDashboardData()
        {
            // Placeholder for loading dashboard data
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Close the application
            System.Windows.Application.Current.Shutdown();
        }

        private void OpenVehicleWindow_Click(object sender, RoutedEventArgs e)
        {
            // Open Vehicles Window
            var vehicleWindow = new VehicleWindow();
            vehicleWindow.Show();
        }

        private void OpenCustomersScreen_Click(object sender, RoutedEventArgs e)
        {
            // Open Customer Window
            var customerWindow = new CustomerWindow(); // Assuming CustomerWindow class exists
            customerWindow.Show();
        }

        private void OpenInvoicesScreen_Click(object sender, RoutedEventArgs e)
        {
            // Open Invoices Window
            var invoiceWindow = new InvoiceWindow(); // Assuming InvoiceWindow class exists
            invoiceWindow.Show();
        }

        private void OpenPartsWindow_Click(object sender, RoutedEventArgs e)
        {
            // Open Parts Window
            var partsWindow = new PartsWindow(); // Assuming PartsWindow class exists
            partsWindow.Show();
        }


        private void OpenJobsScreen_Click(object sender, RoutedEventArgs e)
        {
            // Code to open jobs screen
        }

        private void OpenInventoryScreen_Click(object sender, RoutedEventArgs e)
        {
            // Code to open inventory screen
        }

        private void OpenAgingReport_Click(object sender, RoutedEventArgs e)
        {
            // Code to open aging report
        }

        private void OpenRevenueReport_Click(object sender, RoutedEventArgs e)
        {
            // Code to open revenue report
        }


        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void NewJob_Click(object sender, RoutedEventArgs e)
        {
            // Open the New Job Window
            var jobWindow = new WorkOrdersWindow();
            jobWindow.Show();
        }

        private void NewInvoice_Click(object sender, RoutedEventArgs e)
        {
            // Open the New Invoice Window
            var invoiceWindow = new InvoiceWindow();
            invoiceWindow.Show();
        }

        private void NewCustomer_Click(object sender, RoutedEventArgs e)
        {
            // Open the New Customer Window
            var customerFormWindow = new CustomerForm();
            customerFormWindow.Show();
        }

        private void CreateWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            var workOrdersWindow = new WorkOrdersWindow();  
            workOrdersWindow.ShowDialog();
        }
    }
}

using SuntoryManagementSystem.Services;
using System.Diagnostics;
using System.Windows;
using System;
using System.IO;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem
{
    public partial class ReportsWindow : Window
    {
        private readonly SuntoryDbContext _context;
        private readonly ReportService _reportService;

        public ReportsWindow()
        {
            InitializeComponent();
            _context = new SuntoryDbContext();
            _reportService = new ReportService(_context);
        }

        private void btnInventoryReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _reportService.GenerateInventoryReport();
                OpenReport(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij genereren van rapport: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnStockMovementReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _reportService.GenerateStockMovementReport();
                OpenReport(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij genereren van rapport: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDeliveryReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _reportService.GenerateDeliveryReport();
                OpenReport(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij genereren van rapport: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnStockAlertReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _reportService.GenerateStockAlertReport();
                OpenReport(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij genereren van rapport: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnFinancialReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _reportService.GenerateFinancialReport();
                OpenReport(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij genereren van rapport: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnComprehensiveReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _reportService.GenerateComprehensiveReport();
                OpenReport(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij genereren van rapport: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenReport(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                    
                    MessageBox.Show(
                        "Rapport succesvol gegenereerd!\n\n" +
                        $"Bestand: {Path.GetFileName(filePath)}\n" +
                        $"Locatie: Mijn Documenten > Suntory Reports",
                        "Succes",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij openen van rapport: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}

using System.Windows;
using System.Windows.Media;

namespace SuntoryManagementSystem
{
    public partial class StockAdjustmentTypeDialog : Window
    {
        public string AdjustmentType { get; private set; } = "Removal";
        public int QuantityChange { get; private set; }

        public StockAdjustmentTypeDialog(int quantityChange)
        {
            InitializeComponent();
            QuantityChange = quantityChange;
            txtQuantityChange.Text = Math.Abs(quantityChange).ToString();
            
            // Set initial color (Removal is checked by default)
            UpdateHeaderColors();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateHeaderColors();
        }

        private void UpdateHeaderColors()
        {
            // Determine which RadioButton is checked
            if (rbRemoval.IsChecked == true)
            {
                // ROOD voor Removal
                headerBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
                headerBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                headerTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
                headerText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
            }
            else if (rbDamage.IsChecked == true || rbTheft.IsChecked == true)
            {
                // ORANJE voor Damage en Theft
                headerBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                headerBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                headerTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100"));
                headerText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100"));
            }
            else if (rbCorrection.IsChecked == true)
            {
                // BLAUW voor Correction
                headerBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
                headerBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
                headerTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                headerText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (rbRemoval.IsChecked == true)
                AdjustmentType = "Removal";
            else if (rbDamage.IsChecked == true)
                AdjustmentType = "Damage";
            else if (rbTheft.IsChecked == true)
                AdjustmentType = "Theft";
            else if (rbCorrection.IsChecked == true)
                AdjustmentType = "Correction";

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

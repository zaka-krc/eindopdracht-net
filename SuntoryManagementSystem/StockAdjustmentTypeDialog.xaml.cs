using System.Windows;

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

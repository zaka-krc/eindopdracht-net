using System;
using System.Windows;

namespace SuntoryManagementSystem
{
    public partial class StockAdditionTypeDialog : Window
    {
        public string AdjustmentType { get; private set; } = "Addition";
        public int QuantityChange { get; private set; }

        public StockAdditionTypeDialog(int quantityChange)
        {
            InitializeComponent();
            QuantityChange = quantityChange;
            txtQuantityChange.Text = Math.Abs(quantityChange).ToString();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (rbAddition.IsChecked == true)
                AdjustmentType = "Addition";
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

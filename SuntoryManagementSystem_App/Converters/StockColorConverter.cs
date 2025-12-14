using System.Globalization;

namespace SuntoryManagementSystem_App.Converters;

public class StockColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 || values[0] is not int stockQuantity || values[1] is not int minimumStock)
        {
            return Colors.Gray;
        }

        // Out of stock
        if (stockQuantity == 0)
        {
            return Color.FromArgb("#DC2626"); // Red
        }

        // Critical (less than half of minimum)
        if (stockQuantity < minimumStock / 2)
        {
            return Color.FromArgb("#F59E0B"); // Orange
        }

        // Low stock (below minimum)
        if (stockQuantity < minimumStock)
        {
            return Color.FromArgb("#EAB308"); // Yellow
        }

        // Good stock
        return Color.FromArgb("#16A34A"); // Green
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

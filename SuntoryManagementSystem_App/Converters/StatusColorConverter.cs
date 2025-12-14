using System.Globalization;

namespace SuntoryManagementSystem_App.Converters;

public class StatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() switch
            {
                "active" => Color.FromArgb("#28a745"),     // Green
                "inactive" => Color.FromArgb("#6c757d"),   // Gray
                "gepland" => Color.FromArgb("#007bff"),    // Blue
                "delivered" => Color.FromArgb("#28a745"),  // Green
                "geannuleerd" => Color.FromArgb("#dc3545"),// Red
                _ => Color.FromArgb("#6c757d")             // Default Gray
            };
        }
        return Color.FromArgb("#6c757d");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

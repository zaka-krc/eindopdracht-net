using System.Globalization;

namespace SuntoryManagementSystem_App.Converters;

/// <summary>
/// Converter voor het toewijzen van kleuren aan delivery status badges
/// </summary>
public class DeliveryStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status switch
            {
                "Gepland" => Color.FromArgb("#F59E0B"),    // Oranje
                "Delivered" => Color.FromArgb("#16A34A"),   // Groen
                "Geannuleerd" => Color.FromArgb("#DC2626"), // Rood
                _ => Color.FromArgb("#6B7280")              // Grijs (default)
            };
        }
        
        return Color.FromArgb("#6B7280"); // Default grijs
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

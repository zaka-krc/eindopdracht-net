// ============================================================================
// SUNTORY MANAGEMENT SYSTEM (SMS)
// BoolToStatusConverter.cs - Converter voor Boolean naar Status tekst
// ============================================================================

using System;
using System.Globalization;
using System.Windows.Data;

namespace SuntoryManagementSystem
{
    /// <summary>
    /// Value Converter die een Boolean waarde omzet naar een status tekst.
    /// Gebruikt voor het tonen van "Actief"/"Inactief" of "Beschikbaar"/"Niet Beschikbaar" in de UI.
    /// </summary>
    public class BoolToStatusConverter : IValueConverter
    {
        /// <summary>
        /// De tekst die wordt getoond wanneer de boolean waarde True is.
        /// Standaard: "Actief"
        /// </summary>
        public string TrueValue { get; set; } = "Actief";

        /// <summary>
        /// De tekst die wordt getoond wanneer de boolean waarde False is.
        /// Standaard: "Inactief"
        /// </summary>
        public string FalseValue { get; set; } = "Inactief";

        /// <summary>
        /// Converteert een Boolean waarde naar een string status.
        /// </summary>
        /// <param name="value">De boolean waarde (true/false)</param>
        /// <param name="targetType">Het doel type (niet gebruikt)</param>
        /// <param name="parameter">Extra parameter (niet gebruikt)</param>
        /// <param name="culture">Cultuur informatie (niet gebruikt)</param>
        /// <returns>TrueValue bij true, FalseValue bij false</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        /// <summary>
        /// Converteert terug van string naar boolean (niet geïmplementeerd).
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is niet ondersteund voor BoolToStatusConverter");
        }
    }
}

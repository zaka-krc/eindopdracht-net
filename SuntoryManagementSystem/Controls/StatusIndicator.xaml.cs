// ============================================================================
// SUNTORY MANAGEMENT SYSTEM (SMS)
// StatusIndicator.xaml.cs - Custom UserControl voor Status Badges
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SuntoryManagementSystem.Controls
{
    /// <summary>
    /// Custom UserControl voor het weergeven van status badges met automatische kleurcodering.
    /// Dit control toont een status met een gekleurde badge op basis van de status waarde.
    /// 
    /// Ondersteunde statussen:
    /// - Active/Actief: Groen
    /// - Inactive/Inactief: Rood
    /// - Gepland/Planned: Blauw
    /// - Delivered/Geleverd: Groen
    /// - Cancelled/Geannuleerd: Rood
    /// - In Transit/Onderweg: Oranje
    /// - Default: Grijs
    /// </summary>
    public partial class StatusIndicator : UserControl
    {
        // Dependency Property voor Status
        // Dit maakt data binding mogelijk vanuit XAML
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
                nameof(Status), 
                typeof(string), 
                typeof(StatusIndicator),
                new PropertyMetadata(string.Empty, OnStatusChanged));

        /// <summary>
        /// De status tekst die wordt weergegeven.
        /// Kan gebonden worden aan een property in het data model.
        /// </summary>
        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        /// <summary>
        /// Constructor - initialiseert het control
        /// </summary>
        public StatusIndicator()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Callback die wordt aangeroepen wanneer de Status property wijzigt.
        /// </summary>
        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatusIndicator control)
            {
                control.UpdateStatus();
            }
        }

        /// <summary>
        /// Update de visuele weergave op basis van de huidige status.
        /// Past automatisch de achtergrondkleur aan op basis van de status waarde.
        /// </summary>
        private void UpdateStatus()
        {
            StatusText.Text = Status;

            // Automatische kleurcodering op basis van status (case-insensitive)
            switch (Status?.ToLower())
            {
                // Groene statussen (positief/actief/beschikbaar)
                case "actief":
                case "active":
                case "geleverd":
                case "delivered":
                case "beschikbaar":
                case "available":
                    StatusBorder.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Material Green
                    break;

                // Blauwe statussen (gepland/in behandeling)
                case "gepland":
                case "planned":
                    StatusBorder.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Material Blue
                    break;

                // Rode statussen (negatief/inactief/niet beschikbaar)
                case "geannuleerd":
                case "cancelled":
                case "inactief":
                case "inactive":
                case "niet beschikbaar":
                case "unavailable":
                    StatusBorder.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Material Red
                    break;

                // Oranje statussen (waarschuwing/in transit)
                case "onderweg":
                case "in transit":
                    StatusBorder.Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Material Orange
                    break;

                // Standaard grijze status (onbekend)
                default:
                    StatusBorder.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Material Gray
                    break;
            }
        }
    }
}

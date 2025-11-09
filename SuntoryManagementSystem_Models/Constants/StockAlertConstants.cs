namespace SuntoryManagementSystem.Models.Constants
{
    /// <summary>
    /// Constants voor Stock Alert types en statuses
    /// </summary>
    public static class StockAlertConstants
    {
        /// <summary>
        /// Stock Alert Types - Typen voorraad waarschuwingen
        /// </summary>
        public static class Types
        {
            public const string LowStock = "Low Stock";
            public const string Critical = "Critical";
            public const string OutOfStock = "Out of Stock";
        }

        /// <summary>
        /// Stock Alert Statuses - Status van de waarschuwing
        /// </summary>
        public static class Status
        {
            public const string Active = "Active";
            public const string Resolved = "Resolved";
        }
    }
}

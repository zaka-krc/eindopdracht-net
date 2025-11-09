namespace SuntoryManagementSystem.Models.Constants
{
    /// <summary>
    /// Constants voor Delivery types en statuses
    /// </summary>
    public static class DeliveryConstants
    {
        /// <summary>
        /// Delivery Types - Type levering
        /// </summary>
        public static class Types
        {
            public const string Incoming = "Incoming";
            public const string Outgoing = "Outgoing";
        }

        /// <summary>
        /// Delivery Statuses - Status van de levering
        /// </summary>
        public static class Status
        {
            public const string Planned = "Gepland";
            public const string Delivered = "Delivered";
            public const string Cancelled = "Geannuleerd";
        }
    }
}

namespace SuntoryManagementSystem_App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes for navigation
            Routing.RegisterRoute(nameof(Pages.ProductDetailPage), typeof(Pages.ProductDetailPage));
            Routing.RegisterRoute(nameof(Pages.DeliveryDetailPage), typeof(Pages.DeliveryDetailPage));
            Routing.RegisterRoute(nameof(Pages.CustomerDetailPage), typeof(Pages.CustomerDetailPage));
        }
    }
}

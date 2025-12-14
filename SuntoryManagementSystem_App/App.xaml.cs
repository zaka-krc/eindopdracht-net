using SuntoryManagementSystem_App.Services;

namespace SuntoryManagementSystem_App
{
    public partial class App : Application
    {
        public App(DatabaseService databaseService)
        {
            InitializeComponent();
            
            // Initialiseer database (seeding gebeurt automatisch)
            databaseService.InitializeAsync().Wait();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
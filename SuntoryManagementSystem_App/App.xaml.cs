using SuntoryManagementSystem_App.Services;
using System.Diagnostics;

namespace SuntoryManagementSystem_App
{
    public partial class App : Application
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;
        private readonly SyncService _syncService;
        private readonly ConnectivityService _connectivityService;

        public App(
            DatabaseService databaseService,
            AuthService authService,
            SyncService syncService,
            ConnectivityService connectivityService)
        {
            InitializeComponent();
            
            _databaseService = databaseService;
            _authService = authService;
            _syncService = syncService;
            _connectivityService = connectivityService;
            
            // Initialiseer database
            _databaseService.InitializeAsync().Wait();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override async void OnStart()
        {
            base.OnStart();
            
            Debug.WriteLine("========================================");
            Debug.WriteLine("APP STARTING - AUTO RE-AUTHENTICATION");
            Debug.WriteLine("========================================");

            try
            {
                // STAP 1: Check authentication
                bool isAuthenticated = await _authService.IsAuthenticatedAsync();
                
                if (isAuthenticated)
                {
                    Debug.WriteLine("✅ User is authenticated");
                    
                    // STAP 2: Trigger initial sync if online
                    if (_connectivityService.IsConnected)
                    {
                        Debug.WriteLine("📶 Online - triggering initial sync...");
                        
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var result = await _syncService.SyncAllAsync();
                                if (result.Success)
                                {
                                    Debug.WriteLine("✅ Initial sync completed");
                                }
                                else
                                {
                                    Debug.WriteLine($"⚠️ Initial sync completed with warnings: {result.Message}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"❌ Sync error: {ex.Message}");
                            }
                        });
                    }
                    else
                    {
                        Debug.WriteLine("📵 Offline - sync skipped");
                    }
                }
                else
                {
                    Debug.WriteLine("❌ User NOT authenticated");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ OnStart error: {ex.Message}");
            }
            
            Debug.WriteLine("========================================\n");
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            Debug.WriteLine("App: OnSleep");
        }

        protected override async void OnResume()
        {
            base.OnResume();
            Debug.WriteLine("App: OnResume");
            
            try
            {
                bool isAuthenticated = await _authService.IsAuthenticatedAsync();
                
                if (isAuthenticated && _connectivityService.IsConnected)
                {
                    Debug.WriteLine("📶 App resumed and online - triggering sync...");
                    _ = Task.Run(async () => 
                    {
                        var result = await _syncService.SyncAllAsync();
                        if (result.Success)
                        {
                            Debug.WriteLine("✅ Resume sync completed");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ OnResume error: {ex.Message}");
            }
        }
    }
}
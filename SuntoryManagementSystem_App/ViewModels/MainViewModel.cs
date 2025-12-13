using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace SuntoryManagementSystem_App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // User info
    [ObservableProperty]
    private string gebruikersNaam = "Admin";
    
    [ObservableProperty]
    private string gebruikersRol = "Administrator";
    
    // Sync info
    [ObservableProperty]
    private string laatsteSyncTijd = "Nog niet gesynchroniseerd";
    
    [ObservableProperty]
    private bool isSyncing = false;
    
    // Dashboard stats
    [ObservableProperty]
    private int aantalProducten = 0;
    
    [ObservableProperty]
    private int lageVoorraadCount = 0;
    
    [ObservableProperty]
    private int openstaandeLeveringen = 0;
    
    [ObservableProperty]
    private int actieveKlanten = 0;
    
    // Waarschuwingen lijst
    [ObservableProperty]
    private ObservableCollection<string> voorraadWaarschuwingen;
    
    // Constructor
    public MainViewModel()
    {
        // Dummy data voor nu
        VoorraadWaarschuwingen = new ObservableCollection<string>
        {
            "Coca-Cola 0.5L - 5 stuks resterend",
            "Sprite 1L - 2 stuks resterend", 
            "Fanta Orange - 8 stuks resterend"
        };
        
        LoadDummyData();
    }
    
    // Commands
    [RelayCommand]
    private async Task RefreshData()
    {
        IsSyncing = true;
        
        // TODO: Later vervangen door echte synchronisatie
        await Task.Delay(1000); // Simuleer API call
        
        LaatsteSyncTijd = DateTime.Now.ToString("HH:mm");
        LoadDummyData();
        
        IsSyncing = false;
    }
    
    [RelayCommand]
    private async Task NieuwProduct()
    {
        // TODO: Navigeer naar ProductDetailPage
        await Shell.Current.DisplayAlert("Info", "Nieuw Product - Coming soon!", "OK");
    }
    
    [RelayCommand]
    private async Task OpenRapporten()
    {
        // TODO: Navigeer naar RapportenPage
        await Shell.Current.DisplayAlert("Info", "Rapporten - Coming soon!", "OK");
    }
    
    // Helper methods
    private void LoadDummyData()
    {
        // Dummy stats
        AantalProducten = 47;
        LageVoorraadCount = 3;
        OpenstaandeLeveringen = 5;
        ActieveKlanten = 12;
    }
}

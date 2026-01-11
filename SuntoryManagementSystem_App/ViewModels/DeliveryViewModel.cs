using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_App.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.ViewModels;

public partial class DeliveryViewModel : ObservableObject
{
    private readonly LocalDbContext _context;

    [ObservableProperty]
    private ObservableCollection<Delivery> deliveries = new();

    [ObservableProperty]
    private ObservableCollection<Delivery> gefilterdeLeveringen = new();

    [ObservableProperty]
    private string zoekTekst = string.Empty;

    [ObservableProperty]
    private string geselecteerdType = "Alle";

    [ObservableProperty]
    private string geselecteerdeStatus = "Alle";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool isRefreshing = false;

    public DeliveryViewModel(LocalDbContext context)
    {
        _context = context;
        _ = LoadLeveringenAsync();
    }

    private async Task LoadLeveringenAsync()
    {
        if (IsLoading && !IsRefreshing) return;

        try
        {
            if (!IsRefreshing)
                IsLoading = true;

            Debug.WriteLine("LoadLeveringenAsync: Starting...");

            // Haal leveringen op inclusief relaties
            var leveringen = await _context.Deliveries
                .Include(d => d.Supplier)
                .Include(d => d.Customer)
                .Include(d => d.Vehicle)
                .Include(d => d.DeliveryItems!)
                    .ThenInclude(di => di.Product)
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.ExpectedDeliveryDate)
                .ToListAsync();

            Debug.WriteLine($"LoadLeveringenAsync: Loaded {leveringen.Count} deliveries");

            Deliveries = new ObservableCollection<Delivery>(leveringen);
            FilterLeveringen();

            Debug.WriteLine($"LoadLeveringenAsync: GefilterdeLeveringen has {GefilterdeLeveringen.Count} items");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadLeveringenAsync ERROR: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Kan leveringen niet laden: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    partial void OnZoekTekstChanged(string value)
    {
        FilterLeveringen();
    }

    partial void OnGeselecteerdTypeChanged(string value)
    {
        FilterLeveringen();
    }

    partial void OnGeselecteerdeStatusChanged(string value)
    {
        FilterLeveringen();
    }

    private void FilterLeveringen()
    {
        try
        {
            var filtered = Deliveries.AsEnumerable();

            // Filter op zoektekst
            if (!string.IsNullOrWhiteSpace(ZoekTekst))
            {
                var searchLower = ZoekTekst.ToLower();
                filtered = filtered.Where(d =>
                    d.ReferenceNumber.ToLower().Contains(searchLower) ||
                    (d.Notes != null && d.Notes.ToLower().Contains(searchLower)) ||
                    d.PartnerName.ToLower().Contains(searchLower));
            }

            // Filter op type
            if (GeselecteerdType != "Alle")
            {
                filtered = filtered.Where(d => d.DeliveryType == GeselecteerdType);
            }

            // Filter op status
            if (GeselecteerdeStatus != "Alle")
            {
                filtered = filtered.Where(d => d.Status == GeselecteerdeStatus);
            }

            GefilterdeLeveringen = new ObservableCollection<Delivery>(filtered);
            Debug.WriteLine($"FilterLeveringen: {GefilterdeLeveringen.Count} filtered items");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FilterLeveringen ERROR: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task VoegLeveringToe()
    {
        try
        {
            Debug.WriteLine("VoegLeveringToe: Navigating to DeliveryDetailPage");
            await Shell.Current.GoToAsync(nameof(Pages.DeliveryDetailPage));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"VoegLeveringToe ERROR: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Kan niet navigeren: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task BekijkLevering(Delivery delivery)
    {
        if (delivery == null) return;

        try
        {
            Debug.WriteLine($"BekijkLevering: Navigating with DeliveryId={delivery.DeliveryId} in View mode");
            await Shell.Current.GoToAsync($"{nameof(Pages.DeliveryDetailPage)}?DeliveryId={delivery.DeliveryId}&ViewMode=true");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BekijkLevering ERROR: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Kan niet navigeren: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task BewerkLevering(Delivery delivery)
    {
        if (delivery == null) return;

        try
        {
            Debug.WriteLine($"BewerkLevering: Navigating with DeliveryId={delivery.DeliveryId} in Edit mode");
            await Shell.Current.GoToAsync($"{nameof(Pages.DeliveryDetailPage)}?DeliveryId={delivery.DeliveryId}&ViewMode=false");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BewerkLevering ERROR: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Kan niet navigeren: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task VerwijderLevering(Delivery delivery)
    {
        if (delivery == null) return;

        bool bevestiging = await Shell.Current.DisplayAlert(
            "Verwijderen",
            $"Weet je zeker dat je levering '{delivery.ReferenceNumber}' wilt verwijderen?",
            "Ja",
            "Nee");

        if (bevestiging)
        {
            try
            {
                delivery.IsDeleted = true;
                delivery.DeletedDate = DateTime.Now;

                _context.Deliveries.Update(delivery);
                await _context.SaveChangesAsync();

                // Reload data to refresh the list
                await LoadLeveringenAsync();

                await Shell.Current.DisplayAlert("Succes", "Levering verwijderd!", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VerwijderLevering ERROR: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Fout bij verwijderen: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        await LoadLeveringenAsync();
    }
}

# SUNTORY MANAGEMENT SYSTEM

## Projectbeschrijving

Het **Suntory Management System** is een multi-platform applicatie suite voor het beheren van voorraden, leveringen, klanten en leveranciers voor Suntory drankendistributie. Het systeem bestaat uit drie componenten:
- **WPF Desktop App** - Windows desktop applicatie voor kantoorpersoneel
- **Web App (Razor Pages)** - Web-based beheerinterface met ASP.NET Core Identity
- **.NET MAUI Mobile App** - Cross-platform mobiele app voor iOS/Android met offline synchronisatie

Het systeem biedt gebruikersbeheer met verschillende rollen, realtime voorraadtracking, uitgebreide rapportage functionaliteit en offline werkmodus voor de mobiele app.

- Auteur: Zakaria Korchi
- School: Erasmushogeschool Brussel
- Schooljaar: 2025-2026

---

## Projectstructuur

### 1. WPF Desktop Applicatie (`SuntoryManagementSystem_WPF`)
Windows desktop applicatie met volledige CRUD functionaliteit.

### 2. Web Applicatie (`SuntoryManagementSystem_Web`)
ASP.NET Core Razor Pages applicatie met:
- ASP.NET Core Identity authenticatie
- Razor Pages UI
- RESTful API Controllers voor mobiele app
- Multi-language support (Nederlands/Engels)
- Admin dashboard

### 3. .NET MAUI Mobile App (`SuntoryManagementSystem_App`)
Cross-platform mobiele applicatie met:
- Offline-first architectuur
- SQLite lokale database
- Automatische synchronisatie
- Responsive UI voor iOS/Android/Windows

### 4. Shared Models (`SuntoryManagementSystem_Models`)
Gedeelde data models en Entity Framework Core context voor alle projecten.

---

## Functionaliteiten

### Voorraadbeheer
- **Product CRUD** met real-time voorraadtracking
- **Automatische waarschuwingen** bij lage voorraad
- **Voorraadcorrecties** met categorie en reden:
  - Addition, Removal, Damage, Theft, Correction
- **Voorraad historie** (StockAdjustment log met datum/tijd)
- **Minimum stock alerts** met automatische resolutie
- **Kleurcodering** op basis van voorraadniveau (groen/oranje/rood)

### Leveringenbeheer
- **Inkomende leveringen** (van leveranciers)
- **Uitgaande leveringen** (naar klanten)
- **Delivery processing**:
  - Validatie: voorraadcontrole bij uitgaande leveringen
  - Automatische voorraadupdate na verwerken
  - Status tracking: Gepland → Delivered/Geannuleerd
- **Multi-item deliveries** met totaalbedrag berekening
- **Leveringen filteren** op type, status, partner

### CRM Functionaliteit
- **Klantenbeheer**: contactgegevens, type (Retail/Wholesale/Restaurant)
- **Leveranciersbeheer**: contactpersonen, status (Active/Inactive)
- **Status badges** met kleurcodering (groen/rood)
- **Zoeken en filteren** op naam, email, telefoon

### Voertuigenbeheer
- Kenteken, merk, model, capaciteit
- Beschikbaarheidsstatus
- Link naar leveringen (optioneel)

### Rapportage (Excel via ClosedXML - WPF only)
Alle rapporten worden opgeslagen in: `Documenten\Suntory Reports\`

1. **Inventarisrapport** - Actuele voorraad met waarden
2. **Voorraadbewegingen** - Alle toevoegingen/verwijderingen
3. **Leveringen** - In/uitgaande leveringen met status
4. **Voorraadwaarschuwingen** - Lage/kritieke stock items
5. **Financieel** - Inkoopwaarde, verkoopwaarde, marge analyse
6. **Volledig Rapport** - Complete overzicht met statistieken

### Gebruikersbeheer
- **Rol management**: Administrator/Manager/Employee toewijzen
- **Account activeren/deactiveren**
- **Wachtwoord reset** (standaard: `Reset@123`)
- **Laatste login tracking**
- **ASP.NET Core Identity** integratie in web app

### Mobile App Features (.NET MAUI)
- **Offline-first architectuur**: Werk zonder internetverbinding
- **Lokale SQLite database**: Alle data lokaal opgeslagen
- **Automatische synchronisatie**: 
  - Achtergrond sync elke 5 minuten
  - Pull-to-refresh handmatig
  - Sync indicator in UI
- **ConnectivityService**: Automatische detectie van netwerkverbinding
- **SyncService**: Bidirectionele data synchronisatie met Web API
- **AuthService**: JWT token-based authenticatie
- **Responsive UI**: Geoptimaliseerd voor touch input

---

## Gebruikersrollen

### Administrator
- Volledige toegang tot alle functionaliteit
- Gebruikersbeheer (rollen toewijzen, accounts blokkeren)
- Wachtwoorden resetten
- Admin dashboard (Web App)

### Manager
- Alle operationele tabs
- CRUD operaties voor alle entiteiten
- Geen gebruikersbeheer
- Rapport generatie (WPF)

### Employee
- Alleen-lezen toegang in WPF
- Beperkte bewerkingsrechten in Web/Mobile
- Rapport generatie mogelijk

### Guest (niet ingelogd)
- Alleen-lezen toegang (WPF)
- Geen toegang tot Web/Mobile

---

## Techniek Stack

### Frontend
- **WPF**: .NET 9, C# 13, XAML
- **Web**: ASP.NET Core Razor Pages, Bootstrap 5
- **Mobile**: .NET MAUI (Android/iOS/Windows/macOS)

### Backend
- **Entity Framework Core 9** met SQL Server LocalDB (Web/WPF)
- **SQLite** lokale database (Mobile)
- **ASP.NET Core Identity** voor authenticatie
- **RESTful Web API** voor mobile communicatie

### Libraries & Packages
- **ClosedXML** (0.104.2) - Excel rapportage (WPF)
- **Microsoft.EntityFrameworkCore** (9.0.0)
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (9.0.0)
- **System.Net.Http** - HTTP client voor API calls (Mobile)
- **Newtonsoft.Json** / **System.Text.Json** - JSON serialisatie

### Architecture
- **MVVM Pattern** (Mobile/WPF)
- **Repository Pattern** (Web API Controllers)
- **Dependency Injection** (alle projecten)
- **Soft Delete** op alle entiteiten
- **Offline-First** architectuur (Mobile)

---

## Project Structuur (Detailed)

```
SuntoryManagementSystem/
├─ SuntoryManagementSystem/                 # WPF Desktop App
│  ├─ MainWindow.xaml(.cs)                  # Hoofdvenster met tabs
│  ├─ LoginWindow.xaml(.cs)                 # Inlogscherm
│  ├─ RegisterWindow.xaml(.cs)              # Registratiescherm
│  ├─ ReportsWindow.xaml(.cs)               # Rapportage menu
│  ├─ Dialogs/
│  │  ├─ CustomerDialog.xaml(.cs)
│  │  ├─ SupplierDialog.xaml(.cs)
│  │  ├─ ProductDialog.xaml(.cs)
│  │  ├─ DeliveryDialog.xaml(.cs)
│  │  ├─ VehicleDialog.xaml(.cs)
│  │  ├─ UserRolesDialog.xaml(.cs)
│  │  ├─ StockAdjustmentTypeDialog.xaml
│  │  └─ StockAdditionTypeDialog.xaml
│  ├─ Controls/
│  │  └─ StatusIndicator.xaml(.cs)          # Custom status badges
│  ├─ Converters/
│  │  └─ BoolToStatusConverter.cs
│  ├─ Services/
│  │  └─ ReportService.cs                   # Excel generatie
│  └─ App.xaml(.cs)
│
├─ SuntoryManagementSystem_Web/             # ASP.NET Core Web App
│  ├─ Controllers/                          # MVC Controllers voor UI
│  │  ├─ HomeController.cs
│  │  ├─ CustomerController.cs
│  │  ├─ DeliveriesController.cs
│  │  ├─ ProductsController.cs
│  │  ├─ SuppliersController.cs
│  │  ├─ VehiclesController.cs
│  │  ├─ AdminController.cs
│  │  └─ UserProfileController.cs
│  ├─ API_Controllers/                      # RESTful API voor Mobile
│  │  ├─ AuthController.cs                  # JWT authenticatie
│  │  ├─ ProductsController.cs
│  │  ├─ CustomersController.cs
│  │  ├─ DeliveriesController.cs
│  │  ├─ DeliveryItemsController.cs
│  │  ├─ SuppliersController.cs
│  │  ├─ VehiclesController.cs
│  │  ├─ StockAlertsController.cs
│  │  └─ StockAdjustmentsController.cs
│  ├─ Areas/Identity/Pages/Account/         # ASP.NET Identity Pages
│  │  ├─ Login.cshtml(.cs)
│  │  ├─ Register.cshtml(.cs)
│  │  ├─ Logout.cshtml(.cs)
│  │  └─ Manage/                            # Account management
│  ├─ Views/                                # Razor Views
│  ├─ Services/
│  │  └─ SharedResource.cs                  # Localization
│  └─ Program.cs                            # App configuration
│
├─ SuntoryManagementSystem_App/             # .NET MAUI Mobile App
│  ├─ Pages/
│  │  ├─ MainPage.xaml(.cs)                 # Dashboard
│  │  ├─ ProductenPage.xaml(.cs)            # Producten lijst
│  │  ├─ ProductDetailPage.xaml(.cs)        # Product details
│  │  ├─ CustomerPage.xaml(.cs)             # Klanten lijst
│  │  ├─ CustomerDetailPage.xaml(.cs)       # Klant details
│  │  ├─ DeliveryPage.xaml(.cs)             # Leveringen lijst
│  │  ├─ DeliveryDetailPage.xaml(.cs)       # Levering details
│  │  └─ SettingsPage.xaml(.cs)             # App instellingen
│  ├─ ViewModels/
│  │  ├─ MainViewModel.cs
│  │  ├─ ProductenViewModel.cs
│  │  ├─ CustomerViewModel.cs
│  │  └─ DeliveryViewModel.cs
│  ├─ Services/
│  │  ├─ ApiService.cs                      # REST API client
│  │  ├─ AuthService.cs                     # JWT authenticatie
│  │  ├─ DatabaseService.cs                 # SQLite lokale DB
│  │  ├─ SyncService.cs                     # Sync logica
│  │  ├─ ConnectivityService.cs             # Netwerk detectie
│  │  ├─ HttpClientFactory.cs               # HTTP client factory
│  │  └─ ApiSettings.cs                     # API configuratie
│  ├─ Converters/
│  │  ├─ InvertedBoolConverter.cs
│  │  ├─ StatusColorConverter.cs
│  │  ├─ StockColorConverter.cs
│  │  └─ DeliveryStatusColorConverter.cs
│  ├─ AppShell.xaml(.cs)                    # App navigation shell
│  └─ MauiProgram.cs                        # DI configuratie
│
└─ SuntoryManagementSystem_Models/          # Shared Domain Models
   ├─ Constants/
   │  ├─ CustomerConstants.cs
   │  ├─ DeliveryConstants.cs
   │  ├─ StatusConstants.cs
   │  ├─ StockAdjustmentConstants.cs
   │  └─ StockAlertConstants.cs
   ├─ Migrations/                           # EF Core migrations
   ├─ ApplicationUser.cs                    # Custom Identity user
   ├─ Customer.cs
   ├─ Delivery.cs
   ├─ DeliveryItem.cs
   ├─ Product.cs
   ├─ StockAdjustment.cs
   ├─ StockAlert.cs
   ├─ Supplier.cs
   ├─ Vehicle.cs
   └─ SuntoryDbContext.cs                   # EF Core DbContext
```

---

## Installatie en Starten

### Vereisten
- **Windows 10/11** (voor WPF)
- **.NET 9 SDK**
- **SQL Server LocalDB** (automatisch met Visual Studio)
- **Visual Studio 2022** of later
- **Android SDK** (voor MAUI Android deployment)

### Stappen

1. **Clone repository**:
   ```bash
   git clone https://github.com/zaka-krc/eindopdracht-net.git
   cd eindopdracht-net
   ```

2. **Open solution** in Visual Studio 2022

3. **Database migraties uitvoeren**:
   - Open Package Manager Console
   - Selecteer `SuntoryManagementSystem_Models` als Default Project
   - Run: `Update-Database`

4. **Start Web API** (voor Mobile App):
   - Rechtsklik op `SuntoryManagementSystem_Web`
   - Selecteer "Set as Startup Project"
   - Druk F5 om te starten
   - Noteer de URL (bijv. `https://localhost:7123`)

5. **Configureer Mobile App**:
   - Open `SuntoryManagementSystem_App/Services/ApiSettings.cs`
   - Update `BaseUrl` naar de Web API URL
   - Voor Android emulator: gebruik `10.0.2.2` i.p.v. `localhost`

6. **Start gewenste applicatie**:
   - **WPF**: Set `SuntoryManagementSystem` als startup project
   - **Web**: Set `SuntoryManagementSystem_Web` als startup project
   - **Mobile**: Set `SuntoryManagementSystem_App` als startup project
   - Druk F5

---

## Testaccounts

| Email               | Wachtwoord   | Rol          |
|---------------------|--------------|--------------|
| admin@suntory.com   | Admin@123    | Administrator|
| manager@suntory.com | Manager@123  | Manager      |
| employee@suntory.com| Employee@123 | Employee     |

Nieuwe registraties krijgen automatisch de rol **Employee**.

---

## API Endpoints (Mobile App)

### Authenticatie
- `POST /api/auth/login` - Login met email/wachtwoord
- `POST /api/auth/register` - Nieuwe gebruiker registreren

### Producten
- `GET /api/products` - Alle producten ophalen
- `GET /api/products/{id}` - Enkel product
- `POST /api/products` - Nieuw product
- `PUT /api/products/{id}` - Product bijwerken
- `DELETE /api/products/{id}` - Product verwijderen

### Klanten
- `GET /api/customers` - Alle klanten
- `POST /api/customers` - Nieuwe klant
- `PUT /api/customers/{id}` - Klant bijwerken
- `DELETE /api/customers/{id}` - Klant verwijderen

### Leveringen
- `GET /api/deliveries` - Alle leveringen
- `POST /api/deliveries` - Nieuwe levering
- `PUT /api/deliveries/{id}` - Levering bijwerken
- `DELETE /api/deliveries/{id}` - Levering verwijderen

_Volledig REST API met CRUD operaties voor alle entiteiten_

---

## Synchronisatie Architectuur

### Offline-First Workflow
1. **App Start**: Laad data uit lokale SQLite database
2. **Connectiviteit Check**: Controleer internetverbinding
3. **Achtergrond Sync**: Auto-sync elke 5 minuten als online
4. **Handmatige Sync**: Pull-to-refresh op elke pagina
5. **Conflict Resolution**: Laatste wijziging wint (last-write-wins)

### Sync Proces
1. **Pull van server**: Download nieuwe/gewijzigde data via API
2. **Merge lokaal**: Update lokale SQLite met server data
3. **Push naar server**: Upload lokale wijzigingen
4. **UI Update**: Refresh lists met nieuwe data

### Connectivity Service
- Real-time netwerk monitoring
- Automatische retry bij verbindingsverlies
- Visuele indicator in UI (sync icon)

---

## NuGet Packages

### Shared (Models)
| Package | Versie | Gebruik |
|---------|--------|---------|
| Microsoft.EntityFrameworkCore | 9.0.0 | Database ORM |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.0 | SQL Server provider |
| Microsoft.EntityFrameworkCore.Tools | 9.0.0 | EF Core CLI tools |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 9.0.0 | Identity framework |

### WPF Desktop
| Package | Versie | Gebruik |
|---------|--------|---------|
| ClosedXML | 0.104.2 | Excel rapportage |
| Microsoft.Extensions.DependencyInjection | 9.0.0 | Dependency injection |

### Web Application
| Package | Versie | Gebruik |
|---------|--------|---------|
| Microsoft.AspNetCore.Identity.UI | 9.0.0 | Identity scaffolding |
| Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation | 9.0.0 | Razor hot reload |

### Mobile App (.NET MAUI)
| Package | Versie | Gebruik |
|---------|--------|---------|
| sqlite-net-pcl | 1.9.172 | SQLite lokale DB |
| Newtonsoft.Json | 13.0.3 | JSON serialisatie |
| Microsoft.Maui.Controls | 9.0.0 | MAUI framework |
| Microsoft.Maui.Graphics | 9.0.0 | Graphics API |

**Licenties**: Alle packages zijn MIT licensed

---

## AI-gegenereerde Code

Dit project maakt gebruik van **GitHub Copilot** voor:
- Code completion en suggesties
- LINQ query optimalisaties
- Lambda expressie syntaxis
- REST API endpoint implementaties
- XML/XAML documentatie
- Try-catch error handling patronen
- Async/await implementaties
- UI layout suggesties
- Data binding expressions

### AI Gebruik Verklaring

**Alle AI-gegenereerde code is**:
- Handmatig gereviewd en getest
- Aangepast aan project requirements
- Volledig begrepen en gedocumenteerd
- Functioneel en geïntegreerd
- Getest op alle platforms

**Voorbeelden van AI-assistentie**:
- LINQ queries (Method & Query Syntax)
- Lambda expressies voor data filtering
- Async/await HTTP client calls
- Entity Framework queries
- XAML data binding syntaxis
- Converters en Value Converters
- Navigation patterns in MAUI
- REST API controller methods
- SQLite CRUD operations
- Deze README

---

## Features per Platform

| Feature | WPF | Web | Mobile |
|---------|-----|-----|--------|
| Product CRUD | ✅ | ✅ | ✅ |
| Klanten CRUD | ✅ | ✅ | ✅ |
| Leveringen CRUD | ✅ | ✅ | ✅ |
| Voorraad Alerts | ✅ | ✅ | ✅ |
| Excel Rapporten | ✅ | ❌ | ❌ |
| Gebruikersbeheer | ✅ | ✅ | ❌ |
| Offline Mode | ❌ | ❌ | ✅ |
| Auto Sync | ❌ | ❌ | ✅ |
| Multi-language | ❌ | ✅ | ❌ |
| Touch Optimized | ❌ | ❌ | ✅ |

---

## Known Issues & Limitations

### Mobile App
- Sync conflicts worden opgelost via "last-write-wins"
- Geen real-time push notifications (polling only)
- Beperkte offline conflictresolutie
- Geen attachments/foto's support (nog)

### Web App
- Excel export nog niet geïmplementeerd
- Beperkte data visualisatie (geen charts)

### WPF App
- Windows-only platform
- Geen cloud synchronisatie

---


---

## Licenties & Copyright

- **EF Core/Identity**: MIT License
- **ClosedXML**: MIT License
- **.NET MAUI**: MIT License

**Copyright**: ©2025 Suntory Global Spirits Inc., New York, NY. Alle rechten voorbehouden.

**Disclaimer**: Dit is een educatief project en niet bedoeld voor commercieel gebruik.

---

## Contact

- **Auteur**: Zakaria Korchi
- **Email**: zakariakorchi2003@hotmail.com
- **GitHub**: [zaka-krc](https://github.com/zaka-krc)
- **Repository**: [eindopdracht-net](https://github.com/zaka-krc/eindopdracht-net)

---

## Changelog

### v2.0 (Huidig)
- ✅ .NET MAUI mobile app toegevoegd
- ✅ Offline-first architectuur met SQLite
- ✅ REST API voor mobile communicatie
- ✅ Automatische synchronisatie
- ✅ JWT authenticatie voor API
- ✅ Connectivity monitoring
- ✅ Pull-to-refresh op alle lijsten

### v1.0 (Initieel)
- ✅ WPF desktop applicatie
- ✅ Web applicatie met Razor Pages
- ✅ ASP.NET Core Identity
- ✅ Entity Framework Core
- ✅ Excel rapportage
- ✅ Gebruikersbeheer met rollen

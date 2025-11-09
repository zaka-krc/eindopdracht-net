# SUNTORY MANAGEMENT SYSTEM (WPF)

## Projectbeschrijving

Het **Suntory Management System** is een WPF desktop applicatie voor het beheren van voorraden, leveringen, klanten en leveranciers voor Suntory drankendistributie. Het systeem biedt gebruikersbeheer met verschillende rollen, realtime voorraadtracking en uitgebreide rapportage functionaliteit.

- Auteur: Zakaria Korchi
- School: Erasmushogeschool Brussel
- Schooljaar: 2025-2026

## Functionaliteiten

### 🗄️ Voorraadbeheer
- **Product CRUD** met real-time voorraadtracking
- **Automatische waarschuwingen** bij lage voorraad
- **Voorraadcorrecties** met categorie en reden:
  - Addition, Removal, Damage, Theft, Correction
- **Voorraad historie** (StockAdjustment log met datum/tijd)
- **Minimum stock alerts** met automatische resolutie

### Leveringenbeheer
- **Inkomende leveringen** (van leveranciers)
- **Uitgaande leveringen** (naar klanten)
- **Delivery processing**:
  - Validatie: voorraadcontrole bij uitgaande leveringen
  - Automatische voorraadupdate na verwerken
  - Status tracking: Gepland → Delivered/Geannuleerd
- **Multi-item deliveries** met totaalbedrag berekening

### CRM Functionaliteit
- **Klantenbeheer**: contactgegevens, type (Retail/Wholesale/Restaurant)
- **Leveranciersbeheer**: contactpersonen, status (Active/Inactive)
- **Status badges** met kleurcodering (groen/rood)

### Voertuigenbeheer
- Kenteken, merk, model, capaciteit
- Beschikbaarheidsstatus
- Link naar leveringen (optioneel)

### Rapportage (Excel via ClosedXML)
Alle rapporten worden opgeslagen in: `Documenten\Suntory Reports\`

1. **Inventarisrapport** - Actuele voorraad met waarden
2. **Voorraadbewegingen** - Alle toevoegingen/verwijderingen
3. **Leveringen** - In/uitgaande leveringen met status
4. **Voorraadwaarschuwingen** - Lage/kritieke stock items
5. **Financieel** - Inkoopwaarde, verkoopwaarde, marge analyse
6. **Volledig Rapport** - Complete overzicht met statistieken

### Gebruikersbeheer (Administrator only)
- **Rol management**: Administrator/Manager/Employee toewijzen
- **Account activeren/deactiveren**
- **Wachtwoord reset** (standaard: `Reset@123`)
- **Laatste login tracking**

### UI Features
- **Custom StatusIndicator** badges met kleurcodering:

- **BoolToStatusConverter** voor "Actief"/"Inactief" conversie
- **User info popup** met rollen en laatste login
- **ComboBox status selectie** (moderne UI vs checkboxes)
- **Responsive DataGrids** met sorting en filtering

- 
## Gebruikersrollen

### Administrator
- Volledige toegang tot alle functionaliteit
- Gebruikersbeheer (rollen toewijzen, accounts blokkeren)
- Wachtwoorden resetten

### Manager
- Alle operationele tabs
- CRUD operaties voor alle entiteiten
- Geen gebruikersbeheer

### Employee
- Alleen-lezen toegang
- Rapport generatie mogelijk


### Guest (niet ingelogd)
- Alleen-lezen toegang
- Geen wijzigingen mogelijk

## Techniek
- .NET 9, C# 13, WPF (XAML)
- Entity Framework Core + SQL Server LocalDB (migrations, seeding)
- ASP.NET Core Identity met custom `ApplicationUser` (rollen/claims)
- ClosedXML voor Excel-rapporten
- Soft delete op entiteiten (`IsDeleted`, `DeletedDate`)

SuntoryManagementSystem/
├─ SuntoryManagementSystem/                 # WPF Desktop App (UI Layer)
│  ├─ MainWindow.xaml(.cs)                  # Hoofdvenster met tabs
│  ├─ LoginWindow.xaml(.cs)                 # Inlogscherm
│  ├─ RegisterWindow.xaml(.cs)              # Registratiescherm
│  ├─ ReportsWindow.xaml(.cs)               # Rapportage menu
│  ├─ Dialogs/
│  │  ├─ CustomerDialog.xaml(.cs)           # Klant CRUD
│  │  ├─ SupplierDialog.xaml(.cs)           # Leverancier CRUD
│  │  ├─ ProductDialog.xaml(.cs)            # Product CRUD met stock
│  │  ├─ DeliveryDialog.xaml(.cs)           # Levering CRUD
│  │  ├─ VehicleDialog.xaml(.cs)            # Voertuig CRUD
│  │  ├─ UserRolesDialog.xaml(.cs)          # Rol toewijzing (Admin)
│  │  ├─ StockAdjustmentTypeDialog.xaml     # Voorraad verminderen
│  │  └─ StockAdditionTypeDialog.xaml       # Voorraad toevoegen
│  ├─ Controls/
│  │  └─ StatusIndicator.xaml(.cs)          # Custom status badges
│  ├─ Converters/
│  │  └─ BoolToStatusConverter.cs           # Boolean → "Actief"/"Inactief"
│  ├─ Services/
│  │  └─ ReportService.cs                   # Excel generatie (ClosedXML)
│  └─ App.xaml(.cs)                         # WPF app resources & styles
│
├─ SuntoryManagementSystem_Models/          # Domain Models + EF Core
│  ├─ Constants/
│  │  ├─ CustomerConstants.cs
│  │  ├─ DeliveryConstants.cs
│  │  ├─ StatusConstants.cs
│  │  ├─ StockAdjustmentConstants.cs
│  │  └─ StockAlertConstants.cs
│  ├─ Migrations/
│  │  ├─ 20251016205143_InitialCreate.cs
│  │  ├─ 20251105131713_AddSoftDeleteToAllModels.cs
│  │  ├─ 20251105133613_AddCustomerAndDeliveryType.cs
│  │  ├─ 20251108223543_AddIdentityFramework.cs
│  │  └─ SuntoryDbContextModelSnapshot.cs
│  ├─ ApplicationUser.cs                    # Custom Identity user
│  ├─ Customer.cs                           # Klant entiteit
│  ├─ Delivery.cs                           # Levering entiteit
│  ├─ DeliveryItem.cs                       # Levering items (junction)
│  ├─ Product.cs                            # Product entiteit
│  ├─ StockAdjustment.cs                    # Voorraad wijzigingen log
│  ├─ StockAlert.cs                         # Voorraad waarschuwingen
│  ├─ Supplier.cs                           # Leverancier entiteit
│  ├─ Vehicle.cs                            # Voertuig entiteit
│  └─ SuntoryDbContext.cs                   # EF Core DbContext + Seeding
│
└─ SuntoryManagementSystem_Cons/            # Console testapp (optioneel)


## Installatie en starten
1) Vereisten: Windows 10/11, .NET 9 SDK, SQL Server LocalDB, Visual Studio 2022+
2) Clone repo en open solution
3) Database migraties uitvoeren (indien van toepassing): `Update-Database`
4) Stel `SuntoryManagementSystem` in als startup project en start (F5)

## Testaccounts
| Email               | Wachtwoord   | Rol          |
|---------------------|--------------|--------------|
| admin@suntory.com   | Admin@123    | Administrator|
| manager@suntory.com | Manager@123  | Manager      |
| employee@suntory.com| Employee@123 | Employee     |

Nieuwe registraties krijgen automatisch de rol Employee.

## NuGet Packages

| Package | Versie | Licentie | Gebruik |
|---------|--------|----------|---------|
| **Microsoft.EntityFrameworkCore** | 9.0.0 | MIT | Database ORM framework |
| **Microsoft.EntityFrameworkCore.SqlServer** | 9.0.0 | MIT | SQL Server database provider |
| **Microsoft.EntityFrameworkCore.Tools** | 9.0.0 | MIT | EF Core CLI tools (Package Manager Console) |
| **Microsoft.AspNetCore.Identity.EntityFrameworkCore** | 9.0.0 | MIT | Gebruikersauthenticatie en rollenbeheer |
| **ClosedXML** | 0.104.2 | MIT | Excel rapportage en exports |

**Database**: SQL Server LocalDB (automatisch aangemaakt bij eerste start)

# AI-gegenereerde Code

Dit project maakt gebruik van **GitHub Copilot** voor:
- Code completion en suggesties
- LINQ query optimalisaties
- Lambda expressie syntaxis
- XML commentaar documentatie
- Try-catch foutafhandelings patronen
- UI layout suggesties

### AI Gebruik Verklaring

**Alle AI-gegenereerde code is**:
- Handmatig gereviewd en getest
- Aangepast aan project requirements
- Volledig begrepen en gedocumenteerd
- Functioneel en geïntegreerd

**Voorbeelden van AI-assistentie**:
- LINQ queries (Method & Query Syntax)
- Lambda expressies voor data filtering
- Async/await implementaties
- Entity Framework queries
- XAML data binding syntaxis
- Try-catch error handling

**Disclaimer**: Dit is een educatief project en niet bedoeld voor commercieel gebruik. Alle rechten op externe libraries behoren toe aan hun respectievelijke eigenaars.

## Licenties
- EF Core/Identity: MIT
- ClosedXML: MIT

## Copyright
©2025 Suntory Global Spirits Inc., New York, NY. Alle rechten voorbehouden.

## Contact
- Mail: zakariakorchi2003@hotmail.com

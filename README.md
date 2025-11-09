# SUNTORY MANAGEMENT SYSTEM

## Projectbeschrijving

Het **Suntory Management System** is een WPF desktop applicatie voor het beheren van voorraden, leveringen, klanten en leveranciers voor Suntory drankendistributie. Het systeem biedt gebruikersbeheer met verschillende rollen, realtime voorraadtracking en uitgebreide rapportage functionaliteit.

### Hoofdfuncties
- **Voorraadbeheer** - Producten, voorraad tracking en waarschuwingen
- **Leveringenbeheer** - Inkomende/uitgaande leveringen met status tracking
- **Klanten & Leveranciers** - Complete CRM functionaliteit
- **Voertuigenbeheer** - Fleet management voor distributie
- **Gebruikersbeheer** - Identity Framework met rollen (Admin, Manager, Employee)
- **Rapportages** - Excel exports voor analyse

## Technische Specificaties

- **.NET Version**: .NET 9.0
- **C# Version**: 13.0
- **Framework**: WPF (Windows Presentation Foundation)
- **Database**: SQL Server LocalDB met Entity Framework Core
- **Identity**: ASP.NET Core Identity Framework
- **UI**: XAML met custom styles en data binding

## Project Structuur

```
SuntoryManagementSystem/
??? SuntoryManagementSystem/          # WPF Desktop Applicatie
?   ??? MainWindow.xaml/cs            # Hoofd applicatie window
?   ??? LoginWindow.xaml/cs           # Login functionaliteit
?   ??? RegisterWindow.xaml/cs        # Gebruikersregistratie
?   ??? Controls/                     # Custom UserControls
?   ?   ??? UserInfoCard.xaml/cs      # Gebruiker info display
?   ??? Dialogs/                      # CRUD dialogs
?   ?   ??? SupplierDialog.xaml/cs
?   ?   ??? CustomerDialog.xaml/cs
?   ?   ??? ProductDialog.xaml/cs
?   ?   ??? DeliveryDialog.xaml/cs
?   ?   ??? VehicleDialog.xaml/cs
?   ?   ??? UserRolesDialog.xaml/cs
?   ??? Services/                     # Business logic
?       ??? ReportService.cs          # Excel rapportage
??? SuntoryManagementSystem_Models/   # Models & Database Context
?   ??? Models/                       # Entity classes
?   ?   ??? ApplicationUser.cs        # Custom Identity User
?   ?   ??? Supplier.cs
?   ?   ??? Customer.cs
?   ?   ??? Product.cs
?   ?   ??? Vehicle.cs
?   ?   ??? Delivery.cs
?   ?   ??? DeliveryItem.cs
?   ?   ??? StockAdjustment.cs
?   ?   ??? StockAlert.cs
?   ??? Constants/                    # Business constants
?   ??? SuntoryDbContext.cs           # EF Core DbContext
?   ??? Migrations/                   # Database migraties
??? SuntoryManagementSystem_Cons/     # Console test applicatie
```

## Database Schema

### Entiteiten (8 tabellen)
1. **Suppliers** - Leveranciers met contactgegevens
2. **Customers** - Klanten (Retail/Wholesale)
3. **Products** - Producten met voorraad en prijzen
4. **Vehicles** - Voertuigen voor leveringen
5. **Deliveries** - Leveringen (Incoming/Outgoing)
6. **DeliveryItems** - Producten per levering
7. **StockAdjustments** - Voorraadwijzigingen logging
8. **StockAlerts** - Automatische waarschuwingen

### Relaties
- Product ? Supplier (Many-to-One)
- Delivery ? Customer/Supplier (Many-to-One)
- Delivery ? Vehicle (Many-to-One)
- DeliveryItem ? Product + Delivery (Many-to-One)
- StockAdjustment ? Product (Many-to-One)
- StockAlert ? Product (Many-to-One)

### Soft Delete
Alle entiteiten hebben soft delete functionaliteit met:
- `IsDeleted` (bool)
- `DeletedDate` (DateTime?)

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
- Beperkte toegang
- Geen delete rechten
- Geen voertuigenbeheer

### Guest (niet ingelogd)
- Alleen-lezen toegang
- Geen wijzigingen mogelijk

## Installatie & Gebruik

### Vereisten
- Windows 10/11
- .NET 9.0 SDK
- SQL Server LocalDB (of SQL Server)
- Visual Studio 2022

### Setup
1. **Clone de repository**
   ```bash
   git clone https://github.com/zaka-krc/eindopdracht-net.git
   cd eindopdracht-net
   ```

2. **Database initialiseren**
   ```bash
   # Open Package Manager Console in Visual Studio
   Update-Database
   ```

3. **Start de applicatie**
   - Set `SuntoryManagementSystem` als startup project
   - Druk op F5

### Test Accounts

| Email | Wachtwoord | Rol |
|-------|-----------|-----|
| admin@suntory.com | Admin@123 | Administrator |
| manager@suntory.com | Manager@123 | Manager |
| employee@suntory.com | Employee@123 | Employee |

## Gebruikte Libraries & Licenties

### NuGet Packages

| Package | Versie | Licentie | Gebruik |
|---------|--------|----------|---------|
| **Microsoft.EntityFrameworkCore** | 9.0.0 | MIT | Database ORM |
| **Microsoft.EntityFrameworkCore.SqlServer** | 9.0.0 | MIT | SQL Server provider |
| **Microsoft.EntityFrameworkCore.Tools** | 9.0.0 | MIT | Migratie tools |
| **Microsoft.AspNetCore.Identity.EntityFrameworkCore** | 9.0.0 | MIT | Gebruikersbeheer |
| **EPPlus** | 7.5.2 | Polyform Noncommercial | Excel exports |

### Licentie Details

#### Microsoft Packages (MIT License)
```
Copyright (c) .NET Foundation and Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

#### EPPlus (Polyform Noncommercial License)
EPPlus wordt gebruikt onder de Noncommercial licentie voor **educatieve doeleinden**.
- **Licentie**: https://polyformproject.org/licenses/noncommercial/1.0.0/
- **EPPlus Info**: https://www.epplussoftware.com/

**Nota**: Dit project is uitsluitend voor educatief gebruik. Voor commercieel gebruik is een EPPlus licentie vereist.

## AI-gegenereerde Code

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

**Belangrijke opmerking**: Hoewel AI tools zijn gebruikt voor code suggesties, is alle code:
1. Zelfstandig geschreven met AI als hulpmiddel
2. Volledig begrepen en kunnen worden uitgelegd
3. Aangepast aan de specifieke requirements van het project
4. Getest en geverifieerd op correcte werking

## Educatief Project

Dit project is ontwikkeld als eindopdracht voor **Erasmus Hogeschool Brussel (EHB)** - .NET Development cursus.

- **Student**: Zakarya Karouach
- **Academiejaar**: 2024-2025
- **Cursus**: Advanced .NET Development
- **GitHub**: https://github.com/zaka-krc/eindopdracht-net

## Vereisten Checklist

### Verplichte Functionaliteit
- [x] .NET 9.0 WPF Desktop applicatie
- [x] Solution met Models library + WPF project
- [x] Minimum 3 tabellen met relaties (8 geïmplementeerd)
- [x] Entity Framework met migrations & seeding
- [x] Soft delete op alle entiteiten
- [x] Identity Framework met custom User klasse
- [x] Registratie, login, logout
- [x] Minimum 2 rollen (3 geïmplementeerd: Admin, Manager, Employee)
- [x] Rolgebaseerd menu/tabs
- [x] CRUD operaties voor alle entiteiten
- [x] Minimum 2 selectievelden (5+ ComboBoxes geïmplementeerd)
- [x] Minimum 3 containers (6+ types: TabControl, DataGrid, StackPanel, Grid, Border, ScrollViewer)
- [x] Menu/tab structuur (TabControl met 8 tabs)
- [x] Extra popup windows (9+ dialogs geïmplementeerd)
- [x] Styles in XAML (App.xaml met uitgebreide styles)
- [x] Data binding (systematisch in alle views)
- [x] Custom UserControl (UserInfoCard met DependencyProperties)
- [x] LINQ Query Syntax & Method Syntax
- [x] Lambda expressies
- [x] Try-Catch foutafhandeling
- [x] Waarschuwingsboodschappen
- [x] Foutloos opstarten

### Bonus Features
- [x] Excel rapportage (EPPlus)
- [x] Uitgebreide seeding data
- [x] Console test applicatie
- [x] Geavanceerde voorraad tracking
- [x] Audit trail (StockAdjustments)
- [x] Real-time waarschuwingen

## Code Voorbeelden

### LINQ Query Syntax
```csharp
// MainWindow.xaml.cs - Gebruikersrollen ophalen
var userRoles = (from ur in _context.UserRoles
                 where ur.UserId == loginWindow.LoggedInUser.Id
                 join r in _context.Roles on ur.RoleId equals r.Id
                 select r.Name).ToList();

// Administrator check
bool isAdmin = (from role in _userRoles
               where role == "Administrator"
               select role).Any();
```

### LINQ Method Syntax
```csharp
// MainWindow.xaml.cs - Leveranciers laden
dgSuppliers.ItemsSource = await _context.Suppliers
    .Where(s => !s.IsDeleted)
    .OrderBy(s => s.Status == StatusConstants.Active ? 0 : 1)
    .ThenBy(s => s.SupplierName)
    .ToListAsync();
```

### Lambda Expressies
```csharp
// RegisterWindow.xaml.cs - Wachtwoord validatie
if (!password.Any(char.IsUpper))
    ShowError("Wachtwoord moet minimaal 1 hoofdletter bevatten.");

if (!password.Any(char.IsDigit))
    ShowError("Wachtwoord moet minimaal 1 cijfer bevatten.");

// Event handler met lambda
Loaded += async (s, e) => await LoadAllDataAsync();
```

### Try-Catch Foutafhandeling
```csharp
try
{
    _context.Users.Add(newUser);
    await _context.SaveChangesAsync();
    DialogResult = true;
}
catch (Exception ex)
{
    ShowError($"Fout bij registratie: {ex.Message}");
}
```

### Custom UserControl (DependencyProperties)
```csharp
// UserInfoCard.xaml.cs
public static readonly DependencyProperty FullNameProperty =
    DependencyProperty.Register("FullName", typeof(string), typeof(UserInfoCard),
        new PropertyMetadata(string.Empty, OnFullNameChanged));

public string FullName
{
    get => (string)GetValue(FullNameProperty);
    set => SetValue(FullNameProperty, value);
}
```

## Bekende Limitaties

- LocalDB vereist Windows omgeving
- EPPlus vereist .NET Framework compatibiliteit
- Excel rapportage werkt alleen op Windows

## Toekomstige Uitbreidingen

- Web API backend voor multi-platform toegang
- Blazor WebAssembly frontend
- Barcode scanning voor voorraad
- Automatische email notificaties
- Dashboard met grafieken (LiveCharts)

## Contact & Support

**Repository**: https://github.com/zaka-krc/eindopdracht-net

Voor vragen of problemen, open een issue op GitHub.

---

## Copyright & Licentie

Copyright 2024-2025 Zakarya Karouach - Erasmus Hogeschool Brussel

Dit project is ontwikkeld voor educatieve doeleinden. Alle code is origineel geschreven met ondersteuning van GitHub Copilot. 
Externe libraries worden gebruikt volgens hun respectievelijke licenties (zie sectie "Gebruikte Libraries & Licenties").

**Disclaimer**: Dit is een educatief project en niet bedoeld voor commercieel gebruik. Alle rechten op externe libraries behoren toe aan hun respectievelijke eigenaars.

---

**Project Status**: ? Voltooid en klaar voor indiening
**Laatste Update**: December 2024

### 4. README.md Toegevoegd
**Inhoud**:
- Complete projectbeschrijving
- Technische specificaties
- Database schema (8 tabellen)
- Gebruikersrollen documentatie
- Installatie instructies met test accounts
- NuGet packages met licentie informatie
- AI-gebruik verklaring
- Code voorbeelden (LINQ, Lambda, Try-Catch)
- Vereisten checklist (alle afgevinkt)
- Copyright & educatieve disclaimer

### 5. UserInfoPopup Aangemaakt (Vervanging UserInfoCard)
**Features**:
- Moderne popup window met gebruikersdetails
- Compacte user info button in header (geen grote card)
- Popup toont volledige informatie:
  - Gebruikersnaam en rol
  - E-mailadres
  - Afdeling
  - Laatste login datum
- Auto-positionering near button
- Auto-close bij click buiten popup
- Professional design met shadow en rounded corners

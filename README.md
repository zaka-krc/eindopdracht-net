# SUNTORY MANAGEMENT SYSTEM (WPF)

## Projectbeschrijving

Het **Suntory Management System** is een WPF desktop applicatie voor het beheren van voorraden, leveringen, klanten en leveranciers voor Suntory drankendistributie. Het systeem biedt gebruikersbeheer met verschillende rollen, realtime voorraadtracking en uitgebreide rapportage functionaliteit.

- Auteur: Zakaria Korchi
- School: Erasmushogeschool Brussel
- Schooljaar: 2025-2026

## Functionaliteiten
- Voorraadbeheer: producten, minimumstock, waarschuwingen en correcties
- Leveringen: inkomend/uitgaand, status, verwerken met voorraadimpact
- Klanten en leveranciers (basis-CRM)
- Voertuigenbeheer
- Gebruikersbeheer met rollen: Administrator, Manager, Employee, Guest (gast = alleen lezen)
- Rapportage: Excel-export (ClosedXML)

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

## Projectstructuur
```
SuntoryManagementSystem/
├─ SuntoryManagementSystem/                # WPF-app (UI)
│  ├─ MainWindow.xaml(.cs), Login/ Register
│  ├─ Dialogs/                             # CRUD-dialogen
│  ├─ Services/ ReportService.cs           # Excel-rapporten (ClosedXML)
│  └─ Controls & Popups (UserInfoPopup)
├─ SuntoryManagementSystem_Models/         # Domein + EF Core
│  ├─ Models/ (Product, Supplier, Customer, Delivery, DeliveryItem, Vehicle, StockAdjustment, StockAlert)
│  ├─ Constants/ (DeliveryConstants, StatusConstants, ...)
│  └─ SuntoryDbContext.cs
└─ SuntoryManagementSystem_Cons/           # Console testapp
```

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

### NuGet Packages

| Package | Versie | Licentie | Gebruik |
|---------|--------|----------|---------|
| **Microsoft.EntityFrameworkCore** | 9.0.0 | MIT | Database ORM |
| **Microsoft.EntityFrameworkCore.SqlServer** | 9.0.0 | MIT | SQL Server provider |
| **Microsoft.EntityFrameworkCore.Tools** | 9.0.0 | MIT | Migratie tools |
| **Microsoft.AspNetCore.Identity.EntityFrameworkCore** | 9.0.0 | MIT | Gebruikersbeheer |
| **EPPlus** | 7.5.2 | Polyform Noncommercial | Excel exports |

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

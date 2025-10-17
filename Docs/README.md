# 💼 EconomicWPF - Regnskabssystem

[![.NET](https://img.shields.io/badge/.NET-7.0-512BD4)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-0078D4)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927)](https://www.microsoft.com/sql-server)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📋 Beskrivelse

EconomicWPF er et komplet Windows-baseret regnskabssystem udviklet i C# med WPF teknologi. Systemet er inspireret af e-conomic og indeholder omfattende funktionalitet til økonomistyring for små og mellemstore virksomheder.

## ✨ Hovedfunktioner

### 🧾 Fakturering
- ✅ Opret og send salgsfakturaer
- ✅ Kundestyring (Debitorbogholderi)
- ✅ Varekartotek med priser
- ✅ Design og skabeloner
- ✅ Rykkersystem

### 📊 Finansiering
- ✅ Finansbogholderi
- ✅ Leverandørstyring (Kreditorbogholderi)
- ✅ Bankafstemning
- ✅ Momsopgørelse
- ✅ Periodisering og anlæg

### 📈 Rapportering
- ✅ Resultatopgørelse
- ✅ Balance
- ✅ Saldobalance
- ✅ Kontokort
- ✅ Debitorsaldoliste
- ✅ Kreditorsaldoliste
- ✅ Omsætningsstatistik
- ✅ Nøgletal

### 🎯 Avancerede funktioner
- ✅ Budgetstyring
- ✅ Projektstyring og tidsregistrering
- ✅ Dimensioner (afdelinger, projekter)
- ✅ Valutahåndtering
- ✅ Import/Export af data

## 🏗️ Arkitektur

Projektet følger best practices og moderne arkitekturprincipper:

- **MVVM Pattern**: Klar separation mellem UI, logik og data
- **SOLID Principper**: Objektorienteret design
- **Repository Pattern**: Datahåndtering
- **Command Pattern**: UI interaktioner
- **SoC**: Separation of Concerns
- **Database Normalisering**: 1NF, 2NF, 3NF

## 📁 Projektstruktur

```
EconomicWPF/
│
├── 📂 Models/                  # POCO klasser
│   ├── Customer.cs
│   ├── Supplier.cs
│   ├── Invoice.cs
│   ├── Product.cs
│   └── ...
│
├── 📂 ViewModels/              # Business logic
│   ├── Base/
│   │   └── ViewModelBase.cs
│   ├── MainViewModel.cs
│   ├── CustomerViewModel.cs
│   ├── InvoiceViewModel.cs
│   └── ...
│
├── 📂 Views/                   # UI komponenter
│   ├── MainWindow.xaml
│   ├── Customers/
│   ├── Invoices/
│   ├── Reports/
│   └── ...
│
├── 📂 Commands/                # ICommand implementationer
│   ├── RelayCommand.cs
│   └── AsyncRelayCommand.cs
│
├── 📂 Repositories/            # Data access layer
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── ICustomerRepository.cs
│   │   └── ...
│   └── Implementation/
│       ├── CustomerRepository.cs
│       ├── InvoiceRepository.cs
│       └── ...
│
├── 📂 Database/                # Database scripts
│   ├── CreateDatabase.sql
│   └── SeedData.sql
│
├── 📂 Docs/                    # Dokumentation
│   ├── DomainModel.mermaid
│   └── DatabaseDiagram.mermaid
│
│
├── appsettings.json            # Konfiguration
└── App.xaml                    # Application entry
```

## 🚀 Installation

### Forudsætninger

- Windows 10/11
- .NET 7.0 SDK eller nyere
- SQL Server 2019+ eller SQL Server Express
- Visual Studio 2022 (anbefalet)

### Trin 1: Clone Repository

```bash
git clone https://github.com/ditbrugernavn/EconomicWPF.git
cd EconomicWPF
```

### Trin 2: Database Setup

1. Åbn SQL Server Management Studio
2. Kør `Database/CreateDatabase.sql`
3. Opdater connection string i `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EconomicDB;Trusted_Connection=True;"
  }
}
```

### Trin 3: Build & Run

```bash
dotnet restore
dotnet build
dotnet run
```

## 💾 Database Schema

Databasen følger 3. normalform (3NF) og ACID principper:

- **Customers** - Kundeoplysninger
- **Suppliers** - Leverandøroplysninger
- **Products** - Varekartotek
- **Invoices** - Fakturaer
- **InvoiceLines** - Fakturalinjer
- **Accounts** - Kontoplan
- **Transactions** - Posteringer
- **Projects** - Projekter
- **Dimensions** - Dimensioner
- **Budgets** - Budgetter

Se `Docs/DatabaseDiagram.mermaid` for komplet ER-diagram.

## 🎨 UI/UX Features

- 🎨 Moderne, fladt design
- 🌙 Dark/Light theme support (fremtidig feature)
- 📱 Responsiv layout
- ⌨️ Keyboard shortcuts
- 🔍 Avanceret søgefunktionalitet
- 📊 Interaktive grafer og diagrammer

## 🧪 Testing

```bash
dotnet test
```

## 📚 Dokumentation

Komplet dokumentation findes i `Docs/` mappen:

- [Domain Model](Docs/DomainModel.mermaid)
- [Database Design](Docs/DatabaseDiagram.mermaid)
- [Bruger Guide](Docs/UserGuide.md)

## 🤝 Bidrag

Bidrag er velkomne! 

## 📄 Licens

Dette projekt er licenseret under MIT License - se [LICENSE](LICENSE) filen for detaljer.

## 👨‍💻 Forfatter

Dit Navn - [@codemikemike](https://github.com/codemikemike)

## 🙏 Anerkendelser

- Inspireret af e-conomic's funktionalitet
- WPF Community
- .NET Foundation

---

⭐ **Hvis dette projekt hjælper dig, giv det en stjerne på GitHub!** ⭐
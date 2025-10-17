# 🚀 EconomicWPF Installation Guide

## 📋 Forudsætninger

### Software Requirements
- ✅ **Windows 10/11** (64-bit)
- ✅ **Visual Studio 2022** (Community, Professional eller Enterprise)
  - Workload: ".NET Desktop Development"
- ✅ **SQL Server 2019+** eller **SQL Server Express**
- ✅ **SQL Server Management Studio (SSMS)**
- ✅ **.NET 7.0 SDK** eller nyere

### Valgfrit
- Git for version control
- GitHub Desktop for lettere Git håndtering

---

## 📥 Step 1: Download Projektet

### Option A: Via Git
```bash
git clone https://github.com/ditbrugernavn/EconomicWPF.git
cd EconomicWPF
```

### Option B: Download ZIP
1. Download projektet som ZIP fra GitHub
2. Udpak til en mappe (f.eks. `C:\Projects\EconomicWPF`)

---

## 🗄️ Step 2: Database Setup

### 2.1 Opret Database

1. Åbn **SQL Server Management Studio (SSMS)**

2. Connect til din SQL Server instance
   - Server name: `localhost` eller `.\SQLEXPRESS`
   - Authentication: Windows Authentication

3. Åbn filen `Database/CreateDatabase.sql`

4. Kør scriptet ved at trykke **F5** eller klik på **Execute**

5. Verificer at databasen er oprettet:
   ```sql
   USE EconomicDB;
   GO
   
   SELECT * FROM INFORMATION_SCHEMA.TABLES;
   ```

### 2.2 Find Connection String

Kør følgende query for at finde din connection string:

```sql
SELECT 
    'Server=' + @@SERVERNAME + 
    ';Database=EconomicDB;Integrated Security=true;TrustServerCertificate=True;' 
    AS ConnectionString
```

Kopier resultatet - du skal bruge det i næste step!

---

## 🔧 Step 3: Konfigurer Projektet

### 3.1 Åbn Projektet i Visual Studio

1. Dobbeltklik på `EconomicWPF.sln` filen
2. Visual Studio vil åbne projektet

### 3.2 Opdater Connection String

1. Åbn `appsettings.json` filen i Solution Explorer

2. Erstat connection string med den du kopierede fra Step 2.2:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EconomicDB;Integrated Security=true;TrustServerCertificate=True;"
  }
}
```

**Vigtigt:** Højreklik på `appsettings.json` → Properties → Sæt "Copy to Output Directory" til **"Copy if newer"**

### 3.3 Installer NuGet Packages

Visual Studio vil automatisk restore packages når du åbner projektet.

Hvis ikke, højreklik på Solution → **Restore NuGet Packages**

Eller via Package Manager Console:
```powershell
Update-Package -reinstall
```

---

## 🏗️ Step 4: Build Projektet

### Via Visual Studio
1. Vælg **Build** → **Build Solution** (Ctrl+Shift+B)
2. Check **Output** vinduet for at sikre der ingen fejl er

### Via Command Line
```bash
cd EconomicWPF
dotnet build
```

**Forventede Warnings:** Nogle warnings om nullable references er OK og kan ignoreres.

---

## ▶️ Step 5: Kør Applikationen

### Via Visual Studio
1. Tryk **F5** eller klik på den grønne **Start** knap
2. Applikationen skulle starte op og vise Dashboard

### Via Command Line
```bash
dotnet run
```

---

## ✅ Verificer Installation

Når applikationen starter, skulle du se:

1. ✅ **MainWindow** med navigation sidebar
2. ✅ **Dashboard** med statistik kort (alle vil være 0 indtil du tilføjer data)
3. ✅ Navigation virker til forskellige sider
4. ✅ Ingen fejlmeddelelser i konsollen

### Test Funktionalitet

1. **Gå til Kunder**
   - Klik "👥 Kunder" i sidebaren
   - Klik "➕ Ny Kunde"
   - Udfyld kundeoplysninger
   - Klik "💾 Gem"
   - Verificer at kunden vises i listen

2. **Gå til Varer**
   - Klik "📦 Varer" i sidebaren
   - Opret en test vare

3. **Dashboard**
   - Gå tilbage til Dashboard
   - Statistikkene skulle nu reflektere dine nye data

---

## 🐛 Troubleshooting

### Problem: "Cannot connect to database"

**Løsning:**
```sql
-- Verificer SQL Server kører
-- I SSMS, kør:
SELECT @@VERSION
```

Hvis SQL Server ikke kører:
- Start SQL Server fra Services (services.msc)
- Eller start SQL Server Configuration Manager

### Problem: "Login failed for user"

**Løsning:**
1. Check om du bruger korrekt authentication mode
2. Prøv at tilføje dit bruger login:
   ```sql
   USE EconomicDB;
   GO
   CREATE USER [DinWindovsUser] FOR LOGIN [DinDomain\DinWindovsUser];
   GO
   ALTER ROLE db_owner ADD MEMBER [DinWindovsUser];
   GO
   ```

### Problem: "appsettings.json not found"

**Løsning:**
1. Højreklik på `appsettings.json` i Solution Explorer
2. Properties
3. Sæt "Copy to Output Directory" til "Copy if newer"
4. Rebuild projektet

### Problem: "The type or namespace name 'X' could not be found"

**Løsning:**
```powershell
# I Package Manager Console
Update-Package -reinstall
```

### Problem: XAML fejl "Cannot locate resource"

**Løsning:**
1. Check at alle XAML filer har korrekt Build Action: "Page"
2. Check at ResourceDictionary paths er korrekte
3. Rebuild projektet

---

## 🔄 Genstart fra Scratch

Hvis du vil starte helt forfra:

### 1. Slet Database
```sql
USE master;
GO
DROP DATABASE EconomicDB;
GO
```

### 2. Kør CreateDatabase.sql igen

### 3. Clean og Rebuild i Visual Studio
- Build → Clean Solution
- Build → Rebuild Solution

---

## 📊 Tilføj Test Data (Optional)

Hvis du vil have testdata til at lege med:

```sql
USE EconomicDB;
GO

-- Tilføj test kunde
INSERT INTO Customers (CustomerNumber, Name, CVR, Email, Phone, City, Country, PaymentTermDays, IsActive, CreatedDate, ModifiedDate)
VALUES 
('K1001', 'Test Virksomhed A/S', '12345678', 'info@test.dk', '12345678', 'København', 'Danmark', 14, 1, GETDATE(), GETDATE());

-- Tilføj test produkt
INSERT INTO Products (ProductNumber, Name, Description, PurchasePrice, SalesPrice, VATRate, Unit, StockQuantity, ReorderLevel, AccountId, IsActive, CreatedDate, ModifiedDate)
VALUES 
('P1001', 'Test Produkt', 'Dette er et test produkt', 100.00, 150.00, 25.00, 'stk', 10, 5, 1, 1, GETDATE(), GETDATE());
```

---

## 🎉 Du er klar!

Projektet skulle nu være installeret og kørende!

### Næste Skridt

1. 📖 Læs **PROJECT_STRUCTURE.md** for at forstå kodebasen
2. 🎨 Eksperimenter med at tilføje/redigere kunder og varer
3. 💻 Start med at tilpasse projektet til dine behov
4. 📚 Check README.md for flere features og funktioner

### Hjælp og Support

- 📖 Check dokumentationen i `/Docs` mappen
- 🐛 Rapporter issues på GitHub
- 💬 Stil spørgsmål i GitHub Discussions

**God fornøjelse med EconomicWPF! 🚀**
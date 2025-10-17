# ⚡ Quick Start Guide - EconomicWPF

Kom i gang med EconomicWPF på 15 minutter!

## 📦 Step 1: Download & Extract (2 min)

```bash
# Clone eller download ZIP
git clone https://github.com/ditbrugernavn/EconomicWPF.git
cd EconomicWPF
```

## 🗄️ Step 2: Database (5 min)

1. Åbn **SQL Server Management Studio (SSMS)**
2. Connect til `localhost` eller `.\SQLEXPRESS`
3. Åbn `Database/CreateDatabase.sql`
4. Tryk **F5** for at køre scriptet
5. Verificer: `SELECT * FROM Customers` ✅

## 🔧 Step 3: Configuration (2 min)

1. Åbn `appsettings.json`
2. Opdater connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EconomicDB;Integrated Security=true;TrustServerCertificate=True;"
  }
}
```

3. **VIGTIGT**: Højreklik på `appsettings.json` → Properties → Sæt "Copy to Output Directory" til **"Copy if newer"**

## ▶️ Step 4: Run (1 min)

Åbn `EconomicWPF.sln` i Visual Studio og tryk **F5**

**🎉 Done! Applikationen skulle nu køre!**

---

## 🎯 First Steps Tutorial (5 min)

### 1️⃣ Opret Din Første Kunde

1. Klik **"👥 Kunder"** i sidebaren
2. Klik **"➕ Ny Kunde"**
3. Udfyld:
   - Kundenummer: `K1001` (auto-genereret)
   - Navn: `Test Virksomhed A/S`
   - Email: `info@test.dk`
   - By: `København`
4. Klik **"💾 Gem"**

✅ Din første kunde er nu oprettet!

### 2️⃣ Opret Et Produkt

1. Klik **"📦 Varer"** i sidebaren
2. Klik **"➕ Ny Vare"**
3. Udfyld:
   - Varenummer: `P1001` (auto-genereret)
   - Navn: `Konsulentydelse`
   - Salgspris: `500`
   - Moms: `25%`
4. Klik **"💾 Gem"**

✅ Dit første produkt er oprettet!

### 3️⃣ Opret Din Første Faktura

1. Klik **"🧾 Fakturaer"** i sidebaren
2. Klik **"➕ Ny Faktura"**
3. Vælg **kunde**: `Test Virksomhed A/S`
4. Klik **"➕ Tilføj linje"**
5. Vælg **vare**: `Konsulentydelse`
6. Antal: `2`
7. Klik **"💾 Gem"**

✅ Din første faktura er oprettet! Total: 1.250 kr (inkl. moms)

---

## 🎨 Explore Features

Nu hvor du har oprettet dine første data, prøv at:

### Dashboard 📊
- Se statistik opdateres real-time
- Seneste fakturaer vises
- Quick actions til hurtig adgang

### Rapporter 📈
- Resultatopgørelse
- Balance
- Omsætningsstatistik
- Kunde/leverandør lister

### Søgning 🔍
- Søg efter kunder i kundelisten
- Filter fakturaer efter status
- Find produkter

---

## 💡 Tips & Tricks

### ⌨️ Keyboard Shortcuts
- **Ctrl+N**: Ny kunde/faktura (afhængig af side)
- **Ctrl+S**: Gem
- **Ctrl+F**: Søg
- **Esc**: Annuller

### 🎯 Best Practices

1. **Start med Kunder**: Opret dine kunder først
2. **Derefter Varer**: Opret varekartotek
3. **Så Fakturaer**: Nu kan du fakturere nemt
4. **Check Dashboard**: Hold øje med nøgletal

### 🐛 Common Issues

**Problem**: "Cannot connect to database"
```sql
-- Test connection i SSMS
SELECT @@VERSION
```

**Problem**: "appsettings.json not found"
- Check "Copy to Output Directory" property

**Problem**: Views vises ikke
- Rebuild solution (Ctrl+Shift+B)
- Check namespaces er korrekte

---

## 📚 Next Steps

Nu hvor du er i gang:

1. 📖 Læs [README.md](README.md) for alle features
2. 🏗️ Check [PROJECT_STRUCTURE.md](Docs/PROJECT_STRUCTURE.md) for arkitektur
3. 🔧 Læs [INSTALLATION.md](Docs/INSTALLATION.md) for avanceret setup
4. 🤝 Læs [CONTRIBUTING.md](CONTRIBUTING.md) hvis du vil bidrage

---

## 🆘 Need Help?

- 📖 Full Documentation: [README.md](README.md)
- 🐛 Report Issues: [GitHub Issues](https://github.com/ditbrugernavn/EconomicWPF/issues)
- 💬 Ask Questions: [GitHub Discussions](https://github.com/ditbrugernavn/EconomicWPF/discussions)

---

## ✨ Demo Data

Vil du have testdata til at lege med?

```sql
USE EconomicDB;
GO

-- Testdata: Kunder
INSERT INTO Customers (CustomerNumber, Name, CVR, Email, City, Country, PaymentTermDays, IsActive, CreatedDate, ModifiedDate)
VALUES 
('K1001', 'Microsoft Danmark', '12345678', 'info@microsoft.dk', 'København', 'Danmark', 14, 1, GETDATE(), GETDATE()),
('K1002', 'Google Danmark', '87654321', 'kontakt@google.dk', 'Aarhus', 'Danmark', 30, 1, GETDATE(), GETDATE()),
('K1003', 'Apple Nordic', '11223344', 'sales@apple.dk', 'Odense', 'Danmark', 14, 1, GETDATE(), GETDATE());

-- Testdata: Produkter
INSERT INTO Products (ProductNumber, Name, Description, PurchasePrice, SalesPrice, VATRate, Unit, StockQuantity, ReorderLevel, AccountId, IsActive, CreatedDate, ModifiedDate)
VALUES 
('P1001', 'Konsulentydelse', 'Time konsulent', 400.00, 800.00, 25.00, 'time', 0, 0, 1, 1, GETDATE(), GETDATE()),
('P1002', 'Support Time', 'Support og vedligehold', 300.00, 600.00, 25.00, 'time', 0, 0, 1, 1, GETDATE(), GETDATE()),
('P1003', 'Projekt Management', 'Projektledelse', 500.00, 1000.00, 25.00, 'time', 0, 0, 1, 1, GETDATE(), GETDATE());

SELECT 'Test data inserted successfully!' AS Result;
```

---

**🎉 Tillykke! Du er nu i gang med EconomicWPF!**

Happy coding! 💻✨
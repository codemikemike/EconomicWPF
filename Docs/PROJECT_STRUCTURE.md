# 📁 EconomicWPF - Komplet Projekt Struktur

## 🗂️ Mappestruktur

```
EconomicWPF/
│
├── 📄 App.xaml
├── 📄 App.xaml.cs
├── 📄 appsettings.json
├── 📄 README.md
│
├── 📂 Commands/
│   ├── RelayCommand.cs
│   └── AsyncRelayCommand.cs (optional)
│
├── 📂 Converters/
│   └── ValueConverters.cs
│
├── 📂 Models/
│   ├── Customer.cs
│   ├── Supplier.cs
│   ├── Invoice.cs
│   ├── InvoiceLine.cs
│   ├── Product.cs
│   ├── Account.cs
│   ├── Transaction.cs
│   ├── Project.cs
│   ├── TimeEntry.cs
│   ├── Dimension.cs
│   ├── Budget.cs
│   ├── Payment.cs
│   ├── BankTransaction.cs
│   └── VATCode.cs
│
├── 📂 ViewModels/
│   ├── Base/
│   │   └── ViewModelBase.cs
│   ├── MainViewModel.cs
│   ├── CustomerViewModel.cs
│   ├── InvoiceViewModel.cs
│   ├── ProductViewModel.cs
│   ├── SupplierViewModel.cs
│   └── DashboardViewModel.cs
│
├── 📂 Views/
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── DashboardView.xaml
│   ├── DashboardView.xaml.cs
│   ├── CustomerView.xaml
│   ├── CustomerView.xaml.cs
│   ├── InvoiceView.xaml
│   ├── InvoiceView.xaml.cs
│   ├── ProductView.xaml
│   ├── ProductView.xaml.cs
│   ├── SupplierView.xaml
│   └── SupplierView.xaml.cs
│
├── 📂 Repositories/
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── ICustomerRepository.cs
│   │   ├── IInvoiceRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── ISupplierRepository.cs
│   │   └── IAccountRepository.cs
│   └── Implementation/
│       ├── CustomerRepository.cs
│       ├── InvoiceRepository.cs
│       ├── ProductRepository.cs
│       ├── SupplierRepository.cs
│       └── AccountRepository.cs
│
├── 📂 Styles/
│   ├── MainStyles.xaml
│   ├── Colors.xaml (optional)
│   └── Buttons.xaml (optional)
│
├── 📂 Database/
│   ├── CreateDatabase.sql
│   └── SeedData.sql (optional)
│
├── 📂 Docs/
│   ├── DomainModel.mermaid
│   ├── DatabaseDiagram.mermaid
│   ├── PROJECT_STRUCTURE.md
│   └── INSTALLATION.md
│
├── 📂 Assets/ (optional)
│   ├── logo.png
│   └── icons/
│
└── 📂 Logs/
    └── app.log (genereres automatisk)
```

## 📦 NuGet Packages

Tilføj følgende packages via NuGet Package Manager:

```xml
<ItemGroup>
    <PackageReference Include="System.Data.SqlClient" Version="4.8.6" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.FileExtensions" Version="8.0.0" />
</ItemGroup>
```

## 🏗️ Arkitektur Lag

### 1️⃣ **Presentation Layer (Views)**
- XAML views til brugerinterface
- Ingen forretningslogik
- DataBinding til ViewModels
- Navigation mellem views

### 2️⃣ **Business Logic Layer (ViewModels)**
- Forretningslogik og validation
- Command handling
- Property change notifications
- Data transformation

### 3️⃣ **Data Access Layer (Repositories)**
- Database operations (CRUD)
- Queries og stored procedures
- Connection management
- Transaction handling

### 4️⃣ **Domain Layer (Models)**
- POCO classes
- Ingen forretningslogik
- Data properties only
- Navigation properties

## 🔑 Vigtige Principper

### MVVM Pattern
```
View ↔️ ViewModel ↔️ Repository ↔️ Database
     ⬇️            ⬇️
  Binding    Commands/Logic
```

### SOLID Principles
- **S**ingle Responsibility: Hver klasse har ét ansvar
- **O**pen/Closed: Åben for udvidelse, lukket for modifikation
- **L**iskov Substitution: Subtypes kan erstatte base types
- **I**nterface Segregation: Små, specifikke interfaces
- **D**ependency Inversion: Afhængigheder via interfaces

### Separation of Concerns (SoC)
- Views: Kun UI markup
- ViewModels: Forretningslogik
- Repositories: Data access
- Models: Data struktur

## 📝 Naming Conventions

### Classes
```csharp
// PascalCase for classes
public class CustomerViewModel { }
public class CustomerRepository { }
```

### Properties
```csharp
// PascalCase for public properties
public string CustomerName { get; set; }
```

### Private Fields
```csharp
// camelCase with underscore prefix
private string _customerName;
private readonly ICustomerRepository _repository;
```

### Methods
```csharp
// PascalCase med beskrivende navne
public async Task LoadCustomersAsync() { }
private void ValidateInput() { }
```

### Commands
```csharp
// PascalCase ending with "Command"
public ICommand SaveCustomerCommand { get; }
public ICommand DeleteCommand { get; }
```

## 🎯 Implementerings Rækkefølge

1. ✅ Database setup (SQL scripts)
2. ✅ Models (POCO classes)
3. ✅ Repository interfaces
4. ✅ Repository implementations
5. ✅ Commands (RelayCommand)
6. ✅ ViewModelBase
7. ✅ Specific ViewModels
8. ✅ Views (XAML)
9. ✅ Styles og converters
10. ✅ Navigation setup

## 🔧 Configuration Files

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EconomicDB;..."
  }
}
```

**Vigtigt:** Husk at sætte "Copy to Output Directory" til "Copy if newer" i Visual Studio properties for appsettings.json!

## 📊 Database Normalisering

Databasen følger 3. normalform (3NF):

- **1NF:** Atomare værdier, ingen gentagne grupper
- **2NF:** Ingen partial dependencies
- **3NF:** Ingen transitive dependencies

## 🚀 Best Practices

### Async/Await
```csharp
// Brug async/await til database operationer
public async Task<Customer> GetCustomerAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}
```

### Error Handling
```csharp
try
{
    // Database operation
}
catch (Exception ex)
{
    HandleException(ex);
}
finally
{
    IsLoading = false;
}
```

### IDisposable Pattern
```csharp
using (var connection = new SqlConnection(_connectionString))
{
    // Use connection
} // Automatisk disposed
```

## 📚 Ressourcer

- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [MVVM Pattern](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [SOLID Principles](https://www.digitalocean.com/community/conceptual_articles/s-o-l-i-d-the-first-five-principles-of-object-oriented-design)

---

💡 **Tip:** Start med én feature (f.eks. Customer management) og få den til at virke komplet før du går videre til næste!
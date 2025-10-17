# 🤝 Contributing til EconomicWPF

Tak fordi du overvejer at bidrage til EconomicWPF! Vi værdsætter alle former for bidrag.

## 📋 Code of Conduct

Ved at deltage i dette projekt accepterer du at overholde vores Code of Conduct (vær respektfuld og professionel).

## 🚀 Hvordan kan jeg bidrage?

### 🐛 Rapportér Bugs

Før du rapporterer en bug, så check venligst om den allerede er rapporteret i Issues.

Når du rapporterer en bug, så inkludér:

- **En klar beskrivelse** af problemet
- **Steps to reproduce** - hvordan kan vi genskabe fejlen?
- **Forventet adfærd** - hvad skulle der ske?
- **Faktisk adfærd** - hvad sker der i stedet?
- **Screenshots** hvis relevant
- **Miljø information**:
  - OS version (Windows 10/11)
  - .NET version
  - SQL Server version

### 💡 Foreslå Features

Vi er altid interesserede i nye ideer! Når du foreslår en feature:

- **Forklar use casen** - hvorfor er det nyttigt?
- **Beskriv løsningen** - hvordan kunne det implementeres?
- **Beskriv alternativer** - har du overvejet andre løsninger?

### 📝 Pull Requests

1. **Fork** projektet
2. **Opret en branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit dine ændringer** (`git commit -m 'Add some AmazingFeature'`)
4. **Push til branchen** (`git push origin feature/AmazingFeature`)
5. **Åbn en Pull Request**

## 💻 Development Setup

Se [INSTALLATION.md](Docs/INSTALLATION.md) for detaljerede instruktioner.

```bash
# Clone repository
git clone https://github.com/ditbrugernavn/EconomicWPF.git
cd EconomicWPF

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

## 📐 Coding Standards

### C# Style Guide

- **Naming Conventions**:
  - PascalCase for classes, methods, properties: `CustomerViewModel`
  - camelCase med underscore for private fields: `_customerRepository`
  - PascalCase for constants: `DefaultTimeout`

- **Formatting**:
  - 4 spaces indentation (ikke tabs)
  - Opening braces på ny linje
  - Én blank linje mellem methods

```csharp
// ✅ GOOD
public class CustomerViewModel : ViewModelBase
{
    private readonly ICustomerRepository _customerRepository;

    public async Task LoadCustomersAsync()
    {
        // Implementation
    }
}

// ❌ BAD
public class customerViewModel {
    private ICustomerRepository customerRepository;
    public async Task loadCustomers() { }
}
```

### XAML Style Guide

- **Naming**: PascalCase for controls: `CustomerNameTextBox`
- **Formatting**: One property per line for readability
- **Databinding**: Altid brug `{Binding}` syntax

```xml
<!-- ✅ GOOD -->
<TextBox x:Name="CustomerNameTextBox"
         Text="{Binding CustomerName, UpdateSourceTrigger=PropertyChanged}"
         Width="200"
         Height="36"/>

<!-- ❌ BAD -->
<TextBox x:Name="customerName" Text="{Binding CustomerName}" Width="200" Height="36"/>
```

### Architecture Patterns

- **MVVM**: Views binder til ViewModels, ingen code-behind logic
- **Repository Pattern**: Al database access gennem repositories
- **SOLID Principles**: Hold classes single-purpose
- **Async/Await**: Brug async for alle I/O operationer

## 🧪 Testing

Før du submitter en PR:

- [ ] Test at projektet bygger uden fejl
- [ ] Test alle CRUD operationer
- [ ] Test på en clean database
- [ ] Check for memory leaks (lange sessions)
- [ ] Test keyboard navigation
- [ ] Verificer databinding virker

## 📚 Documentation

- **Code Comments**: Tilføj XML comments til public methods
- **README Updates**: Opdater README hvis du tilføjer ny funktionalitet
- **CHANGELOG**: Tilføj entry til CHANGELOG.md

```csharp
/// <summary>
/// Gemmer en kunde til databasen
/// </summary>
/// <param name="customer">Kunde der skal gemmes</param>
/// <returns>ID på den gemte kunde</returns>
public async Task<int> SaveCustomerAsync(Customer customer)
{
    // Implementation
}
```

## 🔍 Code Review Process

Alle PRs vil blive reviewet. Vi kigger efter:

1. **Code Quality**: Følger det vores standards?
2. **Testing**: Er det testet grundigt?
3. **Documentation**: Er det dokumenteret?
4. **Performance**: Er der performance issues?
5. **Security**: Er der security concerns?

## 🎯 Priority Areas

Vi har særligt brug for hjælp med:

- 📊 **Rapportering**: Implementering af flere rapporter
- 🧪 **Unit Tests**: Tilføj test coverage
- 📱 **UI/UX**: Forbedre user interface
- 🌐 **Internationalization**: Support for flere sprog
- 📖 **Documentation**: Forbedre docs og tutorials

## 💬 Få Hjælp

Har du spørgsmål?

- 📧 Email: support@example.com
- 💬 GitHub Discussions
- 🐛 GitHub Issues (for bugs)

## 🎉 Anerkendelser

Alle bidragsydere vil blive tilføjet til vores [Contributors](https://github.com/ditbrugernavn/EconomicWPF/graphs/contributors) liste.

## 📜 License

Ved at bidrage accepterer du at dit bidrag vil blive licenseret under [MIT License](LICENSE).

---

**Tak for dit bidrag! 🙏**

Vi sætter stor pris på din tid og indsats i at forbedre EconomicWPF.
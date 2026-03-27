# Dita.Tests

Testovací projekt pro řešení Dita s použitím xUnit a NSubstitute.

## Struktura projektu

```
Dita.Tests/
├── Server/                          # Testy pro Dita.Server projekt
│   ├── Models/
│   │   ├── Enums/
│   │   │   ├── ServerCapabilitiesTests.cs
│   │   │   └── StorageTypeTests.cs
│   │   └── Settings/
│   │       └── MainSettingsTests.cs
│   └── Pages/
│       ├── ErrorModelTests.cs
│       ├── IndexModelTests.cs
│       └── PrivacyModelTests.cs
├── DiskAccessor/
│   ├── Linux/                       # Testy pro Dita.DiskAccessor.Linux
│   │   └── ProgramTests.cs
│   └── Windows/                     # Testy pro Dita.DiskAccessor.Windows
│       └── ProgramTests.cs
└── Tui/                             # Testy pro Dita.Tui
    └── ProgramTests.cs
```

## Použité technologie

- **xUnit** - Testovací framework
- **NSubstitute** - Mocking framework pro vytváření mock objektů
- **coverlet.collector** - Nástroj pro code coverage

## Spuštění testů

### Všechny testy
```bash
dotnet test
```

### S code coverage
```bash
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
```

### Specifická třída testů
```bash
dotnet test --filter "FullyQualifiedName~MainSettingsTests"
```

## Konvence pojmenování

- Testovací třídy: `{TestedClass}Tests`
- Testovací metody: `When{Condition}Then{ExpectedResult}`
- Struktura AAA: Arrange-Act-Assert

## NSubstitute - Základní použití

```csharp
// Vytvoření mock objektu
var mockService = Substitute.For<IService>();

// Nastavení návratové hodnoty
mockService.GetData().Returns("test data");

// Ověření volání
mockService.Received().GetData();
```

## Přidání nových testů

1. Vytvořte nový soubor v odpovídající struktuře složek
2. Pojmenujte třídu podle konvence: `{TestedClass}Tests`
3. Použijte atribut `[Fact]` pro jednoduché testy nebo `[Theory]` pro parametrizované testy
4. Dodržujte AAA pattern (Arrange-Act-Assert)
5. Přidejte XML dokumentační komentáře

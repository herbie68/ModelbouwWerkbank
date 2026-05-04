# Coverage Backlog

> Bedoeld als pragmatische backlog voor zube.io. Focus ligt op risicoreductie en snelle coverage-winst, niet op het kunstmatig verhogen van percentages.

**Bron:** [CodeCoverage.xml](CodeCoverage.xml)

**Belangrijkste observatie:**
- `Modelbouwer.dll` zit op ongeveer `23.87%` line coverage
- veel `0%` zit in `Views\*.xaml.cs`, converters en event handlers; dat zijn voorlopig geen first targets

**Prioriteringsregels:**
- eerst businesslogica en services met lage complexiteit
- daarna viewmodels met hoog regressierisico
- daarna bestaande grote modules verdiepen
- views en converters alleen meenemen als daar echt functionele logica in zit

---

## Wave 1: Quick Wins

### CB-01 UnitService tests toevoegen

- Prioriteit: hoog
- Moeite: laag
- Verwachte winst: snel, waarschijnlijk bijna volledig ongetest
- Bestanden:
  - [UnitService.cs](../../../Modelbouwer/Services/UnitService.cs)
  - [Modelbouwer.UnitTests](../../../Modelbouwer.UnitTests)
- Aanpak:
  - test voor `GetAllUnitsAsync`
  - test voor `GetUnitByIdAsync`
  - test voor `InsertUnitAsync`
  - test voor `UpdateUnitAsync`
  - test voor `DeleteUnitAsync`
  - test voor lege queryresultaten of ontbrekende records
- Acceptatie:
  - alle publieke methoden van `UnitService` hebben ten minste één succespad-test
  - fout- of leegpad is afgedekt waar relevant

### CB-02 CountryService tests toevoegen

- Prioriteit: hoog
- Moeite: laag
- Verwachte winst: snel
- Bestanden:
  - [CountryService.cs](../../../Modelbouwer/Services/CountryService.cs)
- Aanpak:
  - CRUD-paden afdekken
  - mapping van databasevelden naar `CountryModel` valideren
  - delete-gedrag en parameteropbouw controleren
- Acceptatie:
  - alle publieke methods van `CountryService` getest

### CB-03 CategoryService tests toevoegen

- Prioriteit: hoog
- Moeite: laag
- Verwachte winst: snel
- Bestanden:
  - [CategoryService.cs](../../../Modelbouwer/Services/CategoryService.cs)
- Aanpak:
  - lijst laden
  - insert/update met juiste parameterwaarden
  - delete-flow
- Acceptatie:
  - CRUD-scenario's zijn afgedekt met unit tests

### CB-04 StorageLocationService tests toevoegen

- Prioriteit: hoog
- Moeite: laag
- Verwachte winst: snel
- Bestanden:
  - [StorageLocationService.cs](../../../Modelbouwer/Services/StorageLocationService.cs)
- Aanpak:
  - lijst laden
  - opslaan van nieuwe locatie
  - wijzigen van bestaande locatie
  - verwijderen
  - null/lege velden waar relevant
- Acceptatie:
  - servicegedrag rond opslaglocaties is volledig regressiegetest

### CB-05 WorkTypeService tests toevoegen

- Prioriteit: hoog
- Moeite: laag
- Verwachte winst: snel
- Bestanden:
  - [WorkTypeService.cs](../../../Modelbouwer/Services/WorkTypeService.cs)
- Aanpak:
  - standaard CRUD-tests
  - parameteropbouw en modelmapping controleren
- Acceptatie:
  - alle publieke methods van `WorkTypeService` getest

### CB-06 CurrencyService tests toevoegen

- Prioriteit: middel-hoog
- Moeite: laag-middel
- Verwachte winst: redelijk
- Bestanden:
  - [CurrencyService.cs](../../../Modelbouwer/Services/CurrencyService.cs)
- Aanpak:
  - lijst laden
  - insert/update/delete
  - valutavelden en mapping controleren
- Acceptatie:
  - CRUD en mapping van `CurrencyService` afgedekt

---

## Wave 2: High-Risk ViewModels

### CB-07 ProjectPageViewModel tests toevoegen

- Prioriteit: zeer hoog
- Moeite: middel
- Verwachte winst: hoog
- Bestanden:
  - [ProjectPageViewModel.cs](../../../Modelbouwer/ViewModels/ProjectPageViewModel.cs)
- Aanpak:
  - `LoadItemsAsync`
  - selectie wisselen
  - detailvelden synchroniseren
  - nieuw item / annuleren
  - opslaan
  - verwijderen
  - command enabled/disabled state
  - validatiegedrag
- Acceptatie:
  - belangrijkste gebruikersflows voor projecten zijn geautomatiseerd getest

### CB-08 StockManagementPageViewModel tests toevoegen

- Prioriteit: zeer hoog
- Moeite: middel
- Verwachte winst: hoog
- Bestanden:
  - [StockManagementPageViewModel.cs](../../../Modelbouwer/ViewModels/StockManagementPageViewModel.cs)
- Aanpak:
  - voorraad laden
  - filter- en selectiegedrag
  - voorraadcorrectie-commando's
  - herberekening van afgeleide waarden
  - foutpaden rond mutaties
- Acceptatie:
  - voorraadbeheer heeft regressietests voor de kernflows

### CB-09 CategoryPageViewModel tests toevoegen

- Prioriteit: hoog
- Moeite: middel
- Verwachte winst: redelijk
- Bestanden:
  - [CategoryPageViewModel.cs](../../../Modelbouwer/ViewModels/CategoryPageViewModel.cs)
- Aanpak:
  - laden
  - selectie synchroniseren
  - save/delete
  - command state
- Acceptatie:
  - standaard CRUD-viewmodelgedrag afgedekt

### CB-10 StorageLocationPageViewModel tests toevoegen

- Prioriteit: hoog
- Moeite: middel
- Verwachte winst: redelijk
- Bestanden:
  - [StorageLocationPageViewModel.cs](../../../Modelbouwer/ViewModels/StorageLocationPageViewModel.cs)
- Aanpak:
  - laden
  - selectie/detail sync
  - save/delete
  - command state
- Acceptatie:
  - standaard CRUD-flow van opslaglocaties getest

### CB-11 WorkTypePageViewModel tests toevoegen

- Prioriteit: hoog
- Moeite: middel
- Verwachte winst: redelijk
- Bestanden:
  - [WorkTypePageViewModel.cs](../../../Modelbouwer/ViewModels/WorkTypePageViewModel.cs)
- Aanpak:
  - laden
  - selectie/detail sync
  - save/delete
  - command state
- Acceptatie:
  - standaard CRUD-flow van werktypes getest

---

## Wave 3: Deepen Existing Coverage

### CB-12 ProductPageViewModel tests uitbreiden

- Prioriteit: hoog
- Moeite: middel-hoog
- Verwachte winst: hoog
- Bestanden:
  - [ProductPageViewModel.cs](../../../Modelbouwer/ViewModels/ProductPageViewModel.cs)
  - [ProductPageViewModelTests.cs](../../../Modelbouwer.UnitTests/ViewModels/ProductPageViewModelTests.cs)
- Aanpak:
  - niet-afgedekte selectieflows
  - opslaglocatie/categorie synchronisatie
  - save/delete-paden
  - async initialisatie
  - randgevallen rond supplier-gerelateerde state
- Acceptatie:
  - huidige tests dekken niet alleen basisgedrag, maar ook de regressiegevoelige branches

### CB-13 SupplierPageViewModel tests uitbreiden

- Prioriteit: hoog
- Moeite: middel
- Verwachte winst: redelijk-hoog
- Bestanden:
  - [SupplierPageViewModel.cs](../../../Modelbouwer/ViewModels/SupplierPageViewModel.cs)
  - [SupplierPageViewModelTests.cs](../../../Modelbouwer.UnitTests/ViewModels/SupplierPageViewModelTests.cs)
- Aanpak:
  - child collections
  - detail sync
  - save/delete
  - constructor-gestarte async flows
- Acceptatie:
  - belangrijkste supplier-scenario's zijn afgedekt

### CB-14 StockOrderViewModel tests uitbreiden

- Prioriteit: hoog
- Moeite: middel
- Verwachte winst: hoog
- Bestanden:
  - [StockOrderViewModel.cs](../../../Modelbouwer/ViewModels/StockOrderViewModel.cs)
  - [StockOrderViewModelTests.cs](../../../Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs)
- Aanpak:
  - toolbar states
  - `Gesloten orders tonen`
  - memo-save gedrag
  - selectie- en reset-flow
  - closed/read-only UX-regels
  - toekomstige `Heropen order` flow zodra die gebouwd is
- Acceptatie:
  - ordergedrag met de meeste UI-state is regressiegetest

---

## Wave 4: Import/Export Alleen Als Zakelijk Relevant

### CB-15 CsvImportService tests toevoegen

- Prioriteit: middel
- Moeite: middel
- Verwachte winst: branch coverage en foutpaden
- Bestanden:
  - [CsvImportService.cs](../../../Modelbouwer/Services/CsvImportService.cs)
- Aanpak:
  - kolommapping
  - ontbrekende headers
  - parsefouten
  - decimal/culture varianten
- Acceptatie:
  - import faalt gecontroleerd en voorspelbaar bij ongeldige input

### CB-16 CsvExportService tests toevoegen

- Prioriteit: middel
- Moeite: middel
- Verwachte winst: redelijk
- Bestanden:
  - [CsvExportService.cs](../../../Modelbouwer/Services/CsvExportService.cs)
- Aanpak:
  - headers
  - escaping
  - lege datasets
- Acceptatie:
  - export-output is stabiel en reproduceerbaar

### CB-17 ExcelExportService tests toevoegen

- Prioriteit: laag-middel
- Moeite: middel-hoog
- Verwachte winst: afhankelijk van gebruik
- Bestanden:
  - [ExcelExportService.cs](../../../Modelbouwer/Services/ExcelExportService.cs)
- Aanpak:
  - workbook-opbouw
  - kolommen
  - lege waarden
  - basisformattering
- Acceptatie:
  - export genereert een correct workbook voor de belangrijkste scenario's

---

## Bewust Lage Prioriteit

Deze onderdelen trekken de coverage wel omlaag, maar leveren voorlopig minder veiligheidswinst op:

- `Views\*.xaml.cs`
- converters
- `Loaded`-handlers
- `DataGrid_Loaded`-achtige event wiring

Alleen meenemen als:
- er echte businesslogica in staat
- of er al bekende bugs in zitten

---

## Aanbevolen Volgorde Voor Uitvoering

1. `CB-01` t/m `CB-06`
2. `CB-07` en `CB-08`
3. `CB-09` t/m `CB-11`
4. `CB-12` t/m `CB-14`
5. `CB-15` t/m `CB-17` alleen als import/export belangrijk is

## Verwachte Resultaatstrategie

- Wave 1 verhoogt coverage waarschijnlijk het snelst
- Wave 2 verlaagt regressierisico het meest
- Wave 3 maakt bestaande testgebieden merkbaar sterker
- Wave 4 is vooral nuttig als import/export operationeel belangrijk is


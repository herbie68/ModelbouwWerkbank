# Coverage Backlog For Zube

Deze versie is bewust geschreven als ticketset voor directe overname in zube.io.

---

## CB-01 UnitService testdekking toevoegen

**Beschrijving**  
Voeg unit tests toe voor `UnitService`, zodat alle publieke CRUD-paden en de belangrijkste mapping- en foutscenario's zijn afgedekt.

**Bestanden**
- [UnitService.cs](../../../Modelbouwer/Services/UnitService.cs)

**Acceptatiecriteria**
- `GetAllUnitsAsync` is getest
- `GetUnitByIdAsync` is getest
- `InsertUnitAsync` is getest
- `UpdateUnitAsync` is getest
- `DeleteUnitAsync` is getest
- minstens één leegpad of ontbrekend-record scenario is getest

---

## CB-02 CountryService testdekking toevoegen

**Beschrijving**  
Voeg unit tests toe voor `CountryService`, met focus op CRUD, modelmapping en parameteropbouw.

**Bestanden**
- [CountryService.cs](../../../Modelbouwer/Services/CountryService.cs)

**Acceptatiecriteria**
- alle publieke CRUD-methoden zijn getest
- mapping van databasewaarden naar model is getest
- delete-flow is getest

---

## CB-03 CategoryService testdekking toevoegen

**Beschrijving**  
Voeg unit tests toe voor `CategoryService`, zodat laden, opslaan en verwijderen regressiegetest zijn.

**Bestanden**
- [CategoryService.cs](../../../Modelbouwer/Services/CategoryService.cs)

**Acceptatiecriteria**
- lijst laden is getest
- insert is getest
- update is getest
- delete is getest
- parameterwaarden voor save-scenario's zijn gecontroleerd

---

## CB-04 StorageLocationService testdekking toevoegen

**Beschrijving**  
Voeg unit tests toe voor `StorageLocationService`, inclusief basis-CRUD en randgevallen rond lege waarden.

**Bestanden**
- [StorageLocationService.cs](../../../Modelbouwer/Services/StorageLocationService.cs)

**Acceptatiecriteria**
- lijst laden is getest
- nieuwe opslaglocatie opslaan is getest
- bestaande opslaglocatie wijzigen is getest
- verwijderen is getest
- relevante null- of leegscenario's zijn getest

---

## CB-05 WorkTypeService testdekking toevoegen

**Beschrijving**  
Voeg unit tests toe voor `WorkTypeService`, zodat CRUD-gedrag en modelmapping zijn afgedekt.

**Bestanden**
- [WorkTypeService.cs](../../../Modelbouwer/Services/WorkTypeService.cs)

**Acceptatiecriteria**
- alle publieke CRUD-methoden zijn getest
- mapping is getest
- parameteropbouw voor save-scenario's is gecontroleerd

---

## CB-06 CurrencyService testdekking toevoegen

**Beschrijving**  
Voeg unit tests toe voor `CurrencyService`, met dekking voor CRUD en valutaveld-mapping.

**Bestanden**
- [CurrencyService.cs](../../../Modelbouwer/Services/CurrencyService.cs)

**Acceptatiecriteria**
- lijst laden is getest
- insert is getest
- update is getest
- delete is getest
- mapping van valutavelden is getest

---

## CB-07 ProjectPageViewModel testdekking toevoegen

**Beschrijving**  
Voeg gerichte viewmodeltests toe voor `ProjectPageViewModel`, zodat de belangrijkste gebruikersflows en command states regressiegetest zijn.

**Bestanden**
- [ProjectPageViewModel.cs](../../../Modelbouwer/ViewModels/ProjectPageViewModel.cs)

**Acceptatiecriteria**
- `LoadItemsAsync` is getest
- selectie wisselen en detail-sync zijn getest
- nieuw item en annuleren zijn getest
- opslaan is getest
- verwijderen is getest
- belangrijke command enabled/disabled states zijn getest
- validatiegedrag is getest waar relevant

---

## CB-08 StockManagementPageViewModel testdekking toevoegen

**Beschrijving**  
Voeg tests toe voor `StockManagementPageViewModel`, met focus op voorraadladen, mutaties, filters en afgeleide berekeningen.

**Bestanden**
- [StockManagementPageViewModel.cs](../../../Modelbouwer/ViewModels/StockManagementPageViewModel.cs)

**Acceptatiecriteria**
- voorraad laden is getest
- filtergedrag is getest
- selectiegedrag is getest
- voorraadcorrectie-commando's zijn getest
- herberekening van afgeleide waarden is getest
- minstens één foutpad rond mutaties is getest

---

## CB-09 CategoryPageViewModel testdekking toevoegen

**Beschrijving**  
Voeg tests toe voor `CategoryPageViewModel`, zodat standaard CRUD-viewmodelgedrag is afgedekt.

**Bestanden**
- [CategoryPageViewModel.cs](../../../Modelbouwer/ViewModels/CategoryPageViewModel.cs)

**Acceptatiecriteria**
- laden is getest
- selectie/detail-sync is getest
- save is getest
- delete is getest
- command state is getest

---

## CB-10 StorageLocationPageViewModel testdekking toevoegen

**Beschrijving**  
Voeg tests toe voor `StorageLocationPageViewModel`, met dekking voor laden, selectie en CRUD-commando's.

**Bestanden**
- [StorageLocationPageViewModel.cs](../../../Modelbouwer/ViewModels/StorageLocationPageViewModel.cs)

**Acceptatiecriteria**
- laden is getest
- selectie/detail-sync is getest
- save is getest
- delete is getest
- command state is getest

---

## CB-11 WorkTypePageViewModel testdekking toevoegen

**Beschrijving**  
Voeg tests toe voor `WorkTypePageViewModel`, zodat de standaard CRUD-flow en selectie-synchronisatie zijn afgedekt.

**Bestanden**
- [WorkTypePageViewModel.cs](../../../Modelbouwer/ViewModels/WorkTypePageViewModel.cs)

**Acceptatiecriteria**
- laden is getest
- selectie/detail-sync is getest
- save is getest
- delete is getest
- command state is getest

---

## CB-12 ProductPageViewModel bestaande tests verdiepen

**Beschrijving**  
Breid de bestaande tests voor `ProductPageViewModel` uit, zodat niet alleen het basisgedrag maar ook de regressiegevoelige branches zijn afgedekt.

**Bestanden**
- [ProductPageViewModel.cs](../../../Modelbouwer/ViewModels/ProductPageViewModel.cs)
- [ProductPageViewModelTests.cs](../../../Modelbouwer.UnitTests/ViewModels/ProductPageViewModelTests.cs)

**Acceptatiecriteria**
- extra selectieflows zijn getest
- opslaglocatie- en categorie-sync is getest
- save/delete-paden zijn verder afgedekt
- async initialisatie is getest
- supplier-gerelateerde randgevallen zijn getest

---

## CB-13 SupplierPageViewModel bestaande tests verdiepen

**Beschrijving**  
Breid de bestaande tests voor `SupplierPageViewModel` uit, met extra focus op child collections, detail-sync en async laadgedrag.

**Bestanden**
- [SupplierPageViewModel.cs](../../../Modelbouwer/ViewModels/SupplierPageViewModel.cs)
- [SupplierPageViewModelTests.cs](../../../Modelbouwer.UnitTests/ViewModels/SupplierPageViewModelTests.cs)

**Acceptatiecriteria**
- child collections zijn getest
- detail-sync is getest
- save/delete is verder afgedekt
- constructor-gestarte async flows zijn getest

---

## CB-14 StockOrderViewModel bestaande tests verdiepen

**Beschrijving**  
Breid de bestaande tests voor `StockOrderViewModel` uit, zodat meer UI-state en closed-order gedrag regressiegetest zijn.

**Bestanden**
- [StockOrderViewModel.cs](../../../Modelbouwer/ViewModels/StockOrderViewModel.cs)
- [StockOrderViewModelTests.cs](../../../Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs)

**Acceptatiecriteria**
- toolbar states zijn getest
- `Gesloten orders tonen` is getest
- memo-save gedrag is getest
- selectie- en reset-flow zijn getest
- closed/read-only gedrag is getest
- toekomstige `Heropen order` flow krijgt tests zodra die gebouwd is

---

## CB-15 CsvImportService testdekking toevoegen

**Beschrijving**  
Voeg tests toe voor `CsvImportService`, met focus op foutafhandeling en invoervarianten.

**Bestanden**
- [CsvImportService.cs](../../../Modelbouwer/Services/CsvImportService.cs)

**Acceptatiecriteria**
- kolommapping is getest
- ontbrekende headers zijn getest
- parsefouten zijn getest
- decimal/culture-varianten zijn getest

---

## CB-16 CsvExportService testdekking toevoegen

**Beschrijving**  
Voeg tests toe voor `CsvExportService`, zodat export-output stabiel en reproduceerbaar is.

**Bestanden**
- [CsvExportService.cs](../../../Modelbouwer/Services/CsvExportService.cs)

**Acceptatiecriteria**
- headers zijn getest
- escaping is getest
- lege dataset is getest

---

## CB-17 ExcelExportService testdekking toevoegen

**Beschrijving**  
Voeg tests toe voor `ExcelExportService`, met focus op de belangrijkste workbook-scenario's.

**Bestanden**
- [ExcelExportService.cs](../../../Modelbouwer/Services/ExcelExportService.cs)

**Acceptatiecriteria**
- workbook-opbouw is getest
- kolommen zijn getest
- lege waarden zijn getest
- basisformattering is getest waar relevant

---

## Niet als eerste oppakken

**Beschrijving**  
De volgende onderdelen drukken de coverage wel omlaag, maar zijn voorlopig geen first targets omdat ze relatief weinig risicoreductie geven:

**Onderdelen**
- `Views\*.xaml.cs`
- converters
- `Loaded`-handlers
- `DataGrid_Loaded`-achtige event wiring

**Acceptatiecriteria**
- alleen oppakken als er echte businesslogica in zit of als er bekende bugs op zitten


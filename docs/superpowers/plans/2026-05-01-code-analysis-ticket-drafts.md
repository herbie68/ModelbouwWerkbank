# Code Analysis Ticket Drafts

Bron: `C:\Users\hnijk\OneDrive\Downloads\CodeIssuesReport.xml`  
Rapportdatum: `2026-05-01 14:44`

## Samenvatting analyse

- Totaal aantal issues: `579`
- Grootste issuecodes:
  - `CRR0029`: `251` keer (`ConfigureAwait(true) is called implicitly`)
  - `CRR0035`: `167` keer (async methoden missen `CancellationToken`)
  - `CRRSP08`: `105` keer (spelling/public identifier issues)
- Zwaarst geraakte bestanden:
  - `Modelbouwer/Helpers/DBNames.cs`: `86`
  - `Modelbouwer/Services/GenericDataService.cs`: `39`
  - `Modelbouwer/ViewModels/StockOrderViewModel.cs`: `35`
  - `Modelbouwer/Services/StockOrderService.cs`: `20`
  - `Modelbouwer/ViewModels/ProductPageViewModel.cs`: `20`

## Ticket 1

Title: `Add cancellation token support to shared async infrastructure`

```md
## Summary
Add `CancellationToken` support to the shared async infrastructure that sits underneath the rest of the application.

## Why
The chat code analysis report (`CodeIssuesReport.xml`, May 1, 2026) shows a high-impact cluster of async infrastructure findings in the app lifecycle and data/export plumbing. These findings affect correctness, graceful shutdown, cancellation behavior, and make downstream async cleanup harder.

## Expected behavior
- Shared infrastructure methods accept and propagate `CancellationToken`
- App start/stop and transaction commit/rollback pass explicit cancellation tokens
- Export flows and DB helper methods can participate in cancellation-aware workflows
- Touched async calls make their continuation behavior explicit instead of relying on implicit defaults

## Acceptance criteria
- [ ] `App.xaml.cs` passes explicit cancellation tokens into `_host.StartAsync()` and `_host.StopAsync()`
- [ ] `GenericDataService` methods accept and propagate `CancellationToken`
- [ ] Transaction `CommitAsync()` and `RollbackAsync()` calls receive a token
- [ ] `DBCommands.GetLatestIdFromTableAsync` is token-aware
- [ ] `CsvExportService` and `ExcelExportService` async entry points are token-aware where appropriate
- [ ] Touched files no longer rely on implicit `ConfigureAwait(true)`
- [ ] The solution still builds successfully after the infrastructure signature changes

## Notes
Source: `ModelbouwWerkbank`
Priority: `P2`
Points: `8`
Assignee: `herbie68`
Main analyzer codes: `CRR0035`, `CRR0039`, `CRR0029`
Main files: `App.xaml.cs`, `GenericDataService.cs`, `DBCommands.cs`, `CsvExportService.cs`, `ExcelExportService.cs`
This ticket should be completed before broader repo-wide async cleanup tickets.
```

## Ticket 2

Title: `Propagate cancellation tokens through CRUD contracts and base page workflows`

```md
## Summary
Propagate `CancellationToken` support through the CRUD service interfaces, validators, and shared page workflow abstractions.

## Why
A large part of the code analysis report is caused by async signatures that cannot accept a token yet. The biggest leverage is in the shared contracts and base workflows, because they fan out into many page viewmodels and services.

## Expected behavior
- CRUD interfaces expose token-aware async methods
- Shared page workflow methods forward tokens consistently
- Validators can run in token-aware flows
- Async method naming is aligned where analyzer findings call it out

## Acceptance criteria
- [ ] CRUD-oriented interfaces such as `IBrandService`, `ICategoryService`, `IContactService`, `ICountryService`, `ICurrencyService`, `IProductService`, `IProjectService`, `IWorkTypeService` and related interfaces accept `CancellationToken`
- [ ] `IEntityValidator.ValidateAsync` accepts `CancellationToken`
- [ ] `EntityPageViewModel` methods such as `LoadItemsAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync`, `SaveAsync`, and `ReloadAsync` are token-aware
- [ ] The `Delete` async method in `EntityPageViewModel` is renamed to a proper `Async` suffix where appropriate
- [ ] Touched derived page workflows compile cleanly against the updated contracts
- [ ] Touched files make explicit async continuation choices instead of relying on implicit `ConfigureAwait(true)`

## Notes
Source: `ModelbouwWerkbank`
Priority: `P2`
Points: `8`
Assignee: `herbie68`
Main analyzer codes: `CRR0035`, `CRR0034`, `CRR0029`
Main files: `EntityPageViewModel.cs`, service interfaces under `Interfaces`, validators, and dependent page viewmodels
This ticket intentionally focuses on the shared abstraction layer, not every individual low-level service implementation.
```

## Ticket 3

Title: `Fix StockOrder async analyzer findings`

```md
## Summary
Clean up the concentrated async/code-analysis findings in the StockOrder feature set.

## Why
The recent StockOrder implementation has a dense cluster of analyzer findings in `IStockOrderService`, `StockOrderService`, and `StockOrderViewModel`. This is a good bounded slice with high local impact and lower coordination cost than a full repo-wide sweep.

## Expected behavior
- StockOrder service contracts become token-aware
- StockOrder viewmodel async workflows can accept and propagate cancellation
- Stock-related transaction helpers become explicit and analyzer-clean
- Touched StockOrder calls stop relying on implicit `ConfigureAwait(true)`

## Acceptance criteria
- [ ] `IStockOrderService` methods accept `CancellationToken`
- [ ] `StockOrderService` methods accept and propagate `CancellationToken`
- [ ] `StockOrderViewModel` async workflows such as `InitializeAsync`, `SaveOrderAsync`, `LoadReferenceDataAsync`, `LoadOrdersAsync`, `LoadSelectedOrderAsync`, `DeleteOrderAsync`, `AddSelectedProductAsync`, `EditSelectedOrderLineAsync`, `DeleteSelectedOrderLineAsync`, and `UpsertProductSupplierAsync` are token-aware
- [ ] StockOrder transaction helper methods are token-aware and use explicit continuation behavior
- [ ] Any naming cleanup done in this area preserves behavior and does not break the existing StockOrder workflow
- [ ] Existing StockOrder tests still pass after the cleanup

## Notes
Source: `ModelbouwWerkbank`
Priority: `P3`
Points: `5`
Assignee: `herbie68`
Main analyzer codes: `CRR0035`, `CRR0029`, `CRRSP08`
Main files: `IStockOrderService.cs`, `StockOrderService.cs`, `StockOrderViewModel.cs`
This ticket is intentionally scoped to the StockOrder slice so it can be delivered independently of broader async refactors.
```

## Ticket 4

Title: `Make async continuation behavior explicit across services and UI`

```md
## Summary
Audit and fix the repository-wide async continuation warnings so async code stops relying on implicit `ConfigureAwait(true)`.

## Why
`CRR0029` is the single largest analyzer category in the report, with 251 findings. Even after cancellation-token work is done, the codebase still needs a consistent and deliberate policy for UI-bound async code versus service/data-layer async code.

## Expected behavior
- Async continuation behavior is explicit in touched methods
- UI-bound code keeps the UI-thread behavior it actually needs
- service, validator, and data-layer code avoids accidental continuation assumptions
- redundant `await` usage is cleaned up where flagged during the same pass

## Acceptance criteria
- [ ] A clear continuation policy is applied to touched code paths in services, validators, viewmodels, and views
- [ ] High-churn files such as `GenericDataService.cs`, `StockOrderViewModel.cs`, `StockOrderService.cs`, `ProductPageViewModel.cs`, `StockManagementPageViewModel.cs`, and page code-behind files are updated first
- [ ] Redundant `await` findings that are encountered in the same areas are removed where safe
- [ ] The resulting code remains functionally correct for WPF UI flows
- [ ] The solution still builds and existing tests still pass after the cleanup

## Notes
Source: `ModelbouwWerkbank`
Priority: `P3`
Points: `8`
Assignee: `herbie68`
Main analyzer codes: `CRR0029`, `CRR0030`
The intent is not to blindly add one pattern everywhere; the implementation should distinguish UI code from non-UI code.
```

## Ticket 5

Title: `Normalize public naming inconsistencies in WorkType and DB name contracts`

```md
## Summary
Normalize the naming inconsistencies and spelling problems that appear in public type names, file/type pairs, and DB-related constants.

## Why
The report contains 105 spelling-related warnings, with the largest concentration in `DBNames.cs` and several `WorkType`/`Worktype` mismatches. Some of these are low-risk code cleanups, while others may affect public identifiers, XAML wiring, or database contract names, so they need a deliberate pass.

## Expected behavior
- Type names and file names align consistently
- obvious `WorkType`/`Worktype` mismatches are normalized
- public identifier spelling problems are fixed where safe
- DB-facing names are reviewed carefully so schema/query compatibility is preserved

## Acceptance criteria
- [ ] File/type mismatches such as `WorktypeService`, `WorktypeModel`, `WorktypePageViewModel`, and related types are normalized
- [ ] `ExportModel` nested type naming findings are resolved cleanly
- [ ] `DBNames.cs` spelling findings are reviewed and either fixed safely or handled via compatibility aliases/wrappers
- [ ] Potentially breaking renames for query constants, view names, or schema-facing identifiers are documented before implementation
- [ ] The application still builds and runtime references continue to resolve correctly after the rename pass

## Notes
Source: `ModelbouwWerkbank`
Priority: `P4`
Points: `5`
Assignee: `herbie68`
Main analyzer codes: `CRRSP08`, `CRR0048`
High-risk examples include `Worktype`, `Categorys`, `Recieved`, `Stocklog`, `Orderline`, `Fullpath`, and `Totalss`
This ticket should prefer safe normalization over aggressive breaking renames.
```

## Ticket 6

Title: `Clean up low-risk analyzer warnings and file hygiene`

```md
## Summary
Clean up the remaining low-risk analyzer findings that mainly affect readability, consistency, and report noise.

## Why
After the async and naming issues are addressed, there will still be a smaller set of low-risk warnings that are worth resolving to keep future reports readable and reduce maintenance noise.

## Expected behavior
- obviously unused locals, fields, and parameters are removed or replaced with discards
- simple readability fixes are applied where they do not change behavior
- small file hygiene issues such as nested helper/result types in the wrong file are resolved

## Acceptance criteria
- [ ] Unused locals, fields, and parameters are cleaned up where safe
- [ ] Redundant field initializations are removed where they add no value
- [ ] String interpolation findings are applied where it improves readability
- [ ] String comparison findings are reviewed and fixed where the suggested comparison semantics are acceptable
- [ ] Small file hygiene findings such as `CsvImportResult`, `ColumnInfo`, and `ExportData` placement are addressed where it improves structure
- [ ] The solution still builds after the cleanup pass

## Notes
Source: `ModelbouwWerkbank`
Priority: `P5`
Points: `3`
Assignee: `herbie68`
Main analyzer codes: `CRR0044`, `CRR0045`, `CRR0046`, `CRR0047`, `CRR0026`, `CRR0042`, `CRR0050`, `CRR0052`
Representative files: `NavigationViewModel.cs`, `ProductPageViewModel.cs`, `CaptionSummaryRowConverter.cs`, `CsvImportService.cs`, `ExportModel.cs`, `StockManagementModel.cs`
```

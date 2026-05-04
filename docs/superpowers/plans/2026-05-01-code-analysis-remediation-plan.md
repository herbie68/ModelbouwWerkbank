# Code Analysis Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reduce and structurally control the issues from `CodeIssuesReport.xml` by fixing the highest-leverage async, contract, and naming problems first, without destabilizing the WPF application.

**Architecture:** Tackle the report in dependency order instead of raw count order. Start with shared async infrastructure, then propagate signatures through contracts and base workflows, then clean up bounded feature slices like StockOrder, and only then do broader consistency and hygiene passes.

**Tech Stack:** WPF, CommunityToolkit.Mvvm, .NET 10 preview, MSTest, Moq, MySQL data access, external XML-based code analysis report.

---

## Source Context

- Analysis source: `C:\Users\hnijk\OneDrive\Downloads\CodeIssuesReport.xml`
- Total issues in report: `579`
- Largest issue groups:
  - `CRR0029`: implicit `ConfigureAwait(true)` (`251`)
  - `CRR0035`: missing `CancellationToken` on async methods (`167`)
  - `CRRSP08`: public naming/spelling issues (`105`)
- Highest-leverage files:
  - `Modelbouwer/Helpers/DBNames.cs`
  - `Modelbouwer/Services/GenericDataService.cs`
  - `Modelbouwer/ViewModels/StockOrderViewModel.cs`
  - `Modelbouwer/Services/StockOrderService.cs`
  - `Modelbouwer/ViewModels/EntityPageViewModel.cs`

## Working Rules

- Fix by dependency order, not by alphabetical order.
- Do not mix high-risk renames with async signature refactors in the same commit.
- Keep WPF UI-thread behavior explicit; do not apply one blanket async pattern everywhere.
- Treat `DBNames.cs` and any schema/query-facing identifiers as compatibility-sensitive.
- Prefer small commits per slice so regression hunting stays easy.

## Verification Baseline

- Core build:
  - `dotnet build .\ModelbouwWerkbank.slnx -v:minimal`
- Existing focused tests:
  - `dotnet test .\Modelbouwer.UnitTests\Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrder"`
  - `dotnet test .\Modelbouwer.UnitTests\Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductPageViewModelTests"`
- Analyzer regression check:
  - Regenerate the same report workflow that produced `CodeIssuesReport.xml`
  - Compare total counts and hotspot files against the previous export

---

### Task 1: Establish A Stable Remediation Baseline

**Files:**
- Reference: `C:\Users\hnijk\OneDrive\Downloads\CodeIssuesReport.xml`
- Reference: `docs/superpowers/plans/2026-05-01-code-analysis-ticket-drafts.md`
- Reference: `docs/superpowers/plans/2026-05-01-code-analysis-remediation-plan.md`

- [ ] Confirm the current baseline still matches the analysis context
- [ ] Run `dotnet build .\ModelbouwWerkbank.slnx -v:minimal`
- [ ] Run `dotnet test .\Modelbouwer.UnitTests\Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrder"`
- [ ] Run `dotnet test .\Modelbouwer.UnitTests\Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductPageViewModelTests"`
- [ ] If any baseline test already fails, stop and fix that drift before starting remediation work
- [ ] Create a branch dedicated to analyzer cleanup work

**Exit criteria**
- Build is green
- Existing high-value tests are green
- The team is working from one known XML baseline

---

### Task 2: Fix Shared Async Infrastructure First

**Files:**
- Modify: `Modelbouwer/App.xaml.cs`
- Modify: `Modelbouwer/Services/GenericDataService.cs`
- Modify: `Modelbouwer/Helpers/DBCommands.cs`
- Modify: `Modelbouwer/Services/CsvExportService.cs`
- Modify: `Modelbouwer/Services/ExcelExportService.cs`

- [ ] Add `CancellationToken` support to `GenericDataService` entry points first
- [ ] Thread those tokens into DB commands, readers, scalar calls, and transaction helpers
- [ ] Pass explicit tokens into transaction `CommitAsync()` and `RollbackAsync()`
- [ ] Update `App.xaml.cs` so `_host.StartAsync()` and `_host.StopAsync()` receive explicit tokens
- [ ] Update export service async entry points so they can participate in cancellation-aware flows
- [ ] In every touched non-UI async method, make continuation behavior explicit instead of relying on implicit `ConfigureAwait(true)`
- [ ] Run `dotnet build .\ModelbouwWerkbank.slnx -v:minimal`
- [ ] Run any focused tests that hit `GenericDataService`, export flows, or app startup wiring
- [ ] Commit this infrastructure slice independently

**Why this goes first**
- Most downstream tickets depend on these signatures and patterns.
- This removes blocker issues for contract propagation.

**Exit criteria**
- Shared async infrastructure is token-aware
- Transaction calls are analyzer-clean in the touched area
- Downstream callers can start adopting the new signatures

---

### Task 3: Propagate Async Contracts Through Shared CRUD Layers

**Files:**
- Modify: `Modelbouwer/ViewModels/EntityPageViewModel.cs`
- Modify: `Modelbouwer/Interfaces/*.cs` for CRUD-style service contracts
- Modify: `Modelbouwer/Interfaces/IEntityValidator.cs`
- Modify: validator implementations under `Modelbouwer/Validators`
- Modify: representative page viewmodels that no longer compile after the contract update

- [ ] Update service interfaces so async methods accept `CancellationToken`
- [ ] Update validator interfaces and implementations to accept `CancellationToken`
- [ ] Update `EntityPageViewModel` async workflow methods to accept and pass tokens consistently
- [ ] Rename async methods with missing `Async` suffix in shared workflow code where the analyzer calls it out
- [ ] Recompile and fix only the caller surfaces needed to restore a green build
- [ ] Prefer one service-family batch at a time if the compile fallout is too large
- [ ] Run `dotnet build .\ModelbouwWerkbank.slnx -v:minimal`
- [ ] Run page-oriented tests after each family of changes, starting with `ProductPageViewModelTests`
- [ ] Commit the contract propagation slice independently

**Recommended slicing inside this task**
- Slice A: `EntityPageViewModel` + validators
- Slice B: metadata CRUD interfaces and their direct consumers
- Slice C: product/project/worktype interfaces and their direct consumers

**Exit criteria**
- Shared async contracts are token-aware
- Base page workflow compiles cleanly
- The project is ready for feature-specific cleanup on top of the shared layer

---

### Task 4: Clean Up The StockOrder Slice As A Bounded Feature

**Files:**
- Modify: `Modelbouwer/Interfaces/IStockOrderService.cs`
- Modify: `Modelbouwer/Services/StockOrderService.cs`
- Modify: `Modelbouwer/ViewModels/StockOrderViewModel.cs`
- Test: `Modelbouwer.UnitTests/Services/StockOrderServiceTests.cs`
- Test: `Modelbouwer.UnitTests/ViewModels/StockOrderViewModelTests.cs`
- Test: `Modelbouwer.UnitTests/ViewModels/StockOrderProductDialogViewModelTests.cs`

- [ ] Add `CancellationToken` support to `IStockOrderService`
- [ ] Propagate those signatures through `StockOrderService`
- [ ] Update `StockOrderViewModel` async workflows to accept and forward tokens
- [ ] Make continuation behavior explicit in the touched StockOrder methods
- [ ] Keep any behavior-preserving rename separate from logic changes when possible
- [ ] Run `dotnet test .\Modelbouwer.UnitTests\Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrder"`
- [ ] Run `dotnet build .\ModelbouwWerkbank.slnx -v:minimal`
- [ ] Commit the StockOrder slice independently

**Why this is its own task**
- It is a recent, dense analyzer hotspot.
- It already has targeted tests, which makes it a safe early feature slice.

**Exit criteria**
- StockOrder async warnings are materially reduced
- Existing StockOrder tests stay green

---

### Task 5: Apply An Explicit Async Continuation Policy To The Remaining Hotspots

**Files:**
- Modify: high-churn files with `CRR0029` and `CRR0030`
- Priority files:
  - `Modelbouwer/Services/GenericDataService.cs`
  - `Modelbouwer/ViewModels/StockOrderViewModel.cs`
  - `Modelbouwer/Services/StockOrderService.cs`
  - `Modelbouwer/ViewModels/ProductPageViewModel.cs`
  - `Modelbouwer/ViewModels/StockManagementPageViewModel.cs`
  - page code-behind files under `Modelbouwer/Views`

- [ ] Define the rule before editing:
  - UI-bound code may need to resume on the UI context
  - service/data/validator code should avoid accidental UI-context assumptions
- [ ] Sweep service and helper layers first
- [ ] Sweep viewmodels next
- [ ] Sweep code-behind last, because those paths are most likely to need UI-thread continuation
- [ ] Remove redundant `await` findings encountered in the same touched area
- [ ] Run `dotnet build .\ModelbouwWerkbank.slnx -v:minimal`
- [ ] Run focused tests for the slices that changed most
- [ ] Commit the async continuation cleanup independently

**Exit criteria**
- The largest `CRR0029` hotspots are reduced
- The codebase has a consistent async continuation policy in touched files

---

### Task 6: Review And Normalize Naming Inconsistencies Safely

**Files:**
- Modify: `Modelbouwer/Helpers/DBNames.cs`
- Modify: `Modelbouwer/Models/WorkTypeModel.cs`
- Modify: `Modelbouwer/Services/WorkTypeService.cs`
- Modify: `Modelbouwer/ViewModels/WorkTypePageViewModel.cs`
- Modify: `Modelbouwer/Validators/WorkTypeValidator.cs`
- Modify: `Modelbouwer/Interfaces/IWorkTypeService.cs`
- Modify: `Modelbouwer/Models/ExportModel.cs`
- Modify: any direct references to renamed public types

- [ ] Split this task into safe renames and compatibility-sensitive renames
- [ ] Start with file/type mismatches such as `Worktype*` versus `WorkType*`
- [ ] Fix `ExportModel` type/file alignment issues
- [ ] Review `DBNames.cs` spelling issues one group at a time
- [ ] For DB/schema-facing names, decide explicitly:
  - safe internal rename
  - compatibility alias/wrapper
  - defer because it would break runtime contracts
- [ ] Do not batch risky `DBNames.cs` renames with unrelated cleanup
- [ ] Run `dotnet build .\ModelbouwWerkbank.slnx -v:minimal`
- [ ] Run tests around WorkType and any touched feature slices
- [ ] Commit safe naming work independently from any risky compatibility work

**Exit criteria**
- Safe naming inconsistencies are removed
- Risky contract-facing renames are either handled carefully or explicitly deferred

---

### Task 7: Finish With Low-Risk Analyzer Hygiene

**Files:**
- Modify: `Modelbouwer/ViewModels/NavigationViewModel.cs`
- Modify: `Modelbouwer/ViewModels/ProductPageViewModel.cs`
- Modify: `Modelbouwer/Converters/CaptionSummaryRowConverter.cs`
- Modify: `Modelbouwer/Services/CsvImportService.cs`
- Modify: `Modelbouwer/Models/ExportModel.cs`
- Modify: `Modelbouwer/Models/StockManagementModel.cs`
- Modify: small files with `CRR0044`, `CRR0045`, `CRR0046`, `CRR0047`, `CRR0026`, `CRR0042`, `CRR0050`, `CRR0052`

- [ ] Remove unused locals, fields, and parameters where behavior does not change
- [ ] Replace unused locals with discards where that is clearer than deletion
- [ ] Remove redundant field initializations
- [ ] Apply string interpolation cleanup where it improves readability
- [ ] Review string comparison fixes carefully instead of applying them mechanically
- [ ] Move small nested helper/result types to separate files where that improves structure
- [ ] Run `dotnet build .\ModelbouwWerkbank.slnx -v:minimal`
- [ ] Run targeted tests for the touched slices
- [ ] Commit the hygiene pass independently

**Exit criteria**
- Remaining low-risk analyzer noise is reduced
- The final report is easier to read and maintain

---

### Task 8: Final Verification And Report Refresh

**Files:**
- Reference: `C:\Users\hnijk\OneDrive\Downloads\CodeIssuesReport.xml`
- Reference: fresh regenerated report from the same analysis workflow

- [ ] Run `dotnet build .\ModelbouwWerkbank.slnx -v:minimal`
- [ ] Run the most relevant unit test filters:
  - `dotnet test .\Modelbouwer.UnitTests\Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockOrder"`
  - `dotnet test .\Modelbouwer.UnitTests\Modelbouwer.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductPageViewModelTests"`
- [ ] Regenerate the XML analysis report using the same workflow that produced `CodeIssuesReport.xml`
- [ ] Compare:
  - total issue count
  - top issue codes
  - top hotspot files
- [ ] Document what remains intentionally deferred, especially in `DBNames.cs` or other compatibility-sensitive areas

**Done definition**
- Build is green
- Key tests are green
- The fresh report shows a meaningful reduction in total issues and in the top hotspots
- Any deferred items are explicit instead of accidental

---

## Suggested Execution Order

1. Shared async infrastructure
2. Shared CRUD contracts and base page workflows
3. StockOrder bounded cleanup
4. Broader async continuation cleanup
5. Safe naming normalization
6. Low-risk hygiene cleanup
7. Fresh report + compare results

## Suggested Commit Strategy

- Commit once per task, and split a task into smaller commits if compile fallout is large.
- Never combine naming renames and async signature propagation in the same commit unless the rename is unavoidable.
- Prefer commits that are reversible by concern:
  - `refactor: add cancellation tokens to shared data infrastructure`
  - `refactor: propagate cancellation tokens through entity page workflows`
  - `refactor: clean up stock order async analyzer findings`
  - `refactor: make async continuation behavior explicit in service layer`
  - `refactor: normalize work type naming`
  - `chore: clean up low-risk analyzer findings`

## Risks To Watch

- WPF UI code may break if continuation behavior is changed without respecting UI-thread requirements.
- Contract changes can ripple widely through interfaces and viewmodels.
- `DBNames.cs` spelling fixes may accidentally break SQL/view/table mappings.
- StockOrder is a high-change area and should stay covered by focused tests after every slice.

## Practical Recommendation

If the team wants the fastest visible reduction with the lowest risk, start with:

1. Task 2
2. Task 4
3. Task 3

That sequence gives quick impact in the biggest async hotspots while keeping the most regression-sensitive feature slice covered by existing tests.

# Cornwall Dowel Import Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build and execute a one-time importer that reads Cornwall dowel product pages, creates missing `product` records, and upserts `productsupplier` rows for supplier `1`.

**Architecture:** Keep the importer isolated from the WPF UI by adding a focused import service plus a small executable entrypoint. Reuse existing `ProductService`, `SupplierService`, `GenericDataService`, and `DBNames` conventions for database writes. Parse live Cornwall HTML with `HttpClient` and regular expressions tailored to the known page structure.

**Tech Stack:** .NET 10, C#, MySQL via `MySql.Data`, MSTest.

---

## File Structure

**Create**
- `Modelbouwer/Models/CornwallDowelImportItem.cs`
- `Modelbouwer/Services/CornwallDowelImportService.cs`
- `tmp/CornwallDowelImport/CornwallDowelImport.csproj`
- `tmp/CornwallDowelImport/Program.cs`
- `Modelbouwer.UnitTests/Services/CornwallDowelImportServiceTests.cs`

**Modify**
- `Modelbouwer/Interfaces/IProductService.cs`
- `Modelbouwer/Interfaces/ISupplierService.cs`
- `Modelbouwer/Services/ProductService.cs`
- `Modelbouwer/Services/SupplierService.cs`
- `Modelbouwer/App.xaml.cs`

---

### Task 1: Write Failing Tests For Code Generation And Import Decisions

**Files:**
- Create: `Modelbouwer.UnitTests/Services/CornwallDowelImportServiceTests.cs`

- [ ] **Step 1: Write the failing test for product code generation**

Add a test that expects:

```csharp
Assert.AreEqual(
    "CWD-LI-10X1000",
    CornwallDowelImportService.BuildProductCode("Lime Dowel 10mm x 1000mm", new HashSet<string>()));
```

- [ ] **Step 2: Write the failing test for duplicate code suffixing**

Add a test that expects:

```csharp
var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CWD-WA-14X1000" };
Assert.AreEqual(
    "CWD-WA-14X1000-2",
    CornwallDowelImportService.BuildProductCode("Walnut Dowl 14mm x 1000mm", existing));
```

- [ ] **Step 3: Write the failing test for supplier-row upsert choice**

Add a pure decision test that expects an existing supplier row to trigger update instead of insert.

- [ ] **Step 4: Run the tests to verify they fail**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --filter "FullyQualifiedName~CornwallDowelImportServiceTests"
```

Expected:
- FAIL because the importer service does not exist yet.

---

### Task 2: Implement The Import Model And Service

**Files:**
- Create: `Modelbouwer/Models/CornwallDowelImportItem.cs`
- Create: `Modelbouwer/Services/CornwallDowelImportService.cs`

- [ ] **Step 1: Add the import item model**

Create a focused DTO with:
- `Name`
- `ProductNumber`
- `Price`
- `RelativeProductUrl`
- `AbsoluteProductUrl`
- `AbsoluteImageUrl`
- `MaterialCode`
- `GeneratedProductCode`

- [ ] **Step 2: Implement the HTML scraping**

Add methods that:
- fetch the root page
- extract the five dowel subpage urls
- fetch each subpage
- parse product name, product number, price, image and detail url

- [ ] **Step 3: Implement code generation**

Add pure helpers that:
- infer material code from the name
- parse `Nmm x MMMMmm`
- build `CWD-XX-NNXMMMM`
- append `-2`, `-3`, ... when needed

- [ ] **Step 4: Re-run the focused tests**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --filter "FullyQualifiedName~CornwallDowelImportServiceTests"
```

Expected:
- PASS for the pure helper tests.

---

### Task 3: Add Service Support For Product And Supplier Deduplication

**Files:**
- Modify: `Modelbouwer/Interfaces/IProductService.cs`
- Modify: `Modelbouwer/Interfaces/ISupplierService.cs`
- Modify: `Modelbouwer/Services/ProductService.cs`
- Modify: `Modelbouwer/Services/SupplierService.cs`

- [ ] **Step 1: Add focused lookup methods**

Expose methods for:
- get product by exact name
- get product by exact code
- get supplier-product row by supplier id and product id

- [ ] **Step 2: Keep the queries narrow**

Use exact-match SQL with `LIMIT 1` so the importer can make deterministic insert/update decisions.

- [ ] **Step 3: Re-run relevant existing service tests**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --filter "FullyQualifiedName~ProductServiceTests|FullyQualifiedName~SupplierServiceTests"
```

Expected:
- PASS with the new methods added cleanly.

---

### Task 4: Implement Database Import Execution

**Files:**
- Create: `tmp/CornwallDowelImport/CornwallDowelImport.csproj`
- Create: `tmp/CornwallDowelImport/Program.cs`

- [ ] **Step 1: Build a small executable entrypoint**

The program should:
- instantiate `CornwallDowelImportService`
- scrape the live Cornwall data
- load existing products and Cornwall supplier links
- download product images
- create missing products
- upsert `productsupplier` rows for supplier `1`

- [ ] **Step 2: Fill safe default product fields**

For new products use:
- `ProductName` from source
- `ProductCode` generated by importer
- `ProductPrice` from source
- `ProductImage` downloaded bytes
- remaining numeric fields as `0`
- hide flag as `0`

- [ ] **Step 3: Print a run summary**

Show:
- total scraped items
- created products
- reused products
- created supplier rows
- updated supplier rows
- skipped rows if any

- [ ] **Step 4: Build the importer**

Run:

```powershell
dotnet build tmp/CornwallDowelImport/CornwallDowelImport.csproj
```

Expected:
- `Build succeeded`.

---

### Task 5: Verify And Execute The Live Import

**Files:**
- No new files required unless verification reveals a bug

- [ ] **Step 1: Run the full unit test slice**

Run:

```powershell
dotnet test Modelbouwer.UnitTests/Modelbouwer.UnitTests.csproj --filter "FullyQualifiedName~CornwallDowelImportServiceTests"
```

Expected:
- PASS.

- [ ] **Step 2: Execute the importer against the live database**

Run:

```powershell
dotnet run --project tmp/CornwallDowelImport/CornwallDowelImport.csproj
```

Expected:
- live Cornwall scrape succeeds
- inserts and updates complete without SQL errors

- [ ] **Step 3: Verify resulting records**

Check:
- number of Cornwall-linked `productsupplier` rows for supplier `1`
- a sample of created products with name, code and image length
- a sample of updated supplier rows with article number, price and url

- [ ] **Step 4: Summarize the exact outcome**

Report:
- how many source rows were processed
- how many new `product` records were added
- how many existing `product` records were reused
- how many `productsupplier` rows were inserted or updated

---

## Self-Review

**Spec coverage**
- new product creation: covered in Tasks 3 and 4
- supplier `1` links: covered in Tasks 3, 4 and 5
- generated short product codes: covered in Tasks 1 and 2
- live execution: covered in Task 5

**Placeholder scan**
- no `TODO` or deferred implementation markers remain

**Type consistency**
- importer DTO, service helpers and runner all speak in terms of the existing `product` and `productsupplier` schema

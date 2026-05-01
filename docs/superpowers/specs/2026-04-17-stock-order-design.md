# Stock Order Design

**Date:** 2026-04-17

**Goal:** Ontwerp een nieuw bestelbeheer-scherm waarmee een gebruiker een nieuwe bestelling kan opbouwen of een bestaande bestelling kan selecteren en bewerken, zolang de bestelling of de orderregels nog niet volledig gesloten zijn.

## Context

De applicatie gebruikt WPF met MVVM, `CommunityToolkit.Mvvm`, losse services per domein en rechtstreekse MySQL-toegang via `MySql.Data`. Bestaande CRUD-schermen volgen grotendeels het patroon `View` + `PageViewModel` + `Service` + `Model`, met SQL-queries op basis van `DBNames`.

De relevante database-objecten voor dit ontwerp zijn:
- `supplyorder`
- `supplyorderline`
- `productsupplier`
- `product`
- `stocklog`
- `view_supplyorder`
- `view_supplyopenorder`
- `view_supplyorderline`
- `view_supplyopenorderdetails`
- `view_productinventory`

## Gebruikersdoel

De gebruiker moet:
- direct een nieuwe order kunnen invoeren vanuit een leeg detailformulier
- een bestaande order uit een grid kunnen selecteren en vervolgens kunnen bekijken of bewerken
- orderregels kunnen toevoegen vanuit een productcatalogus
- leverancier-specifieke productinformatie kunnen onderhouden tijdens het toevoegen van een orderregel
- nooit een gesloten order kunnen wijzigen

## Kernbeslissingen

### 1. Schermgedrag: new-first

Het scherm opent standaard in nieuwe-order-modus. De detailvelden zijn leeg totdat de gebruiker:
- een nieuwe order begint in te vullen, of
- een bestaande order selecteert in het ordergrid

Een nieuwe order wordt nog niet direct in de database aangemaakt. De order bestaat eerst alleen in memory en wordt pas opgeslagen wanneer de gebruiker expliciet kiest voor opslaan. Dit voorkomt ongewenste lege records en maakt een rollback-mechanisme overbodig.

### 2. Hoofdindeling van de view

Het scherm krijgt vier functionele zones:
- Toolbar met acties zoals `Nieuw`, `Opslaan`, `Verwijderen` en `Reset`
- Detailsectie met tabs:
  - `Orderinformatie`
  - `Orderregels`
  - `Memo`
- Grid met bestaande orders
- Grid met beschikbare producten om aan de order toe te voegen

Deze opzet ondersteunt zowel nieuwe orders als het terugzoeken en aanpassen van bestaande orders binnen één scherm.

### 3. Closed orders zijn volledig read-only

Wanneer een geselecteerde order `Closed = 1` heeft, is de volledige detailsectie read-only:
- orderinformatie
- memo
- orderregels
- toevoegen/verwijderen van regels

Hiermee sluit de UI direct aan op de bedrijfsregel dat een gesloten bestelling niet meer mag worden aangepast.

### 4. Product toevoegen via popup

De onderste productgrid fungeert als catalogus. Een gebruiker voegt geen orderregel rechtstreeks inline toe, maar via een popup.

De popup toont leverancier-specifieke artikelinformatie op basis van de combinatie:
- `Product_Id`
- `Supplier_Id`

Bronvolgorde:
1. eerst `productsupplier`
2. als leverancier-specifieke velden leeg zijn: fallback naar `product`
3. als er nog geen `productsupplier`-record bestaat voor deze leverancier en dit product, wordt dat record bij bevestigen aangemaakt

De popup beheert dus zowel:
- de informatie voor de nieuwe of gewijzigde orderregel
- als de update van de `productsupplier`-informatie voor toekomstig gebruik

`DefaultSupplier` blijft bewust buiten scope voor deze view.

### 5. Supplier-filter op ordergrid als bonus

Wanneer in het detailgedeelte al een leverancier is gekozen, mag het ordergrid optioneel gefilterd worden op orders van die leverancier. Dit is een UX-bonus, geen harde voorwaarde. Zonder gekozen leverancier blijft het ordergrid alle relevante orders tonen.

## Informatiearchitectuur

### Tab 1: Orderinformatie

Deze tab bevat de headergegevens van de bestelling, minimaal:
- leverancier
- valuta
- bestelnummer
- besteldatum
- verzendkosten
- orderkosten
- gesloten-status
- eventuele afgeleide totalen indien gewenst

Bij een nieuwe order zijn deze velden leeg of op logische defaults ingesteld.

### Tab 2: Orderregels

Deze tab toont de regels van de huidige order in een aparte grid. Vanuit deze tab moet de gebruiker regels kunnen:
- bekijken
- bewerken
- verwijderen

Nieuwe regels worden toegevoegd via de onderste productgrid en de popup.

Als de order nog niet is opgeslagen, worden de regels tijdelijk in memory bijgehouden en pas bij opslaan van de order definitief weggeschreven.

### Tab 3: Memo

Deze tab bevat uitsluitend het `Memo`-veld van de orderheader.

## Domeinmodel

### StockOrderModel

Verantwoordelijk voor de orderheader uit `supplyorder`, met ten minste:
- `Id`
- `Supplier_Id`
- `Currency_Id`
- `OrderNumber`
- `OrderDate`
- `ShippingCosts`
- `OrderCosts`
- `Memo`
- `Closed`
- `ClosedDate`

Voor weergave in grids kunnen aanvullende velden uit `view_supplyorder` of `view_supplyopenorder` gebruikt worden, zoals suppliernaam en valuta-informatie.

### StockOrderLineModel

Verantwoordelijk voor de orderregel uit `supplyorderline`, met ten minste:
- `Id`
- `Supplyorder_Id`
- `Product_Id`
- `SupplierProductName`
- `Amount`
- `OpenAmount`
- `Price`
- `RealRowTotal`
- `Closed`
- `ClosedDate`

Voor gridweergave zijn extra velden uit `view_supplyorderline` bruikbaar, zoals productcode, productnaam, ontvangen hoeveelheid en verwachte hoeveelheid.

### StockOrderProductDialogModel

Popupmodel dat leverancier-specifieke artikeldata en orderregeldata combineert, bijvoorbeeld:
- `Supplier_Id`
- `Product_Id`
- `SupplierProductNumber`
- `SupplierProductName`
- `UnitPrice`
- `Amount`
- `RowTotal`
- fallbackvelden vanuit `product`

Dit model is bedoeld als bewerkcontext voor de popup en niet als directe 1-op-1 representatie van één database-object.

## Service-ontwerp

### StockOrderService

Verantwoordelijkheden:
- laden van orderlijst
- laden van orderheader
- laden van orderregels
- inserten van nieuwe order
- updaten van bestaande order
- verwijderen van order indien toegestaan
- opslaan en verwijderen van orderregels

De service werkt rechtstreeks op `supplyorder`, `supplyorderline` en de relevante views.

### ProductSupplierService

Verantwoordelijkheden:
- ophalen van leverancier-specifieke productinformatie voor `Product_Id + Supplier_Id`
- bepalen van fallbackwaarden vanuit `product`
- inserten van een nieuw `productsupplier`-record wanneer nodig
- updaten van leverancier-specifieke artikeldata wanneer de popup bevestigd wordt

### Eventuele workflowlaag

Alleen indien nodig kan een kleine workflow- of orchestration-service worden toegevoegd om deze flow te centraliseren:
1. valideer dat een leverancier gekozen is
2. open popup met supplier/product-combinatie
3. insert of update `productsupplier`
4. voeg orderregel toe aan memory of direct aan database, afhankelijk van de status van de order

Als de bestaande codebase liever slank blijft, mag deze logica ook in het viewmodel blijven zolang verantwoordelijkheden helder blijven.

## ViewModel-ontwerp

Het scherm past functioneel minder goed in het generieke `EntityPageViewModel<T>`-patroon, omdat het zowel een orderheader als meerdere orderregels en een productcatalogus beheert. Daarom krijgt dit scherm een dedicated `StockOrderPageViewModel`.

Belangrijke collecties en state:
- `ObservableCollection<StockOrderModel> Orders`
- `ObservableCollection<StockOrderLineModel> OrderLines`
- `ObservableCollection<StockOrderLineModel> PendingOrderLines`
- `ObservableCollection<ProductModel> AvailableProducts`
- `StockOrderModel EditableOrder`
- `StockOrderModel? SelectedOrder`
- `ProductModel? SelectedProduct`
- `SupplierModel? SelectedSupplier`
- `bool IsNewOrder`
- `bool IsClosedOrder`
- `bool CanEditOrder`

Belangrijke commands:
- `NewOrderCommand`
- `SaveOrderCommand`
- `DeleteOrderCommand`
- `ResetOrderCommand`
- `AddProductToOrderCommand`
- `EditOrderLineCommand`
- `DeleteOrderLineCommand`

## Opslagflow

### Nieuwe order

1. gebruiker vult orderinformatie in
2. gebruiker voegt eventueel al orderregels toe
3. order en regels bestaan voorlopig alleen in memory
4. bij `Opslaan`:
   - insert orderheader in `supplyorder`
   - haal nieuwe `Id` op
   - schrijf alle pending orderregels weg naar `supplyorderline`
   - laad de order opnieuw in edit-modus

### Bestaande order

1. gebruiker selecteert een order uit het ordergrid
2. header en regels worden geladen
3. wijzigingen in header en memo worden op save geüpdatet
4. nieuwe regels worden direct aan die bestaande order gekoppeld

### Product toevoegen

1. gebruiker selecteert product in productgrid
2. systeem controleert of een leverancier gekozen is
3. popup wordt gevuld met `productsupplier`-data en fallbackwaarden uit `product`
4. gebruiker bevestigt popup
5. `productsupplier` wordt direct insert of update uitgevoerd
6. orderregel wordt toegevoegd:
   - aan memory als order nog nieuw is
   - direct in `supplyorderline` als order al bestaat

## Validatie en foutafhandeling

Minimale validatieregels:
- leverancier is verplicht voordat een product kan worden toegevoegd
- bestelnummer en besteldatum zijn verplicht voordat een order opgeslagen kan worden
- een gesloten order mag niet worden opgeslagen, gewijzigd of uitgebreid
- aantal en prijs in de popup moeten valide en groter dan nul kunnen zijn wanneer het domein dat vereist

Foutmeldingen moeten aansluiten op de bestaande WPF-patronen met duidelijke `MessageBox`-feedback.

## Teststrategie

De implementatie moet gericht getest worden op:
- nieuwe order in memory opbouwen en pas op save wegschrijven
- bestaande order laden en wijzigen
- gesloten order read-only maken
- popup-fallback van `productsupplier` naar `product`
- automatisch aanmaken van `productsupplier` bij onbekende leverancier/product-combinatie
- supplier-filter op het ordergrid

## Scopegrens

Bewust buiten scope:
- beheer van `DefaultSupplier`
- ontvangstverwerking van bestellingen
- automatische sluiting van orders op basis van ontvangsten
- uitgebreide rapportage

Deze view richt zich uitsluitend op orderinvoer en orderbeheer.

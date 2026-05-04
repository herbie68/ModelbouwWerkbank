# Cornwall Dowel Import Design

**Date:** 2026-05-01

**Goal:** Importeer alle Cornwall dowel-producten vanaf de opgegeven hoofd- en subpagina's naar de lokale `product`-tabel en maak daarna voor supplier `1` (`Cornwall`) een gekoppelde `productsupplier`-regel aan met leverancier-specifieke gegevens.

## Context

De applicatie gebruikt WPF met MVVM en schrijft rechtstreeks naar MySQL via `MySql.Data`. Productgegevens zijn opgesplitst over:
- `product` voor het hoofdartikel, inclusief `Image`
- `productsupplier` voor leverancier-specifieke velden zoals artikelnummer, prijs en url

De bron is:
- `https://www.cornwallmodelboats.co.uk/acatalog/Model-Boat-Timber-Wood-Dowels.html`

De hoofdpagina verwijst naar vijf relevante subpagina's:
- `Balsa_Dowel.html`
- `birch-dowel.html`
- `lime_dowel.html`
- `Sapelli-Dowel.html`
- `walnut_dowel.html`

## Goedgekeurde werkwijze

### 1. Bronbereik

Alle producten op de vijf subpagina's worden verwerkt. Daarbij nemen we de leverancierstekst zoals zichtbaar op de pagina over, inclusief kleine bronafwijkingen zoals `Dowl` in plaats van `Dowel` wanneer die op Cornwall zelf zo staat.

### 2. Productaanmaak

Voor ieder Cornwall-item controleren we eerst of er al een `product` bestaat met exact dezelfde productnaam.

- Bestaat het product al, dan gebruiken we dat bestaande `product`
- Bestaat het product nog niet, dan maken we een nieuw `product` aan

Nieuwe producten krijgen:
- `ProductName` uit Cornwall
- `ProductCode` als korte gegenereerde code
- `ProductPrice` gelijk aan de Cornwall-prijs
- `ProductImage` gevuld met de Cornwall-afbeelding
- overige velden gevuld met veilige defaults die door de bestaande validator worden geaccepteerd

### 3. Supplier-koppeling

Na het bepalen of aanmaken van het `product` zorgen we dat er een `productsupplier`-regel voor supplier `1` bestaat.

Die regel bevat:
- `Product_Id` verwijzend naar het juiste product
- `Supplier_Id = 1`
- `ProductNumber` uit het Cornwall artikelnummer
- `ProductName` uit de Cornwall productnaam
- `Price` uit de Cornwall prijs
- `Url` naar de detailpagina van het Cornwall product

Wanneer voor deze combinatie al een `productsupplier`-record bestaat, wordt dat bijgewerkt in plaats van dubbel aangemaakt.

### 4. Productcodeformaat

De korte productcode volgt dit patroon:

`CWD-<materiaal>-<maat>`

Voorbeelden:
- `CWD-BA-05X915`
- `CWD-BI-03X900`
- `CWD-LI-10X1000`
- `CWD-WA-14X1000`

Waarbij:
- `CWD` = Cornwall Dowel
- materiaal = `BA`, `BI`, `LI`, `SA`, `WA`, `RA`
- maat wordt opgebouwd uit diameter en lengte in millimeters

Als een gegenereerde code al bestaat, krijgt hij een korte numerieke suffix zodat de code uniek blijft.

### 5. Importgedrag

De import moet idempotent genoeg zijn voor hergebruik:
- geen dubbel `product` op basis van exact gelijke naam
- geen dubbele `productsupplier` voor dezelfde combinatie van supplier en product
- bestaande Cornwall leverancierinformatie mag worden ververst met actuele prijs, artikelnummer, url en naam

### 6. Uitvoering

We bouwen een eenmalige importer in de repo die:
- live de Cornwall subpagina's uitleest
- products data normaliseert
- images downloadt
- database inserts/updates uitvoert
- na afloop een samenvatting geeft van:
  - nieuwe producten
  - hergebruikte bestaande producten
  - nieuwe supplier-links
  - bijgewerkte supplier-links

## Teststrategie

Voor implementatie testen we minimaal:
- parsing van materiaal, diameter en lengte voor codegeneratie
- codegeneratie voor representatieve Cornwall namen
- deduplicatiebeslissingen voor bestaand product op naam
- keuze tussen insert en update voor `productsupplier`

## Scopegrens

Bewust buiten scope:
- generieke leverancier-import voor andere shops
- UI-workflow in de WPF app
- aanpassing van categories, brands, storage locations of units op basis van externe taxonomie

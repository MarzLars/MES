# MES – Interaktiv terminalapplikasjon

En rik, domenedrevet C#-terminalapplikasjon som håndterer arbeidsflyten for produksjonsordrer for stålprodukter.
Bygget med .NET 10 og Entity Framework Core, med vekt på deklarativ design.

## Tech stack

- **Plattform**: .NET 10
- **ORM**: Entity Framework Core 10
- **Database**: SQLite (lokal filbasert)
- **Språk**: C#

---

Applikasjonen initialiserer automatisk en SQLite-database (`mes.db`) og fyller den med startdata for produkter hvis den ikke allerede finnes.

## Kjøring av applikasjonen

Sørg for at du har nyeste .NET 10 SDK installert.

```powershell
dotnet run --project src/MES
```

### Hovedmenygrensesnitt

Når du starter, vil du se følgende meny:

```text
=== Manufacturing Execution System (MES) ===

Available Actions:
1. List Products
2. Create Project
3. Create Work Order
4. View Work Order
5. Exit

Select an option:
```

---

## Funksjoner

- **Interaktiv terminal**: Et enkelt meny-grensesnitt for å administrere produkter, prosjekter og arbeidsordrer.
- **Rich Domain Model**: Innekapslet "forretningslogikk" i `record`-typene for å sikre tilstandskonsistens og invariantvalidering.
- **Entity Framework Core 10**: Datalagring med SQLite, med fokus på deklarative spørringsmønstre.

---

## Prosjektstruktur

```text
src/MES/
  Models/          – Domenerekorder med rik logikk (Product, Project, WorkOrder, WorkOrderLine)
  Commands/        – Skrivende operasjoner som gir sideeffekter (CreateProject, CreateWorkOrder)
  Queries/         – Lesende operasjoner (GetWorkOrder)
  Data/            – Entity Framework Core DbContext (ManufacturingDbContext)
  TerminalInterface.cs – Hoved interaktive applikasjonsløkken og UI-logikk
  Program.cs       – Rent inngangspunkt for oppstart og initiering (seeding)
```

---

## Designfilosofi

### Rik domenemodell

Applikasjonen prøver å bruke en **Rich Domain Model**. 'Business rules' og tilstandsoverganger er innkapslet i hvert sitt domene (ved bruk av records). For eksempel:

- **Validering**: Enheter som vekt og kvantitet valideres ved opprettelse (f.eks. må være større enn 0).
- **Innkapslede samlinger**: Arbeidsordrelinjer administreres gjennom metoder på `WorkOrder`-recordet, og forhindrer ekstern manipulering av den underliggende samlingen.
- **Beregnede Properties**: Totale vekter beregnes dynamisk av domenobjektene.

### Deklarativ design

Logikk uttrykkes **deklarativt**:

- **LINQ**: Brukes i stor grad for å beskrive *hvilken* data som skal hentes og hvordan den skal transformeres.

---

## Dataskjema

Systemet bruker følgende relasjonsstruktur:

| Tabell           | Formål                                                                 |
|------------------|------------------------------------------------------------------------|
| `Product`        | Produktkatalog for stål med spesifikke enhetsvekter.                   |
| `Project`        | Grupperer en eller flere arbeidsordrer for sporing.                    |
| `WorkOrder`      | Hoved-arbeidsordre som tilhører et prosjekt.                           |
| `WorkOrderLine`  | Individuelle elementer som kobler en arbeidsordre til et produkt.      |

---

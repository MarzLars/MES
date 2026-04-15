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
dotnet run --project src
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

- **Interaktiv terminal**: Enkel terminal grensesnitt for å administrere produkter, prosjekter og arbeidsordrer.
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

| Tabell           | Formål og designvalg                                                                 |
|------------------|--------------------------------------------------------------------------------------|
| `Product`        | Sentral katalog med metadata og enhetsvekter for å sikre konsistente beregninger.    |
| `Project`        | Logisk gruppering for å spore arbeidsordrer mot spesifikke kundeoppdrag.             |
| `WorkOrder`      | Master-entitet som knytter en bestilling til et prosjekt.                            |
| `WorkOrderLine`  | Detail-entitet som tillater flere produkter per ordre (master-detail struktur).      |

---

# Hva jeg ville gjort annerledes med mer tid:
- **UI**: Bruke Blazor eller enkel ASP.NET for å lage et mer brukervennlig grensesnitt, i stedet for en ren terminalapplikasjon.
- **EF Core migrations**: Implementere EF Core migrations for å håndtere databaseendringer mer robust, i stedet for å slette og gjenopprette databasen ved schema mismatch.
- **Lagerstatus og validering**: Implementere logikk for lagerbeholdning. Systemet har i dag validering av input (f.eks. positive tall), men mangler sjekk mot faktisk lagerstatus.
- **API-endepunkter**: Eksponere funksjonalitet via et REST API (f.eks. ASP.NET Core) for integrasjon med andre systemer. Per nå er alt integrert i en terminal-app.
- **Unit testing**: Legge til automatiserte tester for domenemodellen og kommandoer for å sikre stabilitet over tid.
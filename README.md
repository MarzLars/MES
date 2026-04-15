# MES

MES er en .NET 10 minimal API-applikasjon for bestilling av stålprodukter, bygget med EF Core, SQLite og et enkelt interaktivt dashboard i nettleseren.

## Teknologi

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core 10
- SQLite

## Oppstart

Databasefilen heter `mes.db` og blir opprettet ved oppstart ved siden av kjørbar fil.

```powershell
dotnet run --project src/SteelOrdering.csproj
```

Når appen starter, åpner du dashboardet i nettleseren på localhost-URL-en som vises i terminalen.

## Endepunkter

- `GET /` – dashboard
- `GET /health` – helsesjekk
- `POST /test/seed` – fyll inn demo-data
- `GET /products`
- `GET /projects`
- `POST /projects`
- `GET /work-orders`
- `POST /work-orders`
- `GET /work-orders/{workOrderId}`

## Kort om designfilosofi

Målet i dette prosjektet er å gjøre det enkelt å uttrykke riktige tilstander, og vanskelig å bygge feilaktige objekter. Derfor ligger reglene så nær domenet som mulig, mens API-laget mest oversetter HTTP-innputt til trygge domenetyper. Resultatet er mindre tilfeldighet, færre null-sjekker og tydeligere ansvar mellom lesing, skriving og presentasjon.

## Regler som brukes mest i prosjektet

| Regel | Hvorfor | Eksempel i koden |
| --- | --- | --- |
| Uforanderlige typer først | Uforanderlige objekter er enklere å forstå, teste og sende rundt uten bivirkninger. | `WorkOrderLineSpec` er en `record struct`, og `WorkOrder.OrderLines` eksponeres som `IReadOnlyCollection<WorkOrderLine>`. |
| Records for data og verdiobjekter | `record` passer godt for små kontrakter og verdiobjekter fordi modellen blir kort og uttrykksfull. | `CreateWorkOrderRequest(int ProjectId, IReadOnlyCollection<CreateWorkOrderLineRequest> Lines)` og `ProductId`. |
| Gjør ugyldige tilstander umulige | Domenet skal stoppe feil tidlig, ikke la dem spre seg videre. | `WorkOrderFactory.Create(...)` krever et eksisterende prosjekt og minst én linje, og `ProjectFactory.Create(...)` avviser slutt-dato før start-dato. |
| Bruk standard exceptions i factory-metoder | Standard exceptions gir konsistent validering og gjør det lett å mappe feil til `ValidationProblem`. | `QuantityFactory.Create(value)` bruker `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value)`. |
| Minimal APIs og tynne handlers | Endepunktene skal være små og kun koordinere input, spørringer og kommandoer. | `EndpointMappings.MapSteelOrderingEndpoints()` grupperer `/products`, `/projects` og `/work-orders`, mens `WorkOrderHandlers.Create(...)` bare orkestrerer. |
| CQRS for lesing og skriving | Lesemodeller kan optimaliseres uavhengig av skriving. | `GetProductsQueryHandler` bruker `AsNoTracking()` og projiserer direkte til `ProductResponse`, mens `WorkOrderHandlers.Create(...)` håndterer opprettelse. |
| Atferd i egne extension-metoder | Små domeneberegninger holdes nær typen uten å gjøre modellene tunge. | `workOrder.GetTotalWeightInKilograms()` og `workOrderLine.GetTotalLineWeightInKilograms()`. |
| Kort og moderne C#-syntaks | Koden skal være så kort som mulig uten å miste lesbarhet. | Eksempler er `public sealed record CreateProjectRequest(string Name);`, collection expressions som `[]`, og små expression-bodied metoder. |

### Praktiske eksempler fra prosjektet

- `ProductFactory.Create(name, unitWeightKilograms)` setter `InventoryStatus` til `OutOfStock` som standard.
- `WorkOrderLineFactory.Create(product, quantity)` bruker produktets `UnitWeightKilograms` direkte, slik at linjen blir komplett med én gang.
- `CreateWorkOrderRequest` oversettes til `WorkOrderLineSpec` i API-laget før domenet får objektet.
- `GetWorkOrderByIdQueryHandler` leser med `AsNoTracking()` og henter bare det som trengs til responsen.

## Struktur

```text
src/
  Program.cs
  Data/
  Domain/
  Api/
  wwwroot/
```

## Merknader

- Domenet bruker uforanderlige typer og value objects for å holde ugyldige tilstander ute av modellen. For eksempel kan ikke en `WorkOrder` opprettes uten et gyldig `Project`.
- EF Core-konfigurasjonen ligger i `src/Data/ManufacturingDbContext.cs`.
- Databaseskjemaet opprettes automatisk ved oppstart fra EF Core-modellen.
- Seed-logikken ligger i `src/Data/DataSeed.cs`.
- Et eksempel på SQLite-export ligger i `examples/steel-ordering-sample.sql`.

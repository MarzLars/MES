# MES – Mini Order System for Steel Products

A minimal C# console application that models a manufacturing-execution order workflow for steel products.

---

## Running the demo

```bash
dotnet run --project src/MES
```

No external database setup is required. The app creates an SQLite file (`mes.db`) in the working directory on first run.

**Expected output**

```
=== MES Mini Order System ===

Created project   Id=1
Created work order Id=1

Work order #1  (Project: Bridge Renovation 2025)
Product                Qty     kg/unit     Line kg
----------------------------------------------------
HEA 200 Beam            10       42.30      423.00
S355 Steel Plate         5       78.50      392.50
----------------------------------------------------
Total weight (kg)                           815.50
```

---

## Project structure

```
src/MES/
  Models/          – Immutable domain records (Product, Project, WorkOrder, OrderLine)
  Commands/        – Side-effecting write operations (CreateProject, CreateWorkOrder)
  Queries/         – Read-only operations (GetWorkOrder)
  Data/            – Connection factory + schema initialiser
  Program.cs       – Demo driver
sql/schema.sql     – SQL Server-dialect DDL + sample query
```

---

## Design choices

### Declarative style

Logic is expressed **declaratively** rather than imperatively:

* Domain objects are C# `record` types — immutable value objects whose meaning is captured by their shape, not by mutation.
* LINQ projections (`.Select(…).ToList()`) describe *what* data is needed rather than *how* to iterate.
* SQL does the heavy lifting for joins, filtering and sorting (see next point).

### Sorting in SQL (`ORDER BY`)

Sorting is intentionally delegated to the database with `ORDER BY ol.Id`.
Philosophically, sorting is a query concern that databases have supported for decades, and they do it optimally (index scans, merge joins, etc.).
Reimplementing sorting in application code would add accidental complexity with no benefit.

### Command / Query Separation (CQS)

All write operations live in `Commands/` and all read operations in `Queries/`.
Commands return **only the generated Id** of the newly created entity — not a full query result — which is the standard CQS corollary for child-object creation:

> *It is acceptable for a command to return a reference (e.g. an ID) needed for the caller's bookkeeping, as long as the return value does not qualify as a query result in its own right.*

This prevents callers from using commands as a backdoor query mechanism, keeping side effects explicit.

### Data access – Dapper

[Dapper](https://github.com/DapperLib/Dapper) is a thin micro-ORM that lets SQL remain the first-class language for data access while still providing clean C# mapping.
It was chosen over Entity Framework Core because:

* The SQL is short, explicit and easy to audit.
* There is no object-graph tracking needed for this simple domain.
* It reinforces the declarative philosophy: you write the query you mean.

### SQLite for the demo / SQL Server for production

The demo uses **SQLite** (via `Microsoft.Data.Sqlite`) so it runs without any server setup.
The `sql/schema.sql` file contains equivalent **SQL Server (T-SQL)** DDL ready for a production environment; the only changes required are the connection string and swapping `Microsoft.Data.Sqlite` for `Microsoft.Data.SqlClient`.

---

## SQL schema

See [`sql/schema.sql`](sql/schema.sql) for the full DDL and a sample analytic query that returns per-line and total weight using a window function.

| Table       | Purpose                                         |
|-------------|-------------------------------------------------|
| `Product`   | Steel product catalogue with weight per unit    |
| `Project`   | Groups one or more work orders                  |
| `WorkOrder` | Belongs to a project; has multiple order lines  |
| `OrderLine` | Links a work order to a product with a quantity |

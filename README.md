# Manufacturing Execution System (MES) – Interactive Terminal Application

A rich domain-driven C# terminal application that manages a manufacturing-execution order workflow for steel products. Built with .NET 10 and Entity Framework Core, emphasizing declarative design and descriptive naming.

---

## Features

- **Interactive Terminal Loop**: A menu-driven interface for managing products, projects, and work orders.
- **Rich Domain Model**: Encapsulated business logic within records to ensure state consistency and invariant validation.
- **Entity Framework Core 10**: Robust data persistence using SQLite with a focus on declarative query patterns.
- **Descriptive Naming**: Elimination of shorthands across the codebase for improved readability and maintainability.

---

## Running the Application

Ensure you have the latest .NET 10 SDK installed.

```powershell
dotnet run --project src/MES
```

The application automatically initializes an SQLite database (`mes.db`) and seeds it with initial product data if it doesn't already exist.

### Main Menu Interface

Upon starting, you'll see the following menu:

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

## Project Structure

```text
src/MES/
  Models/          – Rich domain records (Product, Project, WorkOrder, WorkOrderLine)
  Commands/        – Side-effecting write operations (CreateProject, CreateWorkOrder)
  Queries/         – Read-only operations (GetWorkOrder)
  Data/            – Entity Framework Core DbContext (ManufacturingDbContext)
  TerminalInterface.cs – Main interactive application loop and UI logic
  Program.cs       – Clean entry point for bootstrapping and seeding
```

---

## Design Philosophy

### Rich Domain Design vs. Anemic Domain

The application uses a **Rich Domain Model** instead of an anemic one. Business rules and state transitions are encapsulated within the domain records themselves. For example:
- **Validation**: Units like weight and quantity are validated upon creation (e.g., must be greater than zero).
- **Encapsulated Collections**: Work order lines are managed through methods on the `WorkOrder` record, preventing external manipulation of the underlying collection.
- **Computed Properties**: Total weights are calculated dynamically by the domain objects.

### Declarative Design

Logic is expressed **declaratively**:
- **Entity Framework Core**: Replaces Dapper to allow for expressive, type-safe queries and automatic object-graph mapping.
- **LINQ**: Used extensively to describe *what* data should be retrieved or how it should be transformed.

### Descriptive Naming Convention

Following a strict rule of **avoiding shorthands**, all properties and variables are named descriptively:
- `ProductId`, `ProjectId` (using full names instead of just `Id` where appropriate).
- `Quantity` instead of `Qty`.
- `WeightInKilogramsPerUnit` instead of `WeightKg`.
- `TotalLineWeightInKilograms` instead of `TotalWeight`.

### Latest C# Features & Records

The project leverages the latest **C#** features and **records** to provide a concise syntax for immutable properties while allowing for rich behavioral logic inside the domain objects.

---

## Data Schema

The system uses the following relational structure:

| Table            | Purpose                                                       |
|------------------|---------------------------------------------------------------|
| `Product`        | Steel product catalogue with specific unit weights.           |
| `Project`        | Groups one or more work orders for tracking.                  |
| `WorkOrder`      | Main order entity belonging to a project.                     |
| `WorkOrderLine`  | Individual line items linking a work order to a product.      |

---

## Technical Stack

- **Platform**: .NET 10
- **ORM**: Entity Framework Core 10
- **Database**: SQLite (local file-based)
- **Language**: C# (utilizing latest features)

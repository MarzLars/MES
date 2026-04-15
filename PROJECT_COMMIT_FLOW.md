# Project Commit Flow: Manufacturing Execution System (MES)

This document outlines a logical, sequential flow for committing the MES codebase into a new repository, simulating a natural development evolution from core domain concepts to a fully functional terminal application.

---

### Phase 1: Foundation & Project Setup
Establish the project structure and essential configuration.

1.  **Commit 1: Repository Initialization & Project Setup**
    -   Files: `MES.slnx`, `src/MES/MES.csproj`, `README.md` (initial version).
    -   *Purpose*: Initialize the .NET 10 project and solution, referencing required EF Core 10 packages for SQLite.

---

### Phase 2: Rich Domain Layer
Define the core business entities and logic without external dependencies.

2.  **Commit 2: Core Independent Entities**
    -   Files: `src/MES/Models/Product.cs`, `src/MES/Models/Project.cs`.
    -   *Purpose*: Implement the basic domain entities for Products (e.g., steel beams) and Projects with rich validation in their constructors.

3.  **Commit 3: Composite Domain Entities**
    -   Files: `src/MES/Models/WorkOrderLine.cs`, `src/MES/Models/WorkOrder.cs`.
    -   *Purpose*: Introduce Work Orders and their line items, including encapsulated logic for weight calculations and collection management.

---

### Phase 3: Infrastructure & Data Access
Connect the domain models to a persistent data store using EF Core 10.

4.  **Commit 4: Entity Framework Core Infrastructure**
    -   Files: `src/MES/Data/ManufacturingDbContext.cs`.
    -   *Purpose*: Define the `ManufacturingDbContext` to map domain records to SQLite tables using a declarative approach with Fluent API configurations.

---

### Phase 4: Application Logic (CQRS-lite)
Implement the "Write" and "Read" operations that interact with the data layer.

5.  **Commit 5: Application Commands**
    -   Files: `src/MES/Commands/CreateProjectCommand.cs`, `src/MES/Commands/CreateWorkOrderCommand.cs`.
    -   *Purpose*: Create side-effecting operations for adding new Projects and Work Orders to the system.

6.  **Commit 6: Application Queries**
    -   Files: `src/MES/Queries/GetWorkOrderQuery.cs`.
    -   *Purpose*: Implement read-only operations for retrieving detailed Work Order information, including navigation property loading.

---

### Phase 5: User Interface & Integration
Develop the final layer that exposes functionality to the user.

7.  **Commit 7: Interactive Terminal Interface**
    -   Files: `src/MES/TerminalInterface.cs`.
    -   *Purpose*: Develop the main interactive terminal loop, providing a menu-driven CLI for user interaction.

8.  **Commit 8: Bootstrapping & Entry Point**
    -   Files: `src/MES/Program.cs`.
    -   *Purpose*: Finalize the application by connecting all components, handling database initialization/seeding, and launching the UI.

9.  **Commit 9: Final Documentation**
    -   Files: `README.md` (final version).
    -   *Purpose*: Complete the project documentation with usage instructions, design philosophy, and technical stack details.

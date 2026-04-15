using MES;
using MES.Data;
using MES.Models;

//Bootstrap
using var manufacturingDbContext = new ManufacturingDbContext();

try
{
    manufacturingDbContext.Database.EnsureCreated();
    // Verify the schema is compatible (especially if migrating from previous Dapper versions)
    _ = manufacturingDbContext.Products.Any();
}
catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("no such table"))
{
    // Schema mismatch detected (common when migrating from older Dapper/SQLite versions).
    // Re-initialize for this simple terminal application.
    manufacturingDbContext.Database.EnsureDeleted();
    manufacturingDbContext.Database.EnsureCreated();
}

//Seed Data (Idempotent)
SeedInitialManufacturingData(manufacturingDbContext);

// Start Application
new TerminalInterface(manufacturingDbContext).Run();

// Helpers
static void SeedInitialManufacturingData(ManufacturingDbContext manufacturingDbContext)
{
    if (manufacturingDbContext.Products.Any()) return;

    manufacturingDbContext.Products.AddRange(
        new Product("HEA 200 Beam", 42.3m),
        new Product("S355 Steel Plate", 78.5m),
        new Product("IPE 300 Beam", 42.2m)
    );
    
    manufacturingDbContext.SaveChanges();
}
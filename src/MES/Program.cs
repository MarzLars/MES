using MES;
using MES.Data;
using MES.Models;

//Bootstrap
using var manufacturingDbContext = new ManufacturingDbContext();

try
{
    manufacturingDbContext.Database.EnsureCreated();
    // Verify the schema is compatible (e.g., after code changes or migrations).
    _ = manufacturingDbContext.Products.Any();
}
catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("no such table"))
{
    // Quick fix for: "no such table" errors due to schema mismatches or missing migrations.
    // Re-initialize for this terminal application.
    manufacturingDbContext.Database.EnsureDeleted();
    manufacturingDbContext.Database.EnsureCreated();
}

//Seed Data
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
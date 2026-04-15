using MES;
using MES.Data;
using MES.Models;

//Bootstrap
using var manufacturingDbContext = new ManufacturingDbContext();
manufacturingDbContext.Database.EnsureCreated();

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
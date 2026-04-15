using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SteelOrdering.Domain.Entities;
using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Data;

public sealed class DataSeed(
    ManufacturingDbContext dbContext)
{
    public async Task EnsureSchemaAndSeedProductsAsync(CancellationToken cancellationToken = default) {
        try {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            _ = await dbContext.Products
                .Select(product => product.Id)
                .FirstOrDefaultAsync(cancellationToken);
            _ = await dbContext.WorkOrderLines
                .Select(line => line.UnitWeightKilograms)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (SqliteException exception) when (exception.Message.Contains("no such column",
                                                    StringComparison.OrdinalIgnoreCase) ||
                                                exception.Message.Contains("no such table",
                                                    StringComparison.OrdinalIgnoreCase)) {
            // If the database schema is inconsistent, start fresh in dev.
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (await dbContext.Products.AnyAsync(cancellationToken)) return;

        dbContext.Products.AddRange(
            ProductFactory.Create(ProductNameFactory.Create("HEA 200 Beam"), UnitWeightKilogramsFactory.Create(42.3m),
                ProductInventoryStatusFactory.CreateInStock()),
            ProductFactory.Create(ProductNameFactory.Create("S355 Steel Plate"), UnitWeightKilogramsFactory.Create(78.5m),
                ProductInventoryStatusFactory.CreateInStock()),
            ProductFactory.Create(ProductNameFactory.Create("IPE 300 Beam"), UnitWeightKilogramsFactory.Create(42.2m))
        );

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SeedOrders(CancellationToken cancellationToken = default) {
        await EnsureSchemaAndSeedProductsAsync(cancellationToken);

        if (!await dbContext.Projects.AnyAsync(cancellationToken)) {
            dbContext.Projects.Add(ProjectFactory.Create(ProjectNameFactory.Create("Demo Project")));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (await dbContext.WorkOrders.AnyAsync(cancellationToken)) return;

        var project = await dbContext.Projects
            .OrderBy(project => project.Id.Value)
            .FirstAsync(cancellationToken);

        var products = await dbContext.Products
            .OrderBy(product => product.Id.Value)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (products.Count == 0) return;

        var lineSpecs = products
            .Select((product, index) => WorkOrderLineSpecFactory.Create(product.Id, QuantityFactory.Create(index + 1)))
            .ToArray();

        var workOrder = WorkOrderFactory.Create(project, lineSpecs, products);
        dbContext.WorkOrders.Add(workOrder);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
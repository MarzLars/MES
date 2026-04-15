using SteelOrdering.Data;

namespace SteelOrdering.Api.Handlers;

public static class RootHandlers
{
    public static IResult Health() {
        return Results.Ok("Steel Ordering UI and API are running.");
    }

    public static async Task<IResult> SeedDatabase(DataSeed dataSeed, CancellationToken cancellationToken) {
        await dataSeed.SeedOrders(cancellationToken);
        return Results.Ok("Database is seeded for testing.");
    }
}
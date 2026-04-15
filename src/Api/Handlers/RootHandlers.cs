using SteelOrdering.Data;

namespace SteelOrdering.Api.Handlers;

public static class RootHandlers
{
    public static IResult Get() {
        return Results.Ok("MES API is running. Use POST /test/seed to seed test data.");
    }

    public static async Task<IResult> SeedDatabase(DataSeed dataSeed, CancellationToken cancellationToken) {
        await dataSeed.SeedOrders(cancellationToken);
        return Results.Ok("Database is seeded for testing.");
    }
}
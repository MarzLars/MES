using Microsoft.EntityFrameworkCore;
using SteelOrdering.Api.Contracts.Response;
using SteelOrdering.Data;

namespace SteelOrdering.Api.Queries;

/// <summary>
///     Query for retrieving all products.
///     Part of CQRS pattern - read-only operation with optimized query model.
/// </summary>
public sealed record GetProductsQuery;

public static class GetProductsQueryHandler
{
    public static async Task<IResult> Handle(
        GetProductsQuery query,
        ManufacturingDbContext dbContext,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(query);

        var response = await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .Select(product => new ProductResponse(
                product.Id.Value,
                product.Name.Value,
                product.UnitWeightKilograms.Value,
                product.CreatedDateTimeUtc))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(response);
    }
}
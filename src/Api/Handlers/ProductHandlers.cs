using SteelOrdering.Api.Queries;
using SteelOrdering.Data;

namespace SteelOrdering.Api.Handlers;

public static class ProductHandlers
{
    public static Task<IResult> GetAll(ManufacturingDbContext dbContext, CancellationToken cancellationToken) {
        return GetProductsQueryHandler.Handle(new GetProductsQuery(), dbContext, cancellationToken);
    }
}
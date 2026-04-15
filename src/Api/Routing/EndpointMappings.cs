using SteelOrdering.Api.Handlers;

namespace SteelOrdering.Api.Routing;

public static class EndpointMappings
{
    public static WebApplication MapSteelOrderingEndpoints(this WebApplication app) {
        app.MapGet("/health", RootHandlers.Health);
        app.MapPost("/test/seed", RootHandlers.SeedDatabase);

        var products = app.MapGroup("/products");
        products.MapGet("/", ProductHandlers.GetAll);

        var projects = app.MapGroup("/projects");
        projects.MapGet("/", ProjectHandlers.GetAll);
        projects.MapPost("/", ProjectHandlers.Create);

        var workOrders = app.MapGroup("/work-orders");
        workOrders.MapGet("/", WorkOrderHandlers.GetAll);
        workOrders.MapPost("/", WorkOrderHandlers.Create);
        workOrders.MapGet("/{workOrderId:int}", WorkOrderHandlers.GetById);

        return app;
    }
}
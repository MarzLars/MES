using SteelOrdering.Api.Routing;
using SteelOrdering.Data;

var builder = WebApplication.CreateSlimBuilder(args);

builder.WebHost.UseKestrelHttpsConfiguration();

builder.Services.AddDbContext<ManufacturingDbContext>();
builder.Services.AddScoped<DataSeed>();

var app = builder.Build();

await WarmUpDatabaseAsync(app.Services);

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapSteelOrderingEndpoints();

app.Run();

return;

static async Task WarmUpDatabaseAsync(IServiceProvider services) {
    using var scope = services.CreateScope();
    var dataSeed = scope.ServiceProvider.GetRequiredService<DataSeed>();
    await dataSeed.EnsureSchemaAndSeedProductsAsync();
}
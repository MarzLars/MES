using Dapper;
using MES.Commands;
using MES.Data;
using MES.Queries;

// ── Bootstrap ──────────────────────────────────────────────────────────────
var factory = new SqliteConnectionFactory("Data Source=mes.db");
new SchemaInitializer(factory).Initialize();

// Wire up commands and queries (no IoC container needed for this demo)
var createProject  = new CreateProjectCommand(factory);
var createWorkOrder = new CreateWorkOrderCommand(factory);
var getWorkOrder   = new GetWorkOrderQuery(factory);

// ── Seed some products ──────────────────────────────────────────────────────
SeedProducts(factory);

// ── Demo workflow ───────────────────────────────────────────────────────────
Console.WriteLine("=== MES Mini Order System ===");
Console.WriteLine();

// Command: create a project
int projectId = createProject.Execute("Bridge Renovation 2025");
Console.WriteLine($"Created project   Id={projectId}");

// Command: create a work order with two order lines
int workOrderId = createWorkOrder.Execute(projectId,
[
    new(ProductId: 1, Quantity: 10),   // 10 × HEA 200 beam
    new(ProductId: 2, Quantity:  5),   // 5  × S355 plate
]);
Console.WriteLine($"Created work order Id={workOrderId}");
Console.WriteLine();

// Query: retrieve the work order with total weight
var workOrder = getWorkOrder.Execute(workOrderId)!;
Console.WriteLine($"Work order #{workOrder.Id}  (Project: {workOrder.ProjectName})");
Console.WriteLine($"{"Product",-20} {"Qty",5}  {"kg/unit",10}  {"Line kg",10}");
Console.WriteLine(new string('-', 52));
foreach (var line in workOrder.Lines)
    Console.WriteLine($"{line.ProductName,-20} {line.Quantity,5}  {line.WeightKgPerUnit,10:F2}  {line.TotalWeightKg,10:F2}");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"{"Total weight (kg)",-38}  {workOrder.TotalWeightKg,10:F2}");

// ── Helpers ─────────────────────────────────────────────────────────────────
static void SeedProducts(IDbConnectionFactory connectionFactory)
{
    using var db = connectionFactory.Create();

    // Insert only if the table is empty (idempotent seeding)
    if (db.ExecuteScalar<int>("SELECT COUNT(*) FROM Product;") > 0) return;

    db.Execute("""
        INSERT INTO Product (Name, WeightKgPerUnit) VALUES
            ('HEA 200 Beam',     42.3),
            ('S355 Steel Plate', 78.5),
            ('IPE 300 Beam',     42.2);
        """);
}

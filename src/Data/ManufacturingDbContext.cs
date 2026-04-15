using Microsoft.EntityFrameworkCore;
using SteelOrdering.Domain.Entities;
using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Data;

public sealed class ManufacturingDbContext(
    string? databasePath = null) : DbContext
{
    readonly string _databasePath = databasePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mes.db");

    static string InventoryStatusToString(ProductInventoryStatus status) => status switch {
        InStock => "InStock",
        LowStock => "LowStock",
        OutOfStock => "OutOfStock",
        Discontinued => "Discontinued",
        _ => throw new ArgumentOutOfRangeException(nameof(status), "Unknown inventory status.")
    };

    // Default to a consistent path relative to the executable 
    // to avoid issues with different working directories (e.g., VS Code vs Rider).

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderLine> WorkOrderLines => Set<WorkOrderLine>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite($"Data Source={_databasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Product>(productEntityConfiguration => {
            productEntityConfiguration.ToTable("product");
            productEntityConfiguration.HasKey(product => product.Id);

            productEntityConfiguration.Property(product => product.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => ProductIdFactory.Create(value))
                .ValueGeneratedOnAdd();

            productEntityConfiguration.Property(product => product.Name)
                .HasColumnName("name")
                .HasConversion(name => name.Value, value => ProductNameFactory.Create(value))
                .IsRequired()
                .HasMaxLength(200);

            productEntityConfiguration.Property(product => product.UnitWeightKilograms)
                .HasColumnName("unit_weight_kilograms")
                .HasConversion(unitWeight => unitWeight.Value, value => UnitWeightKilogramsFactory.Create(value))
                .HasColumnType("DECIMAL(10,3)");

            productEntityConfiguration.Property(product => product.CreatedDateTimeUtc)
                .HasColumnName("created_date_time_utc")
                .HasDefaultValueSql("datetime('now')");

            productEntityConfiguration.Property(product => product.InventoryStatus)
                .HasColumnName("inventory_status")
                .HasConversion(
                    status => InventoryStatusToString(status),
                    value => ProductInventoryStatusFactory.Create(value))
                .HasMaxLength(32)
                .IsRequired();

            productEntityConfiguration.ToTable(tableBuilder =>
                tableBuilder.HasCheckConstraint("ck_product_unit_weight_kilograms_positive",
                    "unit_weight_kilograms > 0"));
        });

        modelBuilder.Entity<Project>(projectEntityConfiguration => {
            projectEntityConfiguration.ToTable("project");
            projectEntityConfiguration.HasKey(project => project.Id);

            projectEntityConfiguration.Property(project => project.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => ProjectIdFactory.Create(value))
                .ValueGeneratedOnAdd();

            projectEntityConfiguration.Property(project => project.Name)
                .HasColumnName("name")
                .HasConversion(name => name.Value, value => ProjectNameFactory.Create(value))
                .IsRequired()
                .HasMaxLength(200);

            projectEntityConfiguration.Property(project => project.CreatedDateTimeUtc)
                .HasColumnName("created_date_time_utc")
                .HasDefaultValueSql("datetime('now')");

            projectEntityConfiguration.Property(project => project.StartDate)
                .HasColumnName("start_date");

            projectEntityConfiguration.Property(project => project.EndDate)
                .HasColumnName("end_date");
        });

        modelBuilder.Entity<WorkOrder>(workOrderEntityConfiguration => {
            workOrderEntityConfiguration.ToTable("work_order");
            workOrderEntityConfiguration.HasKey(workOrder => workOrder.Id);

            workOrderEntityConfiguration.Property(workOrder => workOrder.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => WorkOrderIdFactory.Create(value))
                .ValueGeneratedOnAdd();

            workOrderEntityConfiguration.Property(workOrder => workOrder.ProjectId)
                .HasColumnName("project_id")
                .HasConversion(id => id.Value, value => ProjectIdFactory.Create(value));

            workOrderEntityConfiguration.HasOne(workOrder => workOrder.Project)
                .WithMany()
                .HasForeignKey(workOrder => workOrder.ProjectId)
                .IsRequired();

            workOrderEntityConfiguration.Property(workOrder => workOrder.CreatedDateTimeUtc)
                .HasColumnName("created_date_time_utc")
                .HasDefaultValueSql("datetime('now')");

            workOrderEntityConfiguration.Navigation(workOrder => workOrder.OrderLines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<WorkOrderLine>(workOrderLineEntityConfiguration => {
            workOrderLineEntityConfiguration.ToTable("work_order_line");
            workOrderLineEntityConfiguration.HasKey(workOrderLine => workOrderLine.Id);

            workOrderLineEntityConfiguration.Property(workOrderLine => workOrderLine.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => WorkOrderLineIdFactory.Create(value))
                .ValueGeneratedOnAdd();

            workOrderLineEntityConfiguration.Property(workOrderLine => workOrderLine.WorkOrderId)
                .HasColumnName("work_order_id")
                .HasConversion(id => id.Value, value => WorkOrderIdFactory.Create(value));

            workOrderLineEntityConfiguration.Property(workOrderLine => workOrderLine.ProductId)
                .HasColumnName("product_id")
                .HasConversion(id => id.Value, value => ProductIdFactory.Create(value));

            workOrderLineEntityConfiguration.Property(workOrderLine => workOrderLine.Quantity)
                .HasColumnName("quantity")
                .HasConversion(quantity => quantity.Value, value => QuantityFactory.Create(value));

            workOrderLineEntityConfiguration.Property(workOrderLine => workOrderLine.UnitWeightKilograms)
                .HasColumnName("unit_weight_kilograms")
                .HasConversion(unitWeight => unitWeight.Value, value => UnitWeightKilogramsFactory.Create(value))
                .HasColumnType("DECIMAL(10,3)");

            workOrderLineEntityConfiguration.HasOne(workOrderLine => workOrderLine.WorkOrder)
                .WithMany(workOrder => workOrder.OrderLines)
                .HasForeignKey(workOrderLine => workOrderLine.WorkOrderId)
                .IsRequired();

            workOrderLineEntityConfiguration.HasOne(workOrderLine => workOrderLine.Product)
                .WithMany()
                .HasForeignKey(workOrderLine => workOrderLine.ProductId)
                .IsRequired();

            workOrderLineEntityConfiguration.Property(workOrderLine => workOrderLine.CreatedDateTimeUtc)
                .HasColumnName("created_date_time_utc")
                .HasDefaultValueSql("datetime('now')");

            workOrderLineEntityConfiguration.ToTable(tableBuilder =>
                tableBuilder.HasCheckConstraint("ck_work_order_line_quantity_positive", "quantity > 0"));

            workOrderLineEntityConfiguration.ToTable(tableBuilder =>
                tableBuilder.HasCheckConstraint("ck_work_order_line_unit_weight_kilograms_positive",
                    "unit_weight_kilograms > 0"));
        });
    }
}
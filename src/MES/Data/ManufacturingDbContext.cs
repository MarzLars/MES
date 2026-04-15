using Microsoft.EntityFrameworkCore;
using MES.Models;

namespace MES.Data;

public class ManufacturingDbContext : DbContext
{
    readonly string _databasePath;

    public ManufacturingDbContext(string? databasePath = null)
    {
        // Default to a consistent path relative to the executable 
        // to avoid issues with different working directories (e.g., VS Code vs Rider).
        _databasePath = databasePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mes.db");
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderLine> WorkOrderLines => Set<WorkOrderLine>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_databasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(productEntityConfiguration =>
        {
            productEntityConfiguration.HasKey(product => product.ProductId);
            productEntityConfiguration.Property(product => product.ProductName).IsRequired().HasMaxLength(200);
            productEntityConfiguration.Property(product => product.WeightInKilogramsPerUnit).HasColumnType("DECIMAL(10,3)");
        });

        modelBuilder.Entity<Project>(projectEntityConfiguration =>
        {
            projectEntityConfiguration.HasKey(project => project.ProjectId);
            projectEntityConfiguration.Property(project => project.ProjectName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<WorkOrder>(workOrderEntityConfiguration =>
        {
            workOrderEntityConfiguration.HasKey(workOrder => workOrder.WorkOrderId);
            workOrderEntityConfiguration.HasOne(workOrder => workOrder.Project)
                  .WithMany()
                  .HasForeignKey(workOrder => workOrder.ProjectId);
            
            var navigation = workOrderEntityConfiguration.Metadata.FindNavigation(nameof(WorkOrder.OrderLines));
            navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<WorkOrderLine>(workOrderLineEntityConfiguration =>
        {
            workOrderLineEntityConfiguration.HasKey(workOrderLine => workOrderLine.WorkOrderLineId);
            workOrderLineEntityConfiguration.HasOne(workOrderLine => workOrderLine.WorkOrder)
                  .WithMany(workOrder => workOrder.OrderLines)
                  .HasForeignKey(workOrderLine => workOrderLine.WorkOrderId);
            workOrderLineEntityConfiguration.HasOne(workOrderLine => workOrderLine.Product)
                  .WithMany()
                  .HasForeignKey(workOrderLine => workOrderLine.ProductId);
        });
    }
}
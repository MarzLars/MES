using Microsoft.EntityFrameworkCore;
using MES.Commands;
using MES.Data;
using MES.Models;
using MES.Queries;

namespace MES;

public sealed class TerminalInterface(ManufacturingDbContext context)
{
    readonly CreateProjectCommand _createProjectCommand = new(context);
    readonly CreateWorkOrderCommand _createWorkOrderCommand = new(context);
    readonly GetWorkOrderQuery _getWorkOrderQuery = new(context);

    public void Run()
    {
        Console.WriteLine("=== Manufacturing Execution System (MES) ===");
        
        bool isTerminalApplicationRunning = true;
        while (isTerminalApplicationRunning)
        {
            Console.WriteLine("\nAvailable Actions:");
            Console.WriteLine("1. List Products");
            Console.WriteLine("2. Create Project");
            Console.WriteLine("3. Create Work Order");
            Console.WriteLine("4. View Work Order");
            Console.WriteLine("5. Exit");
            Console.Write("\nSelect an option: ");

            string? userMenuSelection = Console.ReadLine();

            switch (userMenuSelection)
            {
                case "1":
                    ListAvailableProducts();
                    break;
                case "2":
                    CreateNewProject();
                    break;
                case "3":
                    CreateNewWorkOrder();
                    break;
                case "4":
                    ViewWorkOrderDetails();
                    break;
                case "5":
                    isTerminalApplicationRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid selection. Please try again.");
                    break;
            }
        }
    }

    void ListAvailableProducts()
    {
        var availableProducts = context.Products.ToList();
        Console.WriteLine("\n--- Available Products ---");
        foreach (var product in availableProducts)
        {
            Console.WriteLine($"Identifier: {product.ProductId} | Name: {product.ProductName} | Weight: {product.WeightInKilogramsPerUnit} kg/unit");
        }
    }

    void CreateNewProject()
    {
        Console.Write("Enter project name: ");
        string? projectNameInput = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(projectNameInput)) return;

        try
        {
            int newlyCreatedProjectId = _createProjectCommand.Execute(projectNameInput);
            Console.WriteLine($"Project created successfully with Identifier: {newlyCreatedProjectId}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error occurred: {exception.Message}");
        }
    }

    void CreateNewWorkOrder()
    {
        var existingProjects = context.Projects.ToList();
        if (existingProjects.Count == 0)
        {
            Console.WriteLine("No projects found. Create a project first.");
            return;
        }

        Console.WriteLine("\n--- Select Project ---");
        foreach (var project in existingProjects)
        {
            Console.WriteLine($"Identifier: {project.ProjectId} | Name: {project.ProjectName}");
        }

        Console.Write("Enter Project Identifier: ");
        if (!int.TryParse(Console.ReadLine(), out int selectedProjectIdentifier)) return;

        List<WorkOrderLineRequest> workOrderLineRequests = [];
        bool isAddingWorkOrderLines = true;

        while (isAddingWorkOrderLines)
        {
            ListAvailableProducts();
            Console.Write("Enter Product Identifier (or 'done' to finish): ");
            string? productIdentifierInput = Console.ReadLine();
            if (productIdentifierInput?.Equals("done", StringComparison.OrdinalIgnoreCase) == true) break;

            if (!int.TryParse(productIdentifierInput, out int selectedProductIdentifier)) continue;
            Console.Write("Enter Quantity: ");
            if (int.TryParse(Console.ReadLine(), out int requestedQuantity))
            {
                workOrderLineRequests.Add(new WorkOrderLineRequest(selectedProductIdentifier, requestedQuantity));
            }
        }

        if (workOrderLineRequests.Count == 0) return;

        try
        {
            int newlyCreatedWorkOrderIdentifier = _createWorkOrderCommand.Execute(selectedProjectIdentifier, workOrderLineRequests);
            Console.WriteLine($"Work Order created successfully with Identifier: {newlyCreatedWorkOrderIdentifier}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error occurred: {exception.Message}");
        }
    }

    void ViewWorkOrderDetails()
    {
        Console.Write("Enter Work Order Identifier: ");
        if (!int.TryParse(Console.ReadLine(), out int requestedWorkOrderId)) return;

        var retrievedWorkOrder = _getWorkOrderQuery.Execute(requestedWorkOrderId);
        if (retrievedWorkOrder is null)
        {
            Console.WriteLine("Work Order not found.");
            return;
        }

        Console.WriteLine($"\nWork Order #{retrievedWorkOrder.WorkOrderId} (Project: {retrievedWorkOrder.Project?.ProjectName})");
        Console.WriteLine($"{"Product",-25} {"Quantity",10} {"kg/unit",12} {"Line weight",12}");
        Console.WriteLine(new string('-', 65));

        foreach (var workOrderLine in retrievedWorkOrder.OrderLines)
        {
            Console.WriteLine($"{workOrderLine.Product?.ProductName,-25} {workOrderLine.Quantity,10} {workOrderLine.Product?.WeightInKilogramsPerUnit,12:F2} {workOrderLine.TotalLineWeightInKilograms,12:F2}");
        }

        Console.WriteLine(new string('-', 65));
        Console.WriteLine($"{"Total Weight (kg)",-49} {retrievedWorkOrder.TotalWeightInKilograms,12:F2}");
    }
}
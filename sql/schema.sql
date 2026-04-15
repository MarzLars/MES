-- =============================================================================
-- MES Mini Order System – Database Schema
-- Dialect: SQL Server (T-SQL)
-- Compatible alternative: SQLite (used by the demo console app)
-- =============================================================================

-- Products catalogue: steel profiles and plates with their weight per unit
CREATE TABLE Product (
    Id              INT           NOT NULL IDENTITY PRIMARY KEY,
    Name            NVARCHAR(200) NOT NULL,
    WeightKgPerUnit DECIMAL(10,3) NOT NULL CHECK (WeightKgPerUnit > 0)
);

-- A Project groups one or more work orders
CREATE TABLE Project (
    Id   INT           NOT NULL IDENTITY PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL
);

-- A WorkOrder belongs to exactly one Project
CREATE TABLE WorkOrder (
    Id        INT NOT NULL IDENTITY PRIMARY KEY,
    ProjectId INT NOT NULL REFERENCES Project(Id)
);

-- Each OrderLine links a WorkOrder to a Product with a quantity
CREATE TABLE OrderLine (
    Id          INT NOT NULL IDENTITY PRIMARY KEY,
    WorkOrderId INT NOT NULL REFERENCES WorkOrder(Id),
    ProductId   INT NOT NULL REFERENCES Product(Id),
    Quantity    INT NOT NULL CHECK (Quantity > 0)
);

-- =============================================================================
-- Sample query: work order with total weight, sorted by order-line insertion order
-- =============================================================================
SELECT
    wo.Id           AS WorkOrderId,
    p.Name          AS Project,
    pr.Name         AS Product,
    pr.WeightKgPerUnit,
    ol.Quantity,
    ol.Quantity * pr.WeightKgPerUnit AS LineWeightKg,
    SUM(ol.Quantity * pr.WeightKgPerUnit) OVER (PARTITION BY wo.Id) AS TotalWeightKg
FROM  WorkOrder wo
JOIN  Project   p  ON p.Id  = wo.ProjectId
JOIN  OrderLine ol ON ol.WorkOrderId = wo.Id
JOIN  Product   pr ON pr.Id = ol.ProductId
ORDER BY ol.Id;

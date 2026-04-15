-- =============================================================================
-- MES Mini Order System – Database Schema
-- Dialect: SQL Server (T-SQL)
-- Compatible alternative: SQLite (used by the demo console app)
-- =============================================================================

-- Products catalogue: steel profiles and plates with their weight per unit
CREATE TABLE Product (
    ProductId                INT            NOT NULL IDENTITY PRIMARY KEY,
    ProductName              NVARCHAR(200)  NOT NULL,
    WeightInKilogramsPerUnit DECIMAL(10,3)  NOT NULL CHECK (WeightInKilogramsPerUnit > 0),
    CreatedDateTimeUtc       DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME()
);

-- A Project groups one or more work orders
CREATE TABLE Project (
    ProjectId           INT            NOT NULL IDENTITY PRIMARY KEY,
    ProjectName         NVARCHAR(200)  NOT NULL,
    CreatedDateTimeUtc  DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME()
);

-- A WorkOrder belongs to exactly one Project
CREATE TABLE WorkOrder (
    WorkOrderId        INT            NOT NULL IDENTITY PRIMARY KEY,
    ProjectId          INT            NOT NULL,
    CreatedDateTimeUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    
    CONSTRAINT FK_WorkOrder_Project FOREIGN KEY (ProjectId) 
        REFERENCES Project(ProjectId) ON DELETE NO ACTION
);

-- Each WorkOrderLine links a WorkOrder to a Product with a quantity
CREATE TABLE WorkOrderLine (
    WorkOrderLineId    INT            NOT NULL IDENTITY PRIMARY KEY,
    WorkOrderId        INT            NOT NULL,
    ProductId          INT            NOT NULL,
    Quantity           INT            NOT NULL CHECK (Quantity > 0),
    CreatedDateTimeUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    
    CONSTRAINT FK_WorkOrderLine_WorkOrder FOREIGN KEY (WorkOrderId) 
        REFERENCES WorkOrder(WorkOrderId) ON DELETE NO ACTION,
    CONSTRAINT FK_WorkOrderLine_Product FOREIGN KEY (ProductId) 
        REFERENCES Product(ProductId) ON DELETE NO ACTION
);

-- =============================================================================
-- Explicit Indexes for Foreign Keys (Performance Optimization)
-- =============================================================================
CREATE INDEX IX_WorkOrder_ProjectId ON WorkOrder(ProjectId);
CREATE INDEX IX_WorkOrderLine_WorkOrderId ON WorkOrderLine(WorkOrderId);
CREATE INDEX IX_WorkOrderLine_ProductId ON WorkOrderLine(ProductId);

-- =============================================================================
-- Sample query: work order with total weight, sorted by order-line insertion order
-- =============================================================================
SELECT
    wo.WorkOrderId                   AS WorkOrderId,
    p.ProjectName                    AS ProjectName,
    pr.ProductName                   AS ProductName,
    pr.WeightInKilogramsPerUnit,
    wol.Quantity,
    wol.Quantity * pr.WeightInKilogramsPerUnit AS LineWeightInKilograms,
    SUM(wol.Quantity * pr.WeightInKilogramsPerUnit) OVER (PARTITION BY wo.WorkOrderId) AS TotalWeightInKilograms,
    wo.CreatedDateTimeUtc            AS WorkOrderCreatedUtc
FROM  WorkOrder wo
JOIN  Project   p   ON p.ProjectId = wo.ProjectId
JOIN  WorkOrderLine wol ON wol.WorkOrderId = wo.WorkOrderId
JOIN  Product   pr  ON pr.ProductId = wol.ProductId
ORDER BY wol.WorkOrderLineId;

-- SQL dialect: SQLite
-- MES database schema snapshot for the current SQLite-backed EF Core model.

CREATE TABLE product (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    unit_weight_kilograms REAL NOT NULL CHECK (unit_weight_kilograms > 0),
    created_date_time_utc TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE project (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    created_date_time_utc TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE work_order (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    project_id INTEGER NOT NULL,
    created_date_time_utc TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT fk_work_order_project FOREIGN KEY (project_id) REFERENCES project (id)
);

CREATE TABLE work_order_line (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    work_order_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    unit_weight_kilograms REAL NOT NULL CHECK (unit_weight_kilograms > 0),
    created_date_time_utc TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT fk_work_order_line_work_order FOREIGN KEY (work_order_id) REFERENCES work_order (id),
    CONSTRAINT fk_work_order_line_product FOREIGN KEY (product_id) REFERENCES product (id)
);

CREATE INDEX ix_work_order_project_id ON work_order (project_id);
CREATE INDEX ix_work_order_line_work_order_id ON work_order_line (work_order_id);
CREATE INDEX ix_work_order_line_product_id ON work_order_line (product_id);

-- Example query shape matching the API: work order with lines and totals.
SELECT
    work_order.id AS work_order_id,
    project.name AS project_name,
    product.name AS product_name,
    product.unit_weight_kilograms AS product_unit_weight_kg,
    work_order_line.quantity AS line_quantity,
    work_order_line.quantity * product.unit_weight_kilograms AS line_weight_kg,
    SUM(work_order_line.quantity * product.unit_weight_kilograms) OVER (PARTITION BY work_order.id) AS total_weight_kg,
    work_order.created_date_time_utc AS work_order_created_utc
FROM work_order
    JOIN project ON project.id = work_order.project_id
    JOIN work_order_line ON work_order_line.work_order_id = work_order.id
    JOIN product ON product.id = work_order_line.product_id
ORDER BY work_order_line.id;
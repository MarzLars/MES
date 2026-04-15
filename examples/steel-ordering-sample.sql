-- Steel Ordering sample SQLite export
-- Generated from the seeded local database used by the ASP.NET app.

PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;

CREATE TABLE "product" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_product" PRIMARY KEY,
    "name" TEXT NOT NULL,
    "unit_weight_kilograms" DECIMAL(10,3) NOT NULL,
    "inventory_status" TEXT NOT NULL,
    "created_date_time_utc" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "ck_product_unit_weight_kilograms_positive" CHECK (unit_weight_kilograms > 0)
);

CREATE TABLE "project" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_project" PRIMARY KEY,
    "name" TEXT NOT NULL,
    "start_date" TEXT NULL,
    "end_date" TEXT NULL,
    "created_date_time_utc" TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE "work_order" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_work_order" PRIMARY KEY,
    "project_id" INTEGER NOT NULL,
    "created_date_time_utc" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_work_order_project_project_id" FOREIGN KEY ("project_id") REFERENCES "project" ("id") ON DELETE CASCADE
);

CREATE TABLE "work_order_line" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_work_order_line" PRIMARY KEY,
    "work_order_id" INTEGER NOT NULL,
    "product_id" INTEGER NOT NULL,
    "quantity" INTEGER NOT NULL,
    "unit_weight_kilograms" DECIMAL(10,3) NOT NULL,
    "created_date_time_utc" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "ck_work_order_line_quantity_positive" CHECK (quantity > 0),
    CONSTRAINT "ck_work_order_line_unit_weight_kilograms_positive" CHECK (unit_weight_kilograms > 0),
    CONSTRAINT "FK_work_order_line_product_product_id" FOREIGN KEY ("product_id") REFERENCES "product" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_work_order_line_work_order_work_order_id" FOREIGN KEY ("work_order_id") REFERENCES "work_order" ("id") ON DELETE CASCADE
);

INSERT INTO "product" VALUES(1,'HEA 200 Beam',42.3,'InStock','2026-04-15 22:07:34.9400391+00:00');
INSERT INTO "product" VALUES(2,'S355 Steel Plate',78.5,'InStock','2026-04-15 22:07:34.9402865+00:00');
INSERT INTO "product" VALUES(3,'IPE 300 Beam',42.2,'OutOfStock','2026-04-15 22:07:34.9403268+00:00');

INSERT INTO "project" VALUES(1,'Demo Project',NULL,NULL,'2026-04-15 22:07:39.061744+00:00');

INSERT INTO "work_order" VALUES(1,1,'2026-04-15 22:08:21.9855118+00:00');

INSERT INTO "work_order_line" VALUES(1,1,1,1,42.3,'2026-04-15 22:08:21.9852518+00:00');
INSERT INTO "work_order_line" VALUES(2,1,2,2,78.5,'2026-04-15 22:08:21.9854857+00:00');

COMMIT;

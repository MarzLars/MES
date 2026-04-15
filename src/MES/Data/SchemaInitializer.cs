using Dapper;
using MES.Data;

namespace MES.Data;

/// <summary>
/// Ensures the SQLite database schema exists (idempotent).
/// Runs at startup so the app works without a pre-existing database file.
/// </summary>
public sealed class SchemaInitializer(IDbConnectionFactory connectionFactory)
{
    private const string Ddl = """
        CREATE TABLE IF NOT EXISTS Product (
            Id              INTEGER PRIMARY KEY AUTOINCREMENT,
            Name            TEXT    NOT NULL,
            WeightKgPerUnit REAL    NOT NULL CHECK (WeightKgPerUnit > 0)
        );

        CREATE TABLE IF NOT EXISTS Project (
            Id   INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT    NOT NULL
        );

        CREATE TABLE IF NOT EXISTS WorkOrder (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            ProjectId INTEGER NOT NULL REFERENCES Project(Id)
        );

        CREATE TABLE IF NOT EXISTS OrderLine (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            WorkOrderId INTEGER NOT NULL REFERENCES WorkOrder(Id),
            ProductId   INTEGER NOT NULL REFERENCES Product(Id),
            Quantity    INTEGER NOT NULL CHECK (Quantity > 0)
        );
        """;

    public void Initialize()
    {
        using var db = connectionFactory.Create();
        db.Execute(Ddl);
    }
}

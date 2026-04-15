using System.Data;
using Microsoft.Data.Sqlite;

namespace MES.Data;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public sealed class SqliteConnectionFactory(
    string connectionString) : IDbConnectionFactory
{
    public IDbConnection Create() {
        return new SqliteConnection(connectionString);
    }
}
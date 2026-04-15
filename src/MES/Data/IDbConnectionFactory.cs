using Microsoft.Data.Sqlite;
using System.Data;

namespace MES.Data;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public sealed class SqliteConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection Create() => new SqliteConnection(connectionString);
}

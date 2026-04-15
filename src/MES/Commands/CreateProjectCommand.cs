using Dapper;
using MES.Data;

namespace MES.Commands;

/// <summary>
/// Creates a new Project and returns its generated Id.
/// Returning the Id here is consistent with the CQS corollary described in the README:
/// it is the minimal status needed by the caller for bookkeeping, not a full query result.
/// </summary>
public sealed class CreateProjectCommand(IDbConnectionFactory connectionFactory)
{
    public int Execute(string name)
    {
        using var db = connectionFactory.Create();
        return db.ExecuteScalar<int>(
            "INSERT INTO Project (Name) VALUES (@Name); SELECT last_insert_rowid();",
            new { Name = name });
    }
}

using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record Project
{
    Project() { } // For EF Core

    Project(
        ProjectId id,
        ProjectName name,
        DateTimeOffset createdDateTimeUtc) {
        Id = id;
        Name = name;
        CreatedDateTimeUtc = createdDateTimeUtc;
    }

    public ProjectId Id { get; private set; }
    public ProjectName Name { get; private set; }
    public DateTimeOffset CreatedDateTimeUtc { get; private set; }

    public static Project Create(ProjectName name) {
        return new Project(new ProjectId(0), name, DateTimeOffset.UtcNow);
    }

    internal static Project FromDatabase(
        int id,
        ProjectName name,
        DateTimeOffset createdDateTimeUtc) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return new Project(new ProjectId(id), name, createdDateTimeUtc);
    }
}
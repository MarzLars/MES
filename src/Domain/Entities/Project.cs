using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record Project
{
    Project() { } // For EF Core

    Project(
        ProjectId id,
        ProjectName name,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        DateTimeOffset createdDateTimeUtc) {
        Id = id;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        CreatedDateTimeUtc = createdDateTimeUtc;
    }

    public ProjectId Id { get; private set; }
    public ProjectName Name { get; private set; }
    public DateTimeOffset? StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public DateTimeOffset CreatedDateTimeUtc { get; private set; }

    public static Project Create(ProjectName name, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null) {
        return new Project(new ProjectId(0), name, startDate, endDate, DateTimeOffset.UtcNow);
    }

    internal static Project FromDatabase(
        int id,
        ProjectName name,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        DateTimeOffset createdDateTimeUtc) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return new Project(new ProjectId(id), name, startDate, endDate, createdDateTimeUtc);
    }
}
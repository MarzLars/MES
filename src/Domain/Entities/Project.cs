using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record Project
{
    Project() { } // For EF Core

    internal Project(
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
}

public static class ProjectFactory
{
    public static Project Create(ProjectName name, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name.Value);

        if (startDate is not null && endDate is not null && endDate.Value < startDate.Value)
            throw new ArgumentException("End date cannot be earlier than start date.", nameof(endDate));

        return new Project(default, name, startDate, endDate, DateTimeOffset.UtcNow);
    }
}
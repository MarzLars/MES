namespace MES.Models;

public record Project
{
    public Project(string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            throw new ArgumentException("Project name cannot be empty.", nameof(projectName));
        }

        ProjectName = projectName;
    }

    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public DateTimeOffset CreatedDateTimeUtc { get; init; } = DateTimeOffset.UtcNow;
}
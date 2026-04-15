using MES.Data;
using MES.Models;

namespace MES.Commands;

public sealed class CreateProjectCommand(ManufacturingDbContext manufacturingDbContext)
{
    public int Execute(string projectName)
    {
        var newlyCreatedProject = new Project(projectName);
        manufacturingDbContext.Projects.Add(newlyCreatedProject);
        manufacturingDbContext.SaveChanges();
        return newlyCreatedProject.ProjectId;
    }
}
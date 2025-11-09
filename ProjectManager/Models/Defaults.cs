namespace ProjectManager.Models;

public class Defaults
{
    public static Project GetStartProject()
    {
        var dfaultProjectInfo = new ProjectInfo
        {
            Name = "New Project",
            StartDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd"),
            EndDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"),
            Description = ""
        };

        var dfaultProject = new Project
        {
            Tasks = [],
            ProjectInfo = dfaultProjectInfo,
            ClientTimestamp = DateTimeOffset.UtcNow
        };

        return dfaultProject;
    }
}

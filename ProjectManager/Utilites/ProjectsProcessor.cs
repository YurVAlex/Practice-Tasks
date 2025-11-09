using ProjectManager.Models;
using ProjectManager.Utilities;

namespace ProjectManager.Utilites;

public class ProjectsProcessor
{
    public Projects Projects { get; set; } = new Projects{ UserProjects = [] };

    public ProjectsProcessor(User user) 
    {

        if (user.Projects == null || user.Projects == "{}" || string.IsNullOrWhiteSpace(user.Projects))
        {
            Projects.UserProjects.Add(Defaults.GetStartProject());
        }
        else
        {
            try
            {
                Projects = ProjectsSerializer.DeserializeProjects(user.Projects);
                if (Projects == null)
                {
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    Console.WriteLine("User's projects deserialization faled!");
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine(ex.Message);
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
        }
    }

    public Project? GetLatestProjectOrDefault()
    {
        // 1. Check if the list is null or empty before attempting to query it.
        if (Projects.UserProjects == null || Projects.UserProjects.Count == 0)
        {
            return Defaults.GetStartProject(); // redundant?
        }

        // 2. Use LINQ to find the project with the maximum ClientTimestamp.
        // The Where clause ensures we only consider projects where ClientTimestamp is not null.
        // The OrderByDescending and FirstOrDefault combination is an efficient way to get the item
        // with the maximum value for a specific property.
        var latestProject = Projects.UserProjects
            .Where(p => p.ClientTimestamp.HasValue)
            .OrderByDescending(p => p.ClientTimestamp!.Value) // Use ! to assert not null after Where
            .FirstOrDefault();

        return latestProject;
    }

    public Project? GetProjectByName(string projectName)
    {
        if (Projects.UserProjects == null || Projects.UserProjects.Count == 0 || string.IsNullOrWhiteSpace(projectName))
        {
            return null;
        }

        // Use LINQ to find the first project whose ProjectInfo Name matches the provided name.
        // We use StringComparison.OrdinalIgnoreCase to ensure the match is case-insensitive,
        // making the search more user-friendly.
        var matchingProject = Projects.UserProjects
            .FirstOrDefault(p =>
                p.ProjectInfo != null &&
                !string.IsNullOrWhiteSpace(p.ProjectInfo.Name) &&
                p.ProjectInfo.Name.Equals(projectName, System.StringComparison.OrdinalIgnoreCase));

        return matchingProject;
    }

    public bool ReplaceProject(Project newProject)
    {
        // 1. Basic validation: check if the list is initialized and the new project is valid (has a name).
        if (Projects.UserProjects == null || newProject?.ProjectInfo == null || string.IsNullOrWhiteSpace(newProject.ProjectInfo.Name))
        {
            return false;
        }

        string projectNameToFind = newProject.ProjectInfo.Name;

        // 2. Find the index of the project to be replaced using the new project's name.
        var index = Projects.UserProjects.FindIndex(p =>
            p.ProjectInfo != null &&
            !string.IsNullOrWhiteSpace(p.ProjectInfo.Name) &&
            p.ProjectInfo.Name.Equals(projectNameToFind, StringComparison.OrdinalIgnoreCase));

        if (index != -1)
        {
            // 3. Replace the old project object with the new one at the found index.
            Projects.UserProjects[index] = newProject;
            return true;
        }

        // Project with the matching name was not found.
        return false;
    }
}

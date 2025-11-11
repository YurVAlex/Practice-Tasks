using ProjectManager.Models;
using ProjectManager.Utilities;

namespace ProjectManager.Utilities;

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

    public Project? GetProjectById(Guid projectId)
    {
        if (Projects.UserProjects == null || Projects.UserProjects.Count == 0 || projectId == Guid.Empty)
        {
            return null;
        }

        // Use LINQ to find the first project whose Id matches the provided ID.
        var matchingProject = Projects.UserProjects
            .FirstOrDefault(p => p.Id == projectId);

        return matchingProject;
    }

    public bool ReplaceProject(Project newProject)
    {
        // 1. Basic validation: check if the list is initialized and the new project is valid.
        if (Projects.UserProjects == null || newProject == null)
        {
            Console.WriteLine("[ReplaceProject] Failed: Projects list or newProject is null");
            return false;
        }

        // 2. Try to find by Id first (preferred method)
        var index = Projects.UserProjects.FindIndex(p => p.Id == newProject.Id);

        if (index != -1)
        {
            Console.WriteLine($"[ReplaceProject] Found project by Id at index {index}");
            Projects.UserProjects[index] = newProject;
            return true;
        }

        Console.WriteLine($"[ReplaceProject] Project not found by Id: {newProject.Id}");
        Console.WriteLine($"[ReplaceProject] Available project Ids: {string.Join(", ", Projects.UserProjects.Select(p => p.Id))}");

        // 3. Special case: if there's only one project in the list, assume it's the one to replace
        // This handles the common single-project scenario and Id mismatches from client/server initialization
        if (Projects.UserProjects.Count == 1)
        {
            Console.WriteLine($"[ReplaceProject] Single project in list - replacing it (Id mismatch resolved)");
            // Update the Id to match what the client is now using
            Projects.UserProjects[0] = newProject;
            return true;
        }

        // 4. If not found by Id and project has a name, try to find by name (fallback for backward compatibility)
        if (newProject.ProjectInfo != null && !string.IsNullOrWhiteSpace(newProject.ProjectInfo.Name))
        {
            string projectNameToFind = newProject.ProjectInfo.Name;
            index = Projects.UserProjects.FindIndex(p =>
                p.ProjectInfo != null &&
                !string.IsNullOrWhiteSpace(p.ProjectInfo.Name) &&
                p.ProjectInfo.Name.Equals(projectNameToFind, StringComparison.OrdinalIgnoreCase));
            
            if (index != -1)
            {
                Console.WriteLine($"[ReplaceProject] Found project by name '{projectNameToFind}' at index {index}");
                Projects.UserProjects[index] = newProject;
                return true;
            }
        }

        // Project was not found by Id, Name, or single-project fallback.
        Console.WriteLine($"[ReplaceProject] Failed to find project by any method. Projects count: {Projects.UserProjects.Count}");
        return false;
    }

    public bool AddProject(Project newProject)
    {
        if (Projects.UserProjects == null || newProject == null)
        {
            return false;
        }

        // Check if project with this Id already exists
        if (Projects.UserProjects.Any(p => p.Id == newProject.Id))
        {
            return false; // Project already exists
        }

        Projects.UserProjects.Add(newProject);
        return true;
    }

    public bool RemoveProject(Guid projectId)
    {
        if (Projects.UserProjects == null || projectId == Guid.Empty)
        {
            return false;
        }

        var project = Projects.UserProjects.FirstOrDefault(p => p.Id == projectId);
        if (project != null)
        {
            Projects.UserProjects.Remove(project);
            return true;
        }

        return false;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
// Reference the namespace where your existing DTOs are defined
using ASP_Empty_WebApplication_01.Models;

/// <summary>
/// Static utility class for formatting Project payloads into detailed log strings.
/// </summary>
public static class ProjectLogger
{
    /// <summary>
    /// Generates a detailed, multi-line string summary of a Project object's contents
    /// for console or log output.
    /// </summary>
    /// <param name="payload">The Project object to summarize.</param>
    /// <returns>A formatted string containing all project and task details.</returns>
    public static string GenerateLogString(Project payload)
    {
        var sb = new StringBuilder();

        if (payload == null)
        {
            return "Received a null Project payload.";
        }

        sb.AppendLine("=== /projectUpdate Received ===");
        // Use "o" for ISO 8601 round-trip format, using DateTimeOffset for accurate client timestamp
        sb.AppendLine($"ClientTimestamp: {payload.ClientTimestamp?.ToString("o") ?? "(none)"}");
        sb.AppendLine();

        // 1. Project Info
        var projectInfo = payload.ProjectInfo;
        if (projectInfo != null)
        {
            sb.AppendLine("-- Project Info --");
            sb.AppendLine($"Name         : {projectInfo.Name ?? "(null)"}");
            // Use the DTO helper methods to display dates cleanly, or the raw string if preferred
            sb.AppendLine($"StartDate    : {projectInfo.StartDateAsDateTime()?.ToShortDateString() ?? projectInfo.StartDate ?? "(null)"}");
            sb.AppendLine($"EndDate      : {projectInfo.EndDateAsDateTime()?.ToShortDateString() ?? projectInfo.EndDate ?? "(null)"}");
            sb.AppendLine($"Description  : {projectInfo.Description ?? "(null)"}");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("No project info provided.");
            sb.AppendLine();
        }

        // 2. Tasks
        var tasks = payload.Tasks;
        var taskCount = tasks?.Count ?? 0;
        sb.AppendLine($"Tasks count: {taskCount}");
        sb.AppendLine();

        if (taskCount > 0 && tasks != null)
        {
            sb.AppendLine("-- Tasks --");
            foreach (var t in tasks)
            {
                // Note: TaskItem.Id is a long in your provided DTO
                sb.AppendLine($"Id: {t.Id}");
                sb.AppendLine($"  Name        : {t.Name ?? "(null)"}");
                sb.AppendLine($"  StartDate   : {t.StartDateAsDateTime()?.ToShortDateString() ?? t.StartDate ?? "(null)"}");
                sb.AppendLine($"  EndDate     : {t.EndDateAsDateTime()?.ToShortDateString() ?? t.EndDate ?? "(null)"}");
                sb.AppendLine($"  Completed   : {t.Completed}");
                // Progress is an int 0-100 in your DTO; display as percentage
                sb.AppendLine($"  Progress    : {(t.Progress.HasValue ? t.Progress.Value.ToString() + "%" : "null")}");
                sb.AppendLine($"  Description : {t.Description ?? "(null)"}");
            }
            sb.AppendLine();
        }

        // 3. Last Updated Task
        var lu = payload.LastUpdatedTask;
        if (lu != null)
        {
            sb.AppendLine("-- LastUpdatedTask --");
            // Combine all fields into one line for brevity
            sb.AppendLine($"Id: {lu.Id}, Name: {lu.Name ?? "(null)"}, Completed: {lu.Completed}, Progress: {(lu.Progress.HasValue ? lu.Progress.Value.ToString() + "%" : "null")}");
            sb.AppendLine();
        }

        sb.AppendLine("=== End payload ===");

        return sb.ToString();
    }
}

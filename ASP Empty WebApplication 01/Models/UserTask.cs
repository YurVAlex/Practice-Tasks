using System.Text.Json.Serialization;

namespace ASP_Empty_WebApplication_01.Models;


/// <summary>
/// Represents a phase or task within a project.
/// Includes JSON property name attributes for correct deserialization from camelCase (JavaScript/JSON) to PascalCase (C#).
/// </summary>
public class UserTask
{
    // The unique identifier for the phase
    [JsonPropertyName("id")]
    public long Id { get; set; }

    // The human-readable name of the phase
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    // The planned or actual start date of the phase
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    // The planned or actual end date of the phase
    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    // A detailed description of the work involved
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    // A flag indicating whether the phase has been completed
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    // Optional: A method to display phase details
    public override string ToString()
    {
        string status = Completed ? "Completed" : "In Progress";
        return $"Phase: {Name} (ID: {Id})\n\tDates: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}\n\tStatus: {status}";
    }
}
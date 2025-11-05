using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProjectManager.Models;

/// <summary>
/// DTO representing a collection of Project objects for serialization/deserialization.
/// </summary>
public class Projects
{
    [JsonPropertyName("userProjects")]
    public List<Project> UserProjects { get; set; } = [];
}
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASP_Empty_WebApplication_01.Models;

/// <summary>
/// Top-level DTO that matches the JSON payload sent to /projectUpdate
/// {
///   "tasks": [ ... ],
///   "project": { ... },
///   "lastUpdatedTask": { ... } | null,
///   "clientTimestamp": "2025-10-30T15:16:48.000Z"
/// }
/// </summary>
public class Project
{
    [JsonPropertyName("tasks")]
    public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    // JSON key "project" maps here
    [JsonPropertyName("project")]
    public ProjectInfo ProjectInfo { get; set; } = new ProjectInfo();

    // Optional - the single task the client recently changed (may be null)
    [JsonPropertyName("lastUpdatedTask")]
    public TaskItem? LastUpdatedTask { get; set; }

    // ISO 8601 timestamp from client; nullable if client didn't send it
    [JsonPropertyName("clientTimestamp")]
    public DateTimeOffset? ClientTimestamp { get; set; }

    /// <summary>
    /// Normalize all contained tasks (useful to call before processing/saving)
    /// </summary>
    public void NormalizeTasks()
    {
        if (Tasks == null) return;
        foreach (var t in Tasks)
        {
            t.Normalize();
        }
    }
}

using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ASP_Empty_WebApplication_01.Models;

public class ProjectInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // Keep as string to match client format "YYYY-MM-DD".
    [JsonPropertyName("startDate")]
    public string? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public string? EndDate { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional helpers to parse dates if you want to work with DateTime on server side.
    /// </summary>
    public DateTime? StartDateAsDateTime()
    {
        if (string.IsNullOrWhiteSpace(StartDate)) return null;
        if (DateTime.TryParse(StartDate, out var dt)) return DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);
        return null;
    }

    public DateTime? EndDateAsDateTime()
    {
        if (string.IsNullOrWhiteSpace(EndDate)) return null;
        if (DateTime.TryParse(EndDate, out var dt)) return DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);
        return null;
    }
}


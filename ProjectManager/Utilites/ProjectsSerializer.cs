using ProjectManager.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace ProjectManager.Utilities;

/// <summary>
/// Provides utility methods for serializing and deserializing Project data using System.Text.Json.
/// </summary>
public static class ProjectsSerializer
{
    // Private options definition to keep configuration consistent
    private static readonly JsonSerializerOptions _optionsDeserialize = new JsonSerializerOptions
    {
        // --- NEW DESERIALIZATION SETTINGS ---
        // Allows case-insensitive matching between JSON properties and C# properties
        PropertyNameCaseInsensitive = true,
        // Allows trailing commas in JSON arrays and objects during deserialization
        AllowTrailingCommas = true
    };

    private static readonly JsonSerializerOptions _optionsSerialize = new JsonSerializerOptions
    {
        // --- NEW SERIALIZATION SETTINGS ---
        PropertyNamingPolicy = null,
        WriteIndented = false
    };


    // --- SERIALIZATION METHODS ---

    /// <summary>
    /// Serializes a single Project object instance into a JSON string.
    /// </summary>
    /// <param name="projectToSerialize">The Project object instance to be converted to JSON.</param>
    /// <returns>A JSON string representation of the Project object, or "{}" on error/null input.</returns>
    public static string SerializeProject(Project projectToSerialize)
    {
        if (projectToSerialize == null)
        {
            Console.WriteLine("[Serialization] Error: Input Project object is null.");
            return "{}";
        }

        try
        {
            return JsonSerializer.Serialize(projectToSerialize, _optionsSerialize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Serialization] Error during single Project serialization: {ex.Message}");
            return "{}";
        }
    }

    /// <summary>
    /// Serializes a Projects container object (including its List<Project> property) into a JSON string.
    /// </summary>
    /// <param name="projectsToSerialize">The Projects container object instance to be converted to JSON.</param>
    /// <returns>A JSON string representation of the Projects object, or "{}" on error/null input.</returns>
    public static string SerializeProjects(Projects projectsToSerialize)
    {
        if (projectsToSerialize == null)
        {
            Console.WriteLine("[Serialization] Error: Input Projects container object is null.");
            return "{}";
        }

        try
        {
            return JsonSerializer.Serialize(projectsToSerialize, _optionsSerialize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Serialization] Error during Projects collection serialization: {ex.Message}");
            return "{}";
        }
    }

    // --- DESERIALIZATION METHODS ---

    /// <summary>
    /// Deserializes a JSON string into a single Project object.
    /// </summary>
    /// <param name="jsonString">The JSON string to deserialize.</param>
    /// <returns>The deserialized Project object, or null on error/invalid input.</returns>
    public static Project? DeserializeProject(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            Console.WriteLine("[Deserialization] Error: Input JSON string is null or empty.");
            return null;
        }

        try
        {
            // Deserialize<T> returns null if the JSON is "null" literal, but we handle empty strings above.
            return JsonSerializer.Deserialize<Project>(jsonString, _optionsDeserialize);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[Deserialization] Error parsing JSON for single Project: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Deserialization] An unexpected error occurred for single Project: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Deserializes a JSON string into a Projects container object.
    /// </summary>
    /// <param name="jsonString">The JSON string to deserialize.</param>
    /// <returns>The deserialized Projects object, or null on error/invalid input.</returns>
    public static Projects? DeserializeProjects(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            Console.WriteLine("[Deserialization] Error: Input JSON string is null or empty.");
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Projects>(jsonString, _optionsDeserialize);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[Deserialization] Error parsing JSON for Projects collection: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Deserialization] An unexpected error occurred for Projects collection: {ex.Message}");
            return null;
        }
    }
}
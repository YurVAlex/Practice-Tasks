using System.Text.Json;

namespace Example_2;

internal static class Loader
{
    public static async Task<List<Item>?> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            CombineLoger.Log($"The storage in {filePath} is not exist or missed. Process aborted.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var bufer = await File.ReadAllLinesAsync(filePath);

            var result = new List<Item>();

            foreach (var line in bufer)
            {
                try
                {
                    var temp = JsonSerializer.Deserialize<Item>(line);
                    if (temp != null)
                    {
                        result.Add(temp);
                    }
                }
                catch { } // to skip non-serialable lines
            }

            return result;
        }
        catch (Exception ex)
        {
            CombineLoger.Log(ex.Message);
        }

        return null;
    }
}

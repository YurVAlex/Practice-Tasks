
using System.Text;
using System.Text.Json;

namespace Example_2;

internal class StorageJSON 
{
    private string _storagePath;

    public StorageJSON(string filePath = "StorageJSON.json")
    {
        try
        {
            File.WriteAllText(filePath, ""); //Create file with test
            Storage.Storages.Add(filePath);
            _storagePath = filePath;
            CombineLoger.Log($"New storage in {filePath} created.");
        }
        catch (Exception ex)
        {
            CombineLoger.Log(ex.Message);
        }
    }

    public string StoragePath { get { return _storagePath; } }

    public async Task SaveCache(List<Item> items)
    {
        var sb = new StringBuilder();

        var lenght = items.Count;

        if (lenght > 0)
        {
            for (int i = 0; i < lenght; i++)
            {
                var json = JsonSerializer.Serialize(items[i]);
                sb.AppendLine(json);
            }

            await File.AppendAllTextAsync(_storagePath, sb.ToString());

            for (int i = 0; i < lenght; i++)
            {
                CombineLoger.Log($"The {items[i]} ==> saved in {_storagePath}");
            }
        }
    }

    public async Task SaveItem(Item item)
    {
        if (item != null)
        {
            var json = JsonSerializer.Serialize(item);
            await File.AppendAllTextAsync(_storagePath, json + "\n");
            CombineLoger.Log($"The {item} ==> saved in {_storagePath}");
        }
    }

    public async Task<Item> FindItemAsunc(int simpleID)
    {
        if (!File.Exists(_storagePath))
        {
            CombineLoger.Log("The storage is not initialized or missed. Process aborted.");
        }

        try
        {
            var content = await File.ReadAllLinesAsync(_storagePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            foreach (var line in content)
            {
                if (line.Contains($"\"SimpleID\":{simpleID}"))
                {
                    try
                    {
                        var temp = JsonSerializer.Deserialize<Item>(line);
                        if (temp != null && temp.SimpleID == simpleID)
                        {
                            return temp;
                        }
                    }
                    catch { } // to skip non-serialable lines
                }
            }
        }
        catch (Exception ex)
        {
            CombineLoger.Log($"Error: {ex.Message}");
        }

        CombineLoger.Log($"Cant find item with simpleID {simpleID} in the storage.");
        return null;
    }

    
}


using System.Text.Json;

namespace Example_2
{
    internal class StorageJSON : IStorage
    {
        private string _storagePath;

        public StorageJSON(string filePath)
        {
            _storagePath = filePath;
            CombineLoger.Log($"New storage in {filePath} created.");
        }

/*        public async Task DeleteItem(int simpleID)
        {
            if (!File.Exists(_storagePath))
            {
                await CombineLoger.Log("The storage is not initialized or missed. Process aborted.");
            }
            try
            {
                var content = await File.ReadAllLinesAsync(_storagePath);

                var listItems = content.ToList();

                var resultListItems = new List<string>();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                foreach (var line in content)
                {
                    try
                    {
                        var temp = JsonSerializer.Deserialize<Item>(line);
                        if (temp != null && temp.SimpleID != simpleID)
                        {
                            resultListItems
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                await CombineLoger.Log($"Error: {ex.Message}");
            }

        }*/

/*        public async Task<Item> FindItemAsunc(int simpleID)
        {
           if (!File.Exists(_storagePath))
            {
                await CombineLoger.Log("The storage is not initialized or missed. Process aborted.")
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
                    try 
                    { 
                        var temp = JsonSerializer.Deserialize<Item>(line); 
                        if (temp != null && temp.SimpleID == simpleID)
                        {
                            return temp;
                        }
                    } 
                    catch { }
                }
            }
            catch (Exception ex)
            {
                await CombineLoger.Log($"Error: {ex.Message}");
            }

            return null;
        }*/

        public void SaveCache(IEnumerable<Item> items)
        {
            foreach (var item in items)
            { 
                SaveItem(item).Wait();
            }

            
        }

        public async Task SaveItem(Item item)
        {
            if (item != null)
            {
                var json = JsonSerializer.Serialize(item);
                await File.AppendAllTextAsync(_storagePath, json + "\n");
                await CombineLoger.Log($"The {item} saved in {_storagePath}");
            }
        }

        /*public static async Task<List<string>> ReadFileUntilCondition(string filePath, Func<string, bool> breakCondition)
        {
            var linesRead = new List<string>();

            if (!File.Exists(filePath))
            {
                await CombineLoger.Log("The storage is not initialized or missed. Process aborted.");
                return linesRead; // Return an empty list if file doesn't exist
            }

            try
            {
                using StreamReader reader = new(filePath);
                string? line;

                while ((line = await reader.ReadLineAsync()) != null) 
                {
                    linesRead.Add(line); 

                    // Check if the break condition is met for the current line
                    if (breakCondition(line))
                    {
                        Console.WriteLine($"Break condition met on line: \"{line}\"");
                        break; // Exit the loop
                    }
                }
            }
            catch (IOException ex)
            {
                // Handle any IO errors that might occur during file reading
                Console.WriteLine($"An I/O error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

            return linesRead;
        }*/
    }
}

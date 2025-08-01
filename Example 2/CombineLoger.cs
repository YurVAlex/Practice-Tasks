using System.Text;

namespace Example_2;

internal static class CombineLoger
{
    private static string _logfilePath = @"D:\Repos\Practice-Tasks\Example 2\Logfile Folder\logfile.txt";

    private static int _logRecords = 0;

    public static StringBuilder _logCache = new();

    private static int _logCacheCapa = 10;

    private static readonly SemaphoreSlim _fileWriteSemaphore = new(1, 1);

    public static void Log(string message)
    {
        Console.WriteLine(message);

        _logRecords++;

        _logCache.AppendLine(message);

        /*if (CacheBoundReached())
        {
            string contentToSave = _logCache.ToString();
            RefreshCache();

            _ = Task.Run(async () =>
            {
                await _fileWriteSemaphore.WaitAsync(); // Wait for access to the file
                try
                {
                    await File.AppendAllTextAsync(_logfilePath, contentToSave);
                }
                catch (Exception ex) 
                {
                    Log(ex.Message);
                }
                finally
                {
                    _fileWriteSemaphore.Release(); // Release the semaphore
                }
            });*/

        if (CacheBoundReached())
        {
            try
            {
                File.AppendAllTextAsync(_logfilePath, _logCache.ToString());
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            RefreshCache();
        } 
    
    }

    private static void RefreshCache()
    {
        _logCache.Clear();
        _logRecords = 0;
    }

    private static bool CacheBoundReached()
    {
        return _logRecords >= _logCacheCapa;
    }
}
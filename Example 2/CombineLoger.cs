namespace Example_2;

internal static class CombineLoger
{
    private static string _logfilePath = @"D:\Practice-Tasks\Example 2\Logfile Folder\logfile.txt";

    public async static Task LogAsunc(string message)
    {
        Console.WriteLine(message);

        await File.AppendAllTextAsync(_logfilePath, message + "\n");

        await Task.Delay(3000);
    }

    public static void Log(string message)
    {
        Console.WriteLine(message);

        File.AppendAllTextAsync(_logfilePath, message + "\n");
    }
}

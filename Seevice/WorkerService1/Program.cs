namespace WorkerService1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<FileWatcherService>();

            var host = builder.Build();
            host.Run();
        }
    }

    public class FileWatcherService : BackgroundService
    {
        private readonly ILogger<FileWatcherService> _logger;

        public FileWatcherService(ILogger<FileWatcherService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var filePath = "folderEvents.txt";
            using var watcher = new FileSystemWatcher("D:\\Temp");

            // записываем изменения
            watcher.Changed += async (o, e) => await File.AppendAllTextAsync(filePath, $"{DateTime.Now} Changed: {e.FullPath}\n");
            watcher.Changed += (o, e) => _logger.LogInformation($"{DateTime.Now} Changed: {e.FullPath}\n");
            // записываем данные о создании файлов и папок
            watcher.Created += async (o, e) => await File.AppendAllTextAsync(filePath, $"{DateTime.Now} Created: {e.FullPath}\n");
            watcher.Created += (o, e) => _logger.LogInformation($"{DateTime.Now} Created: {e.FullPath}\n");
            // записываем данные об удалении файлов и папок
            watcher.Deleted += async (o, e) => await File.AppendAllTextAsync(filePath, $"{DateTime.Now} Deleted: {e.FullPath}\n");
            watcher.Deleted += (o, e) => _logger.LogInformation($"{DateTime.Now} Deleted: {e.FullPath}\n");
            // записываем данные о переименовании
            watcher.Renamed += async (o, e) => await File.AppendAllTextAsync(filePath, $"{DateTime.Now} Renamed: {e.OldFullPath} to {e.FullPath}\n");
            watcher.Renamed += (o, e) => _logger.LogInformation($"{DateTime.Now} Renamed: {e.OldFullPath} to {e.FullPath}\n");
            // записываем данные об ошибках
            watcher.Error += async (o, e) => await File.AppendAllTextAsync(filePath, $"{DateTime.Now} Error: {e.GetException().Message}\n");
            watcher.Error += (o, e) => _logger.LogInformation($"{DateTime.Now} Error: {e.GetException().Message}\n");

            watcher.IncludeSubdirectories = true; // отслеживаем изменения в подкаталогах
            watcher.EnableRaisingEvents = true;    // включаем события

            // если операция не отменена, то выполняем задержку в 200 миллисекунд
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(2000, stoppingToken);
            }
            await Task.CompletedTask;
        }
    }
}
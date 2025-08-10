namespace WorkerService1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<CoingeckoClientService>();

        var host = builder.Build();
        host.Run();
    }
}


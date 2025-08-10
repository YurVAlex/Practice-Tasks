using System.Globalization;
using System.Net.Http.Json;
using System.Text;

namespace WorkerService1;

internal class CoingeckoClientService : BackgroundService
{
    public class CryptoPrice
    {
        public decimal usd {  get; set; }
    }

    private readonly ILogger<CoingeckoClientService> _logger;

    public CoingeckoClientService(ILogger<CoingeckoClientService> logger)
    {
        _logger = logger;
    }

    private static string _urlPrefix = "https://api.coingecko.com/api/v3/simple/price?ids=";

    private static string _urlPostfix = "&vs_currencies=usd";

    public static Dictionary<string, decimal> Currency { get; private set; } = new Dictionary<string, decimal>
    {
        { "solana", 0 },
        { "algorand", 0 },
        { "wax", 0 },
        {"moonriver", 0 },
        {"boson-protocol", 0 }
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var socketHandler = new SocketsHttpHandler { PooledConnectionIdleTimeout = TimeSpan.FromMinutes (10) };

        _logger.LogInformation("\nLaunched HTTP client to get current prices from Coingecko.com...\n");

        while (!stoppingToken.IsCancellationRequested)
        {
            await FetchPricesAsync(socketHandler);
            DisplayPrices();
            await Task.Delay(30000, stoppingToken);
        }
        await Task.CompletedTask;

    }

    public async Task FetchPricesAsync(SocketsHttpHandler socketsHandler)
    {
        var sb = new StringBuilder();
        sb.Append(_urlPrefix);
        foreach (var item in Currency)
        {
            sb.Append(item.Key).Append(',');
        }
        sb.Append(_urlPostfix);
        string url = sb.ToString();

        
        using var client = new HttpClient(socketsHandler, false);
        client.BaseAddress = new Uri(url);

        try
        {
            var prices = await client.GetFromJsonAsync<Dictionary<string, CryptoPrice>>(url);

            if (prices != null)
            {
                foreach (var item in prices)
                {
                    if (Currency.ContainsKey(item.Key))
                    {
                        Currency[item.Key] = item.Value.usd;
                    }
                    else
                    {
                        Currency.Add(item.Key, item.Value.usd);
                    }
                }
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogInformation("Request error: " + e.Message);
        }
        catch (Exception e)
        {
            _logger.LogInformation("Error: " + e.Message);
        }
    }

    public void DisplayPrices()
    {
        var sbResult = new StringBuilder();
        sbResult.AppendLine("\nCurrent prices:\n");

        foreach (var item in Currency)
        {
            if (item.Key.Length > 8)
            {
                sbResult.AppendLine($"{item.Key} - {item.Value}$");
            }
            else
            {
                sbResult.AppendLine($"{item.Key} - {item.Value}$");
            }
        }

        sbResult.AppendLine();

        _logger.LogInformation($"{sbResult.ToString()}");
    }
}
namespace Wallet;

public class Program
{
    static async Task Main()
    {
        var socketsHandler = new SocketsHttpHandler 
        { 
            PooledConnectionLifetime = TimeSpan.FromMinutes(2) 
        };

        Timer timer = new Timer(async (_) => 
        await CoingeckoClient.FetchPrices(socketsHandler), null, 0, 60000);
        
        Console.ReadKey();
    }
    
}




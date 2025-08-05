namespace Wallet;

public class Program
{
    static async Task Main()
    {
        var socketsHandler = new SocketsHttpHandler 
        { 
            PooledConnectionLifetime = TimeSpan.FromMinutes(2) 
        };

        await CoingeckoClient.FetchPrices(socketsHandler);

        var sol = new Coin(CoinsBase.CoinsList[0]);
        var algo = new Coin(CoinsBase.CoinsList[2]);

        var wallet1 = new Billfold();

        wallet1.AddCoins(sol, (decimal)1.125);
        wallet1.AddCoins(algo, 57);

        var wallet2 = new Billfold();
        wallet2.AddCoins(sol, (decimal)5.25);
        wallet2.AddCoins(algo, (decimal)325.57);

        await Task.Delay(60000);

        await TimerLoop.LaunchBatchCombineAsync(60000, CoingeckoClient.FetchPrices(socketsHandler),
            wallet1.ShowKeeping, wallet2.ShowKeeping);

        Console.ReadLine();
    }
}
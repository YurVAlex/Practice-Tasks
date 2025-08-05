namespace Wallet;

public class Program
{
    static async Task Main()
    {
        var socketsHandler = new SocketsHttpHandler 
        { 
            PooledConnectionLifetime = TimeSpan.FromMinutes(2) 
        };

        Timer timer1 = new Timer(async (_) => 
        await CoingeckoClient.FetchPrices(socketsHandler), null, 0, 60000);

        await Task.Delay(2000);

        var sol = new Coin(CoinsBase.CoinsList[0]);
        var algo = new Coin(CoinsBase.CoinsList[2]);

        var wallet1 = new Billfold();

        wallet1.AddCoins(sol, (decimal)1.125);
        wallet1.AddCoins(algo, 57);

        var wallet2 = new Billfold();
        wallet2.AddCoins(sol, (decimal)5.25);
        wallet2.AddCoins(algo, (decimal)325.57);

        Timer timer2 = new Timer((_) => wallet1.ShowKeeping(), null, 0, 60000);

        await Task.Delay(2000);

        Timer timer3 = new Timer((_) => wallet2.ShowKeeping(), null, 0, 60000);

        Console.ReadLine();
    }
}
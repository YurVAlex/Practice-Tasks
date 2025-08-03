namespace Wallet;

public class Program
{
    static async Task Main()
    {
        var socketsHandler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) };
        Timer timer = new Timer(async (_) => await FetchAndDisplayPrices(socketsHandler), null, 0, 60000);
        Console.ReadKey();
    }
    static async Task FetchAndDisplayPrices(SocketsHttpHandler socketsHandler)
    {
        string url = "https://api.coingecko.com/api/v3/simple/price?ids=solana,kardiachain,algorand,wax,kira-network,rmrk,launchpool,boson-protocol,moonriver&vs_currencies=usd";
        using HttpClient client = new HttpClient(socketsHandler, false);
        client.BaseAddress = new Uri(url);
        try
        {
            string response = await client.GetStringAsync(url); //Send request and get response


            Console.Clear();
            Console.WriteLine("Prises in USD:");

            string[] strings = response.Split(',');
            foreach (string item in strings)
            {
                string[] tempSubString = item.Split(':');

                if (tempSubString[0].Length < 8)
                    Console.WriteLine(tempSubString[0].Trim('{') + "\t\t\t" + tempSubString[2].Trim('}'));

                if (tempSubString[0].Length >= 8 && tempSubString[0].Length < 16)
                    Console.WriteLine(tempSubString[0].Trim('{') + "\t\t" + tempSubString[2].Trim('}'));

                if (tempSubString[0].Length >= 16)
                    Console.WriteLine(tempSubString[0].Trim('{') + "\t" + tempSubString[2].Trim('}'));
            }

            Console.WriteLine("Press any key to exit...");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Request error: " + e.Message);
        }
    }
}




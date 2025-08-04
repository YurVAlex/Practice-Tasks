using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet;

internal static class CoingeckoClient
{
    private static string  _urlPrefix = "https://api.coingecko.com/api/v3/simple/price?ids=";

    private static string  _urlPostfix = "&vs_currencies=usd";

    public static Dictionary<string, decimal> Currency = [];

    public static async Task FetchPrices(SocketsHttpHandler socketsHandler)
    {
        var sb = new StringBuilder();

        sb.Append(_urlPrefix).Append(CoinsBase.GetCoinsNames()).Append(_urlPostfix);

        string url = sb.ToString();
        using var client = new HttpClient(socketsHandler, false);
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

                Currency.Add(tempSubString[0].Trim('}'), decimal.Parse(tempSubString[2].Trim('}')));
            }

            Console.WriteLine("Press any key to exit...");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Request error: " + e.Message);
        }
    }
}
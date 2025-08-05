using System;
using System.Collections.Generic;
using System.Globalization;
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
            Console.WriteLine("Launched HTTP client to get current prices from Coingecko.com...");

            string[] strings = response.Split(',');
            foreach (string item in strings)
            {
                string[] tempSubString = item.Split(':');
                string returnedName = tempSubString[0].Trim('}', '}', '{', '\\', '"');
                decimal returnedPrice = Convert.ToDecimal(tempSubString[2].Trim('}'), CultureInfo.InvariantCulture);

                if (Currency.ContainsKey(returnedName))
                {
                    Currency[returnedName] = returnedPrice;
                }
                else
                {
                    Currency.Add(returnedName, returnedPrice);
                }
            }

            Console.WriteLine("Press any key to exit...");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Request error: " + e.Message);
        }
    }
}
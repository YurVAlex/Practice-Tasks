using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Wallet
{
    internal static class CoinsBase
    {
        public static List<string> CoinsList { get; set; } =
        [
            "solana", 
            "kardiachain",
            "algorand",
            "wax",
            "kira-network",
            "rmrk",
            "launchpool",
            "boson-protocol",
            "moonriver"
        ];

        internal static void AddToBaseCoin(string coinName)
        { 
            CoinsList.Add(coinName); 
        }

        public static void RemoveCoinByName(string name)
        {
            if (CoinsList.Contains(name) )
            {
                CoinsList.Remove(name);
            }
            else
            {
                Console.WriteLine($"Coin with name ({name}) does not exists in base.");
            }
        }

        internal static string GetCoinsNames()
        {
            var sb = new StringBuilder();

            foreach (var coinName in CoinsList)
            {
                sb.Append(coinName + ",");
            }
            return sb.ToString();
        }
    }
}

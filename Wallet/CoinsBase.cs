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
        private static List<Coin> _coinsBase =
        {
            { solana, CoingeckoClient.Currency[solana] }
            kardiachain,
            algorand,
            wax,
            kira-network,
            rmrk,
            launchpool,
            boson-protocol,
            moonriver
        };

        internal static void AddToBaseCoin(Coin coin)
        { 
            _coinsBase.Add(coin); 
        }

        public static void RemoveCoinByName(string name)
        {
            var temp = _coinsBase.FirstOrDefault(_ => (_.Name == name));

            if (temp == null)
            {
                Console.WriteLine($"Coin with name ({name}) does not exists in base.");
            }
            else
            { 
                _coinsBase.Remove(temp);
            }
        }

        internal static string GetCoinsNames()
        {
            var sb = new StringBuilder();

            foreach (Coin coin in _coinsBase)
            {
                sb.Append(coin.Name + ",");
            }
            return sb.ToString();
        }
    }
}

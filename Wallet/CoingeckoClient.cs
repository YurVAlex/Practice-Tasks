using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet;

internal class CoingeckoClient
{
    private string  _urlPrefix = "https://api.coingecko.com/api/v3/simple/price?ids=solana,kardiachain,algorand,wax,kira-network,rmrk,launchpool,boson-protocol,moonriver&vs_currencies=usd";

    private string  _urlPostfix = "&vs_currencies=usd";
}


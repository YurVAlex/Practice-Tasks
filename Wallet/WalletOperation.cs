using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet;

internal class WalletOperation
{
    public string ShowItemsWithAmountAndPrices(Wallet wallet)
    {
        var sbResult = new StringBuilder();

        foreach (var item in wallet.Coins)
        { 
            sbResult.AppendLine(item.Key + ", amount : " + item.Value.ToString() + wallet.Price);
        }
    }
}

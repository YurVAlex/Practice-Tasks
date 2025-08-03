using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet;

internal class Coin
{
    public string Name { get; set; }

    public decimal Price
    {
        get; set;
    }

    public Coin(string name, decimal currentPrice)
    {
        
    }

    private async Task <decimal> GetCurrentPriceAsync(string name)
    { 
    
    }
}

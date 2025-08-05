using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Wallet;

internal class Billfold
{
    private Dictionary<Coin,decimal> _coins { get; set; } = [];

    public void AddCoins(Coin coin, decimal amount)
    {
        if (_coins.ContainsKey(coin))
        {
            _coins[coin] += amount;
        }
        else
        {
            _coins.Add(coin, amount);
        }
    }

    public void WithdrawCoins(Coin coin, decimal amount)
    {
        var value = _coins[coin];
        value -= amount;
    }

    public void ShowKeeping()
    {
        int number = 0;
        decimal total = 0;  

        Console.WriteLine("\nWallet keeping:");

        foreach (var coin in _coins)
        {
            number++;
            var subTotal = coin.Key.Price * coin.Value;
            Console.WriteLine($"{number}: {coin.Key} - {coin.Value} pieces. Total worth: {subTotal}$");
            total += subTotal;
        }

        Console.WriteLine($"\nOverall worth: {total}");
    }
}
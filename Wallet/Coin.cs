namespace Wallet;

internal class Coin
{
    public string Name { get; set; }

    public decimal Price { get; set; }

    public Coin(string name)
    {
        if (CoingeckoClient.Currency.ContainsKey(name))
        {
            Name = name;
            Price = CoingeckoClient.Currency[name];
        }
        else
        {
            throw new Exception($"The coin with name \"{name}\" are not in the base!");
        }
    }

    public override string ToString()
    {
        return $"{Name} (Current price: {Price}) ";
    }
}

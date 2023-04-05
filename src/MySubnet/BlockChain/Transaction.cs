using System.Text.Json.Serialization;
using MySubnet.Avalanche;
using MySubnet.Shared;

namespace MySubnet.BlockChain;

public record Transaction(string From, string To, ulong Amount, TransactionStatus Status, string Hash)
{
    public static Transaction Create(string from, string to, ulong amount)
    {
        var tx = new Transaction(from, to, amount, TransactionStatus.Pending, string.Empty);
        return tx with
        {
            Hash = tx.HashTransaction()
        };
    }

    internal string HashTransaction()
    {
        var hashLessTx = this with
        {
            Hash = Id.Default.ToString()
        };
        return hashLessTx.HashObject();
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionStatus
{
    Pending,
    Accepted,
    Rejected
}
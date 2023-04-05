using MySubnet.Avalanche;
using MySubnet.Shared;

namespace MySubnet.BlockChain;

public record Block(Dictionary<string, ulong> Balances, Transaction[] Transactions, ulong Height, long Timestamp,
    string Hash, string ParentHash)
{
    public static Block Create(Dictionary<string, ulong> balances, Transaction[] transactions, ulong height,
        string parentHash)
    {
        var block = new Block(balances, transactions,
            height, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            string.Empty, parentHash);
        return block with
        {
            Hash = block.HashBlock()
        };
    }

    internal string HashBlock()
    {
        var hashLessBlock = this with
        {
            Hash = Id.Default.ToString()
        };
        return hashLessBlock.HashObject();
    }
}
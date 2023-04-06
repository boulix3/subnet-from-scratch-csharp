using MySubnet.Avalanche;
using MySubnet.Shared;

namespace MySubnet.BlockChain;

public static class Genesis
{
    internal static void Create()
    {
        var genesisBlock = new Block(
            new Dictionary<string, ulong> { { "Alice", 1000 }, { "Bob", 1000 } },
            new Transaction[] { },
            0,
            DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Id.Default.ToString(),
            Id.Default.ToString());
        var hash = genesisBlock.HashObject();
        genesisBlock = genesisBlock with
        {
            Hash = hash
        };
        File.WriteAllText("genesis.json", genesisBlock.SerializeJson());
    }
}
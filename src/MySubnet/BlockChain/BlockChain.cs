using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using MySubnet.Avalanche;
using Vm;

namespace MySubnet.BlockChain;

public class BlockChain
{
    private readonly ILogger _logger;

    public BlockChain(ILogger<BlockChain> logger)
    {
        _logger = logger;
    }

    public State State { get; set; } = State.Unspecified;
    public ConcurrentDictionary<string, Block> FinalBlocks { get; } = new();
    public ConcurrentDictionary<string, Block> PendingBlocks { get; } = new();
    public ConcurrentDictionary<string, Transaction> TransactionPool { get; } = new();
    public Block? PreferredBlock { get; set; }
    public string LatestBlockHash { get; set; } = Id.Default.ToString();
    public Block LatestBlock => FinalBlocks[LatestBlockHash];

    internal void Accept(Id id)
    {
        var hash = id.ToString();
        _logger.LogInformation("Blockchain.Accept - {hash}", hash);
        if (PendingBlocks.TryGetValue(hash, out var block))
            AddFinalBlock(block);
        else
            _logger.LogError("Blockchain.Accept error block not found - {hash} ", hash);
        PendingBlocks.Clear();
        PreferredBlock = null;
    }

    internal void AddPendingBlock(Block block)
    {
        if (block.ParentHash == LatestBlockHash) PendingBlocks.TryAdd(block.Hash, block);
    }

    internal void AddPendingTransaction(Transaction transaction)
    {
        TransactionPool.TryAdd(transaction.Hash, transaction);
    }

    internal Block BuildBlock(IEnumerable<Transaction> pendingTransactions)
    {
        if (PreferredBlock != null) return PreferredBlock;
        var lastBlock = FinalBlocks[LatestBlockHash];
        var balances = lastBlock.Balances.ToDictionary(k => k.Key, v => v.Value);
        var transactions = new List<Transaction>();
        foreach (var tx in pendingTransactions)
            if (balances.ContainsKey(tx.From) && balances[tx.From] > tx.Amount)
            {
                balances[tx.From] -= tx.Amount;
                if (!balances.ContainsKey(tx.To)) balances.Add(tx.To, 0);
                balances[tx.To] += tx.Amount;
                transactions.Add(tx with { Status = TransactionStatus.Accepted });
            }
            else
            {
                transactions.Add(tx with { Status = TransactionStatus.Rejected });
            }

        var block = Block.Create(
            balances,
            transactions.ToArray(),
            lastBlock.Height + 1,
            lastBlock.Hash);
        PendingBlocks.TryAdd(block.Hash, block);
        return block;
    }

    internal (Block?, Status) GetBlock(Id id)
    {
        var hash = id.ToString();
        if (FinalBlocks.TryGetValue(hash, out var acceptedBlock)) return (acceptedBlock, Status.Accepted);
        if (PendingBlocks.TryGetValue(hash, out var pendingBlock)) return (pendingBlock, Status.Processing);
        return (null, Status.Unspecified);
    }

    internal void Reject(Id id)
    {
        var hash = id.ToString();
        if (PreferredBlock?.Hash == hash) PreferredBlock = null;
        PendingBlocks.TryRemove(hash, out var _);
    }

    internal void SetPreference(Id id)
    {
        var hash = id.ToString();
        if (PendingBlocks.TryGetValue(hash, out var block)) PreferredBlock = block;
    }

    internal bool ShouldBuildBlock()
    {
        if (State != State.NormalOp) return false;
        CleanPendingBlocks();
        return PendingBlocks.IsEmpty && !TransactionPool.IsEmpty;
    }

    private void CleanPendingBlocks()
    {
        var expectedParentHash = LatestBlock.Hash;
        var keysToRemove = PendingBlocks.Where(x => x.Value.ParentHash != expectedParentHash).Select(x => x.Key)
            .ToArray();
        foreach (var key in keysToRemove) PendingBlocks.TryRemove(key, out var _);
    }

    internal bool Verify([NotNullWhen(true)] Block? block)
    {
        if (block != null)
        {
            var expectedHash = block.HashBlock();
            if (expectedHash == block.Hash
                && RebuildBlockHash(block) == block.Hash)
                return true;
            _logger.LogInformation(
                "Block verify failed. Actual : {block}, expectedHash : {expectedHash}, rebuiltBlock : {rebuiltBlock}",
                block, expectedHash, BuildBlock(block.Transactions));
        }

        return false;
    }

    private string RebuildBlockHash(Block block)
    {
        block = BuildBlock(block.Transactions) with
        {
            Timestamp = block.Timestamp
        };
        return block.HashBlock();
    }

    internal void AddFinalBlock(Block block)
    {
        FinalBlocks.TryAdd(block.Hash, block);
        LatestBlockHash = block.Hash;
        foreach (var tx in block.Transactions) TransactionPool.TryRemove(tx.Hash, out var _);
    }
}
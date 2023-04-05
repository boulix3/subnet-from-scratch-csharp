namespace MySubnet.Shared;

public record GlobalSettings(string RuntimeUrl, CancellationTokenSource TokenSource, uint ProtocolVersion)
{
    public string? AvalancheRpcUrl { get; set; }
    public int? VmPort { get; set; }
}
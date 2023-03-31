namespace MySubnet.Shared;
public record GlobalSettings(string Url, CancellationTokenSource TokenSource, uint ProtocolVersion);
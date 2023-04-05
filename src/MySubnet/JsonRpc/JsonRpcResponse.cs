namespace MySubnet.JsonRpc;

public record JsonRpcResponse(
    string Jsonrpc,
    object? Id,
    JsonRpcError? Error,
    object? Result);

public record JsonRpcError(string Code, string Message, object? Data);
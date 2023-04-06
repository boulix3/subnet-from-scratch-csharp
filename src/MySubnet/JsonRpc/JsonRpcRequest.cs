using System.Text.Json;
using System.Text.Json.Serialization;
using MySubnet.Shared;

namespace MySubnet.JsonRpc;

public record JsonRpcRequest(
    object Id,
    string Jsonrpc,
    string Method,
    [property: JsonPropertyName("params")] JsonElement? Parameters)
{
    public T? DeserializeParameters<T>()
    {
        return Parameters.HasValue ? Parameters.Value.Deserialize<T>(HelperExtensions.DefaultJsonOptions) : default;
    }
}
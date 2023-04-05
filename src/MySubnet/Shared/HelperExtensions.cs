using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace MySubnet.Shared;

public static class HelperExtensions
{
    private const string HexPrefix = "0x";

    public static readonly JsonSerializerOptions DefaultJsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    public static string HashObject(this object item)
    {
        var json = JsonSerializer.Serialize(item);
        return Sha256(json);
    }

    public static string Sha256(this string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return SHA256.HashData(bytes).BytesToHexString();
    }

    public static string BytesToHexString(this byte[] bytes)
    {
        return HexPrefix + Convert.ToHexString(bytes).ToLower(CultureInfo.InvariantCulture);
    }

    public static ByteString HexStringToByteString(this string hexString)
    {
        if (hexString.StartsWith(HexPrefix, true, CultureInfo.InvariantCulture)) hexString = hexString[2..];
        var bytes = Convert.FromHexString(hexString);
        return ByteString.CopyFrom(bytes);
    }

    public static string SerializeJson(this object? item)
    {
        return item == null
            ? "null"
            : JsonSerializer.Serialize(item, DefaultJsonOptions);
    }

    public static T? DeserializeJson<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultJsonOptions);
    }

    public static ByteString SerializeJsonByteString(this object? item)
    {
        return ByteString.CopyFromUtf8(item.SerializeJson());
    }

    public static T? DeserializeJsonByteString<T>(this ByteString byteString)
    {
        return byteString.ToStringUtf8().DeserializeJson<T>();
    }

    public static Timestamp LongToTimestamp(this long milliseconds)
    {
        return Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(milliseconds));
    }
}
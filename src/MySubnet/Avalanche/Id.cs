using Google.Protobuf;
using MySubnet.Shared;

namespace MySubnet.Avalanche;

internal class Id
{
    public static Id Default = new(new byte[32]);
    private readonly byte[] _bytes;

    private Id(byte[] bytes)
    {
        _bytes = bytes;
    }

    public bool IsValid => _bytes.Length == 32;

    public static implicit operator ByteString(Id id)
    {
        return ByteString.CopyFrom(id._bytes);
    }

    public static explicit operator Id(ByteString byteString)
    {
        return new(byteString.ToByteArray());
    }

    public override string ToString()
    {
        return _bytes.BytesToHexString();
    }
}
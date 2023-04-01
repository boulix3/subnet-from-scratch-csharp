using Google.Protobuf;

namespace MySubnet.Avalanche
{
    internal class Id
    {
        private readonly byte[] _bytes;
        private Id(byte[] bytes)
        {
            _bytes = bytes;
        }
        public bool IsValid => _bytes.Length == 32;
        public static Id Empty = new Id(new byte[32]);
        public static implicit operator ByteString(Id id) => id.ToByteString();
        public ByteString ToByteString()
        {
            return ByteString.CopyFrom(_bytes);
        }
        public static explicit operator Id(ByteString byteString) => FromByteString(byteString);
        public static Id FromByteString(ByteString byteString)
        {
            return new Id(byteString.ToByteArray());
        }
    }
}
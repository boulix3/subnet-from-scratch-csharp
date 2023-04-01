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
        public static implicit operator ByteString(Id id) => ByteString.CopyFrom(id._bytes);
        public static explicit operator Id(ByteString byteString) => new Id(byteString.ToByteArray());       
        public static Id Default = new Id(new byte[32]);
    }
}
using System;

namespace net.azirale.geosharer.core
{
    class ChunkIndex : IEquatable<ChunkIndex>
    {
        private long indexValue;

        private ChunkIndex() { }

        public ChunkIndex(int x, int z)
        {
            this.indexValue = ((long)(x) << 32) + (long)z;
        }

        public int X
        {
            get { return (int)(this.indexValue >> 32); }
        }

        public int Z
        {
            get { return (int)(this.indexValue & 0xFFFF); }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChunkIndex)) return false;
            return this.Equals((ChunkIndex)obj);
        }

        public override int GetHashCode()
        {
            return indexValue.GetHashCode();
        }

        public override string ToString()
        {
            return this.X.ToString() + ',' + this.Z.ToString();
        }

        public bool Equals(ChunkIndex other)
        {
            return this.indexValue == other.indexValue;
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(BitConverter.IsLittleEndian ? this.indexValue : Endian.Reverse(this.indexValue));
        }

        public static ChunkIndex FromBytes(byte[] bytes, int offset)
        {
            long indexValue = BitConverter.IsLittleEndian ? BitConverter.ToInt64(bytes, offset) : Endian.Reverse(BitConverter.ToInt64(bytes, offset));
            ChunkIndex value = new ChunkIndex();
            value.indexValue = indexValue;
            return value;
        }
    }
}

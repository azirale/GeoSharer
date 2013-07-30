using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net.azirale.civcraft.GeoSharer
{
    class GeoChunk : IComparable<GeoChunk>, IEquatable<GeoChunk>, IEquatable<Substrate.ChunkRef>
    {
        public long TimeStamp { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
        public GeoBiomeArray Biomes { get; set; }
        public GeoBlockCollection Blocks { get; set; }

        public bool ParseByteArray(byte[] chunkBytes)
        {
            TimeStamp = Endian.Reverse(BitConverter.ToInt64(chunkBytes, 0));

            X = Endian.Reverse(BitConverter.ToInt32(chunkBytes, 8));
            Z = Endian.Reverse(BitConverter.ToInt32(chunkBytes, 12));
            byte[] biomeBytes = new byte[256];
            Array.Copy(chunkBytes, 16, biomeBytes, 0, 256);
            Biomes = new GeoBiomeArray(biomeBytes);
            byte[] blockBytes = new byte[2 * 16 * 16 * 256];
            Array.Copy(chunkBytes, 16 + 256, blockBytes, 0, 2 * 16 * 16 * 256);
            Blocks = new GeoBlockCollection(blockBytes);
            return true;
        }

        public string TimeStampText
        {
            get
            {
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dt = dt.AddSeconds(this.TimeStamp / 1000).ToLocalTime();
                return dt.ToString("ddMMMyyyy-HH:mm:ss").ToUpper();
            }
        }

        public new string ToString()
        {
            return "GeoChunk: X[" + this.X.ToString() + "] Z[" + this.Z.ToString() + "] TimeStamp[" + this.TimeStampText + "]";
        }

        public int CompareTo(GeoChunk other)
        {
            return TimeStamp.CompareTo(other.TimeStamp);
        }

        public bool Equals(GeoChunk other)
        {
            return this.X == other.X && this.Z == other.Z;
        }

        public bool Equals(Substrate.ChunkRef other)
        {
            return this.X == other.X && this.Z == other.Z;
        }
    }

    class GeoBlockCollection : IEnumerable<GeoBlock>
    {
        private readonly GeoBlockData[] Blocks;
        public GeoBlockData BlockAt(int x, int y, int z)
        {
            return Blocks[y * 256 + x * 16 + z];
        }
        public GeoBlockCollection(byte[] blockBytes)
        {
            if (blockBytes.Length != 131072) Blocks = null;
            else
            {
                this.Blocks = new GeoBlockData[16 * 16 * 256];
                for (int y = 0; y < 256; ++y)
                {
                    for (int x = 0; x < 16; ++x)
                    {
                        for (int z = 0; z < 16; ++z)
                        {
                            Blocks[y * 256 + x * 16 + z] = new GeoBlockData(blockBytes[(y * 256 + x * 16 + z)*2], blockBytes[(y * 256 + x * 16 + z)*2 + 1]);
                        }
                    }
                }
            }
        }

        public IEnumerator<GeoBlock> GetEnumerator()
        {
            return new GeoBlockCollectionEnum(Blocks);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        // Enumerator for this class
        public class GeoBlockCollectionEnum : IEnumerator<GeoBlock>, IDisposable
        {
            private GeoBlockData[] Blocks;
            int position;
            public GeoBlockCollectionEnum(GeoBlockData[] blocks)
            {
                this.Blocks = blocks;
                position = -1;
            }

            public bool MoveNext()
            {
                position++;
                position++;
                return (position < Blocks.Length);
            }

            public void Reset()
            {
                position = -1;
            }

            object IEnumerator.Current { get { return Current; } }

            public GeoBlock Current
            {
                get
                {
                    if (position >= Blocks.Length) throw new InvalidOperationException();
                    int i = position/2;
                    byte z = (byte)(i % 16);
                    i /= 16;
                    byte x = (byte)(i % 16);
                    i /= 16;
                    byte y = (byte)(i);
                    
                    
                    return new GeoBlock(new GeoBlockVector(x, y, z), Blocks[y * 256 + x * 16 + z]);
                }
            }

            public void Dispose()
            {
                // ?
            }
        }
    }

    class GeoBlock
    {
        private readonly GeoBlockVector position;
        private readonly GeoBlockData data;
        public int X { get { return position.X; } }
        public int Y { get { return position.Y; } }
        public int Z { get { return position.Z; } }
        public int ID { get { return data.ID; } }
        public int Meta { get { return data.Meta; } }
        public GeoBlock(GeoBlockVector position, GeoBlockData data)
        {
            this.position = position;
            this.data = data;
        }
    }

    struct GeoBlockData
    {
        public readonly byte ID;
        public readonly byte Meta;

        public GeoBlockData(byte id, byte meta)
        {
            ID = id;
            Meta = meta;
        }
    }

    struct GeoBlockVector
    {
        public readonly byte X;
        public readonly byte Y;
        public readonly byte Z;

        public GeoBlockVector(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }

    class GeoBiomeArray
    {
        public byte BiomeAt(int x, int z)
        {
            return BiomeValues[x * 16 + z];
        }
        private readonly byte[] BiomeValues;
        public GeoBiomeArray(byte[] biomeValues)
        {
            if (biomeValues.Length != 256) this.BiomeValues = null;
            else this.BiomeValues = biomeValues;
        }
    }


}

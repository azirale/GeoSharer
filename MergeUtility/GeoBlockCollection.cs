using System;
using System.Collections;
using System.Collections.Generic;

namespace net.azirale.civcraft.GeoSharer
{
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
                            Blocks[y * 256 + x * 16 + z] = new GeoBlockData(blockBytes[(y * 256 + x * 16 + z) * 2], blockBytes[(y * 256 + x * 16 + z) * 2 + 1]);
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
                    int i = position / 2;
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

}

using System;
using System.Collections;
using System.Collections.Generic;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// A collection of all of the block data for a chunk, as collected by GeoSharer mod
    /// </summary>
    public class GeoBlockCollection : IEnumerable<GeoBlock>
    {
        /***** CONSTRUCTOR MEMBERS **************************************************************/
        #region Constructor Members

        /// <summary>
        /// Create a new GeoBlockCollection object based on individual byte arrays for BlockID and BlockMeta, and maximum Y level from GeoSharer mod chunk data
        /// </summary>
        /// <param name="idBytes">Byte array of Block ID values in YXZ order</param>
        /// <param name="dataBytes">Byte array of 4-bit Block Meta values in YXZ order, with alternating Z values merged into single bytes</param>
        /// <param name="maxY">Maximum Y height of data int he idBytes and dataBytes arrays</param>
        public GeoBlockCollection(byte[] idBytes, byte[] dataBytes, int maxY)
        {
            this.Blocks = new GeoBlockData[MC.ChunkVolume];
            for (int y = 0; y <= maxY; ++y)
            {
                for (int x = 0; x < MC.ChunkWidth; ++x)
                {
                    for (int z = 0; z < MC.ChunkWidth; ++z)
                    {
                        byte id = idBytes[y * MC.ChunkArea + x * MC.ChunkWidth + z];
                        byte data = dataBytes[(y * MC.ChunkArea + x * MC.ChunkWidth + z) / 2]; // halved because two 4-bit meta values are in each byte
                        data = z % 2 == 0 ? (byte)(data >> 4) : (byte)(data & 0x00FF); // get the half of the byte needed for the 4-bit meta value
                        Blocks[y * MC.ChunkArea + x * MC.ChunkWidth + z] = new GeoBlockData(id, data);
                    }
                }
            }
            // Add air blocks
            for (int y = maxY + 1; y < MC.ChunkHeight; ++y)
            {
                for (int x = 0; x < MC.ChunkWidth; ++x)
                {
                    for (int z = 0; z < MC.ChunkWidth; ++z)
                    {
                        Blocks[y * MC.ChunkArea + x * MC.ChunkWidth + z] = new GeoBlockData(0, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Create a new GeoBlockCollection object based on a byte array of block data from the GeoSharer mod [DEPRECATED]
        /// </summary>
        /// <param name="blockBytes">Byte array of block data from GeoSharer mod</param>
        public GeoBlockCollection(byte[] blockBytes)
        {
            if (blockBytes.Length != MC.ChunkVolume * 2 /* 1x for ID, 1x for Meta */) this.Blocks = null;
            else
            {
                this.Blocks = new GeoBlockData[MC.ChunkVolume];
                for (int y = 0; y < MC.ChunkHeight; ++y)
                {
                    for (int x = 0; x < MC.ChunkWidth; ++x)
                    {
                        for (int z = 0; z < MC.ChunkWidth; ++z)
                        {
                            Blocks[y * MC.ChunkHeight + x * MC.ChunkWidth + z] =
                                new GeoBlockData(
                                blockBytes[(y * MC.ChunkHeight + x * MC.ChunkWidth + z) * 2],
                                blockBytes[(y * MC.ChunkHeight + x * MC.ChunkWidth + z) * 2 + 1]
                                );
                        }
                    }
                }
            }
        }
        #endregion

        /***** PRIVATE FIELDS *******************************************************************/
        #region Private Fields
        /// <summary>
        /// Internal storage array for all of the block data
        /// </summary>
        private readonly GeoBlockData[] Blocks;
        #endregion

        /***** PUBLIC READONLY PROPERTIES *******************************************************/
        #region Public Readonly Properties
        public GeoBlockData this[int x, int y, int z]
        {
            get { return Blocks[y * MC.ChunkHeight + x * MC.ChunkWidth + z]; }
        }
        #endregion

        /***** IENUMERABLE IMPLEMENTATION *******************************************************/
        #region IEnumerable Implementation

        public IEnumerator<GeoBlock> GetEnumerator()
        {
            return new GeoBlockCollectionEnum(Blocks);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        /// <summary>
        /// IEnumerator class for GeoBlockCollection implementation of IEnumerable
        /// </summary>
        private class GeoBlockCollectionEnum : IEnumerator<GeoBlock>, IDisposable
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
                    int i = position;
                    byte z = (byte)(i % 16);
                    i /= 16;
                    byte x = (byte)(i % 16);
                    i /= 16;
                    byte y = (byte)(i);


                    return new GeoBlock(new GeoBlockVector(x, y, z), Blocks[y * MC.ChunkHeight + x * MC.ChunkWidth + z]);
                }
            }

            public void Dispose()
            {
                // not needed in this instance
            }
        }
        #endregion
    }
}
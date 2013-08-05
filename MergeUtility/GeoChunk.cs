using Substrate.Core;
using System;

namespace net.azirale.civcraft.GeoSharer
{
    class GeoChunk : IComparable<GeoChunk>, IEquatable<GeoChunk>, IEquatable<Substrate.ChunkRef>
    {

        /***** CONSTRUCTOR MEMBERS **************************************************/
        // default constructor, just call new GeoChunk, then ParseByteArray to fill it with data
        public GeoChunk() { }

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

        /***** PUBLIC READ / PRIVATE WRITE PROPERTIES *******************************/
        public long TimeStamp { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
        public GeoBiomeArray Biomes { get; set; }
        public GeoBlockCollection Blocks { get; set; }

        /***** PUBLIC READ PROPERTIES ***********************************************/
        public string TimeStampText
        {
            get
            {
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dt = dt.AddSeconds(this.TimeStamp / 1000).ToLocalTime();
                return dt.ToString("ddMMMyyyy-HH:mm:ss").ToUpper();
            }
        }

        /***** PUBLIC METHODS *******************************************************/
        public new string ToString()
        {
            return "GeoChunk: X[" + this.X.ToString() + "] Z[" + this.Z.ToString() + "] TimeStamp[" + this.TimeStampText + "]";
        }

        /***** INTERFACE IMPLEMENTATIONS ********************************************/
        #region Interface Implementations
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
        #endregion
    }











}

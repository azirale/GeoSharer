using Substrate.Core;
using System;
using System.IO;
using System.IO.Compression;

namespace net.azirale.civcraft.GeoSharer
{
    public class GeoChunk : IChunkSync, IEquatable<Substrate.ChunkRef>
    {
        /***** CONSTRUCTOR MEMBERS **************************************************/
        // private constructor, use factory method
        private GeoChunk() { }

        // factory method
        public static GeoChunk FromText(string text)
        {
            byte[] chunkData = GetArrayFromText(text);
            GeoChunk value = new GeoChunk();
            if (!value.ParseByteArray(chunkData)) return null;
            return value;
        }

        private static byte[] GetArrayFromText(string text)
        {
            if (text == null) return null;
            byte[] decode = Convert.FromBase64String(text);
            MemoryStream inStream = new MemoryStream(decode);
            GZipStream zipper = new GZipStream(inStream, CompressionMode.Decompress);
            MemoryStream outStream = new MemoryStream();
            zipper.CopyTo(outStream);
            byte[] value = outStream.ToArray();
            return value;
        }

        private bool ParseByteArray(byte[] chunkBytes)
        {
            try
            {
                int offset = 0;
                // Version of saved geosharer chunk from mod
                byte version = chunkBytes[offset];
                this.Version = version;
                offset++;
                // time stamp for when chunk was stored
                TimeStamp = Endian.Reverse(BitConverter.ToInt64(chunkBytes, offset));
                offset += 8;
                // chunk X position
                X = Endian.Reverse(BitConverter.ToInt32(chunkBytes, offset));
                offset += 4;
                // chunk Z position
                Z = Endian.Reverse(BitConverter.ToInt32(chunkBytes, offset));
                offset += 4;
                // biome data
                byte[] biomeBytes = new byte[256];
                Array.Copy(chunkBytes, offset, biomeBytes, 0, 256);
                Biomes = new GeoBiomeArray(biomeBytes);
                offset += biomeBytes.Length;
                byte maxY = chunkBytes[offset];
                this.MaxY = maxY;
                offset++;
                // block ids
                byte[] idBytes = new byte[16 * 16 * (maxY + 1)];
                Array.Copy(chunkBytes, offset, idBytes, 0, idBytes.Length);
                offset += idBytes.Length;
                // block data
                byte[] dataBytes = new byte[8 * 16 * (maxY + 1)];
                Array.Copy(chunkBytes, offset, dataBytes, 0, 8 * 16 * (maxY + 1));
                offset += dataBytes.Length;
                // create the new blocks
                Blocks = new GeoBlockCollection(idBytes, dataBytes, maxY);
            }
            catch { return false; }
            return true;
            /* OLD VERSION
            try // consider adding integrity checks on timestamp, chunk values,
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
            }
            catch { return false; } // index out of bounds and so on - we didn't get appropriate data
            return true;
             * */
        }

        /***** PUBLIC READ / PRIVATE WRITE PROPERTIES *******************************/
        public long TimeStamp { get; private set; }
        public int X { get; private set; }
        public int Z { get; private set; }
        public int MaxY { get; private set; }
        public int Version { get; private set; }
        public GeoBiomeArray Biomes { get; private set; }
        public GeoBlockCollection Blocks { get; private set; }

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
        public bool Equals(Substrate.ChunkRef other)
        {
            return this.X == other.X && this.Z == other.Z;
        }

        public bool Equals(IChunkSync other)
        {
            return this.X == other.X && this.Z == other.Z;
        }

        public int CompareTo(IChunkSync other)
        {
            return this.TimeStamp.CompareTo(other.TimeStamp);
        }
        #endregion
    }











}

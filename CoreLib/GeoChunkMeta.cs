
namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Contains the metadata associated with a chunk in a geosharer file.
    /// </summary>
    public class GeoChunkMeta : IChunkSync
    {
        /// <summary>
        /// Create a new GeoChunkMeta object with all of the necessary fields defined
        /// </summary>
        /// <param name="x">Chunk X coordinate</param>
        /// <param name="z">Chunk Z coordinate</param>
        /// <param name="timestamp">Unix time this chunk was last updated by geosharer</param>
        /// <param name="dataStart">Position of first byte of chunk data in decompressed geosharer file</param>
        /// <param name="dataEnd">Position of last byte of chunk data in decompressed geosharer file (-1 if last chunk)</param>
        /// <param name="sourcePath">The file path to the geosharer file this metadata came from</param>
        public GeoChunkMeta(int x, int z, long timestamp, int dataStart, int dataEnd, string sourcePath)
        {
            this.index = new XZIndex(x, z);
            this.timeStamp = timestamp;
            this.dataStart = dataStart;
            this.dataEnd = dataEnd;
            this.sourcePath = sourcePath;
        }

        #region Fields
        private readonly XZIndex index;
        private readonly long timeStamp;
        private readonly int dataStart;
        private readonly int dataEnd;
        private readonly string sourcePath;
        #endregion

        #region Properties
        /// <summary>
        /// Chunk X coordinate
        /// </summary>
        public int X { get { return this.index.X; } }
        /// <summary>
        /// Chunk Z coordinate
        /// </summary>
        public int Z { get { return this.index.Z; } }
        /// <summary>
        /// Unix time this chunk was last updated by geosharer
        /// </summary>
        public long TimeStamp { get { return this.timeStamp; } }
        /// <summary>
        /// Position of first byte of chunk data in decompressed geosharer file
        /// </summary>
        public int DataStart { get { return this.dataStart; } }
        /// <summary>
        /// Position of last byte of chunk data in decompressed geosharer file (-1 if last chunk)
        /// </summary>
        public int DataEnd { get { return this.dataEnd; } }
        /// <summary>
        /// The file path to the geosharer file this metadata came from
        /// </summary>
        public string SourcePath { get { return this.sourcePath; } }
        /// <summary>
        /// XZIndex of this chunk, suitable for hashing and equality comparisons
        /// </summary>
        public XZIndex Index { get { return this.index; } }
        #endregion

        #region Overrides and Implementations
        public override bool Equals(object obj)
        {
            GeoChunkMeta other = obj as GeoChunkMeta;
            return (other != null && this.Equals(other));
        }

        public static bool operator ==(GeoChunkMeta a, GeoChunkMeta b) { return a.Equals(b); }
        public static bool operator !=(GeoChunkMeta a, GeoChunkMeta b) { return !a.Equals(b); }

        public override int GetHashCode()
        {
            return this.index.GetHashCode();
        }
        /// <summary>
        /// Checks equality of IChunkSync objects according to their XZIndex properties
        /// </summary>
        public bool Equals(IChunkSync other)
        {
            return this.index == other.Index;
        }
        /// <summary>
        /// Compares and orders IChunkSync objects according to their TimeStamp properties
        /// </summary>
        public int CompareTo(IChunkSync other)
        {
            return this.timeStamp.CompareTo(other.TimeStamp);
        }
        #endregion
    }
}

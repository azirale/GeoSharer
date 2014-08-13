using System;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Creates a single index value from integer X and Z coordinates, suitable for hashing and dictionaries
    /// </summary>
    public struct XZIndex : IEquatable<XZIndex>
    {
        /// <summary>
        /// Internal store of the combined XZ index as a long
        /// </summary>
        private readonly long indexValue;

        #region Constructors
        /// <summary>
        /// Create a new XZIndex based on the given X and Z coordinates
        /// </summary>
        /// <param name="x">Integer X coordinate to go into the index</param>
        /// <param name="z">Integer Z coordinate to go into the index</param>
        public XZIndex(int x, int z)
        {
            this.indexValue = ((long)(x) << 32) + (long)z;
        }

        /// <summary>
        /// Create a new XZIndex from an existing index value
        /// </summary>
        /// <param name="indexValue"></param>
        private XZIndex(long indexValue)
        {
            this.indexValue = indexValue;
        }
        #endregion

        #region Coordinate properties
        /// <summary>
        /// Get the original integer X value that went into this index
        /// </summary>
        public int X
        {
            get { return (int)(this.indexValue >> 32); }
        }

        /// <summary>
        /// Get the original integer Z value that went into this index
        /// </summary>
        public int Z
        {
            get { return (int)(this.indexValue & 0xFFFF); }
        }
        #endregion

        #region Overrides and Implementations
        public override bool Equals(object obj)
        {
            if (!(obj is XZIndex)) return false;
            return this.Equals((XZIndex)obj);
        }

        public override int GetHashCode()
        {
            return indexValue.GetHashCode();
        }

        public override string ToString()
        {
            return this.X.ToString() + ',' + this.Z.ToString();
        }

        public bool Equals(XZIndex other)
        {
            return this.indexValue == other.indexValue;
        }
        #endregion

        #region Operators
        public static bool operator ==(XZIndex a, XZIndex b) { return a.Equals(b); }
        public static bool operator !=(XZIndex a, XZIndex b) { return !a.Equals(b); }
        #endregion

        #region To and From Byte Array
        /// <summary>
        /// Converts the internal index value into a big endian ordered byte array
        /// </summary>
        /// <returns>A byte array representing the big endian order of the internal index value </returns>
        public byte[] ToByteArray()
        {
            return BitConverter.GetBytes(BitConverter.IsLittleEndian ? this.indexValue : this.indexValue.EndianReverse());
        }

        /// <summary>
        /// Creates a new XZIndex from an existing byte array as would be generated from the ToByteArray method
        /// </summary>
        /// <param name="bytes">Byte array containing the big endian representation of the index value</param>
        /// <param name="offset">Position of the first byte to read</param>
        /// <returns>A new XZIndex based on the provided bytes</returns>
        public static XZIndex FromBytes(byte[] bytes, int offset)
        {
            long indexValue = BitConverter.IsLittleEndian ? BitConverter.ToInt64(bytes, offset) : BitConverter.ToInt64(bytes, offset).EndianReverse();
            return new XZIndex(indexValue);
        }
        #endregion
    }
}

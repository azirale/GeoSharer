using System;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Creates a single index value from integer X and Z coordinates and dimension number, suitable for hashing, equality, and dictionaries
    /// </summary>
    public struct XZDIndex : IEquatable<XZDIndex>
    {
        /// <summary>
        /// Internal store of the combined XZD index as a long
        /// </summary>
        private readonly int hash;
        private readonly int x;
        private readonly int z;
        private readonly int d;

        #region Constructors
        /// <summary>
        /// Create a new XZIndex based on the given X and Z coordinates and dimension number
        /// </summary>
        /// <param name="x">Integer X coordinate to go into the index</param>
        /// <param name="z">Integer Z coordinate to go into the index</param>
        /// <param name="d">Integer dimension number to go into the index</param>
        public XZDIndex(int x, int z, int d)
        {
            this.x = x;
            this.z = z;
            this.d = d;
            this.hash = (x << 3) ^ (z << 3) ^ d;
        }
        #endregion

        #region Coordinate properties
        /// <summary>
        /// Get the X coordinate for this chunk index
        /// </summary>
        public int X
        {
            get { return this.x; }
        }

        /// <summary>
        /// Get the Z coordinate for this chunk index
        /// </summary>
        public int Z
        {
            get { return this.z; }
        }

        /// <summary>
        /// Get the dimension of this chunk index
        /// </summary>
        public int Dimension
        {
            get { return this.d; }
        }
        #endregion

        #region Overrides and Implementations
        public override bool Equals(object obj)
        {
            if (!(obj is XZDIndex)) return false;
            return this.Equals((XZDIndex)obj);
        }

        public override int GetHashCode()
        {
            return this.hash;
        }

        public override string ToString()
        {
            return this.X + ',' + this.Z + " (" + this.Dimension + ")";
        }

        public bool Equals(XZDIndex other)
        {
            return this.x == other.x && this.z == other.z && this.d == other.d;
        }
        #endregion

        #region Operators
        public static bool operator ==(XZDIndex a, XZDIndex b) { return a.Equals(b); }
        public static bool operator !=(XZDIndex a, XZDIndex b) { return !a.Equals(b); }
        #endregion
    }
}

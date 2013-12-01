namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Struct to hold the XYZ coordinates of a block within a chunk
    /// </summary>
    public struct GeoBlockVector
    {
        /***** CONSTRUCTOR MEMBERS **************************************************************/
        #region Constructor Members
        /// <summary>
        /// Create a new GeoBlockVector struct, with the given X, Y, and Z in-chunk coordinates
        /// </summary>
        /// <param name="x">In-chunk X coordinate of block</param>
        /// <param name="y">In-chunk Y coordinate of block</param>
        /// <param name="z">In-chunk Z coordinate of block</param>
        public GeoBlockVector(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        #endregion

        /***** PUBLIC READONLY FIELDS ***********************************************************/
        #region Public Readonly Fields
        /// <summary>
        /// In-chunk X coordinate of block
        /// </summary>
        public readonly byte X;
        /// <summary>
        /// In-chunk Y coordinate of block
        /// </summary>
        public readonly byte Y;
        /// <summary>
        /// In-chunk Z coordinate of block
        /// </summary>
        public readonly byte Z;
        #endregion

    }
}

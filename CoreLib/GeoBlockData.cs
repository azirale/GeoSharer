namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Struct containing the byte values for the ID and Meta data for a block
    /// </summary>
    public struct GeoBlockData
    {
        /***** CONSTRUCTOR MEMBERS **************************************************************/
        #region Constructor Members
        /// <summary>
        /// Create a new GeoBlockData struct with the given ID and Meta values
        /// </summary>
        /// <param name="id"></param>
        /// <param name="meta"></param>
        public GeoBlockData(byte id, byte meta)
        {
            ID = id;
            Meta = meta;
        }
        #endregion

        /***** PUBLIC READONLY FIELDS ***********************************************************/
        #region Public Readonly Fields
        /// <summary>
        /// Block ID value
        /// </summary>
        public readonly byte ID;
        /// <summary>
        /// Block Meta value
        /// </summary>
        public readonly byte Meta;
        #endregion

    }
}

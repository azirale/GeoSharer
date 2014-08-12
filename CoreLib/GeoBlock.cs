namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Represents the data for an individual block from GeoSharer
    /// </summary>
    public class GeoBlock
    {
        /***** CONSTRUCTOR MEMBERS **************************************************/
        #region Constructor Members
        /// <summary>
        /// Create a new GeoBlock object with a given position and data values
        /// </summary>
        /// <param name="position">GeoBlockVector struct representing the X/Y/Z coordinates of the block</param>
        /// <param name="data">GeoBlockData struct holding the ID and metadata values for the block</param>
        public GeoBlock(GeoBlockVector position, GeoBlockData data)
        {
            this.position = position;
            this.data = data;
        }
        #endregion

        /***** PRIVATE FIELDS *******************************************************/
        #region Private Fields
        /// <summary>
        /// Internal storage for the block position in the world
        /// </summary>
        private readonly GeoBlockVector position;

        /// <summary>
        /// Internal storage for the block data - ID and Meta
        /// </summary>
        private readonly GeoBlockData data;

        /// <summary>
        /// X coordinate of the block within the world
        /// </summary>
        public int X { get { return position.X; } }

        /// <summary>
        /// Y coordinate of the block within the world
        /// </summary>
        public int Y { get { return position.Y; } }

        /// <summary>
        /// Z coordinate of the block within the world
        /// </summary>
        public int Z { get { return position.Z; } }

        /// <summary>
        /// ID value for this block
        /// </summary>
        public int ID { get { return data.ID; } }

        /// <summary>
        /// Metadata value for this block
        /// </summary>
        public int Meta { get { return data.Meta; } }
        #endregion
    }
}

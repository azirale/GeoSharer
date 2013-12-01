using System;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Readonly array of biome values for a chunk, with values accessible by X/Z coordinate within a chunk
    /// </summary>
    public class GeoBiomeArray
    {
        /***** CONSTRUCTOR MEMBERS **************************************************/
        #region Constructor Members
        /// <summary>
        /// Create a new GeoBiomeArray object based on a byte array of biome values for a single chunk
        /// </summary>
        /// <param name="biomeValues">Byte array of biome values for a single chunk</param>
        public GeoBiomeArray(byte[] biomeValues)
        {
            if (biomeValues.Length != MC.ChunkArea) this.BiomeValues = null;
            else this.BiomeValues = biomeValues;
        }
        #endregion

        /***** PRIVATE FIELDS *******************************************************/
        #region Private Fields
        /// <summary>
        /// Internal storage of the byte array
        /// </summary>
        private readonly byte[] BiomeValues;
        #endregion

        /***** PUBLIC READONLY PROPERTIES *******************************************/
        #region Public Readonly Properties
        /// <summary>
        /// Get the individual biome value at the X/Z coordinate in a chunk
        /// </summary>
        /// <param name="x">The X coordinate within the chunk</param>
        /// <param name="z">The Z coordinate within the chunk</param>
        /// <returns></returns>
        public byte this[int x, int z] { get { return BiomeValues[x * MC.ChunkWidth + z]; } }
        #endregion

        /***** PUBLIC METHODS *******************************************************/
        #region Public Methods
        /// <summary>
        /// Returns a copy of the byte array internal to this GeoBiomeArray object
        /// </summary>
        /// <returns>Byte array</returns>
        public byte[] CopyArray() { byte[] value = new byte[MC.ChunkArea]; Array.Copy(this.BiomeValues, value, MC.ChunkArea); return value; }
        #endregion
    }
}

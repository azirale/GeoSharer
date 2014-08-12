
namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Contains magic number style values from Minecraft. These probably will not change, but it helps understand certain
    /// parts of the code better if we explicitly mention where the numbers come from or what they relate to
    /// </summary>
    internal static class MC
    {
        /// <summary>
        /// Width of a chunk on the X and Z axis
        /// </summary>
        internal static int ChunkWidth = 16;

        /// <summary>
        /// How tall chunks can be
        /// </summary>
        internal static int ChunkHeight = 256;

        /// <summary>
        /// The XZ area of a chunk (like with a top-down view)
        /// </summary>
        internal static int ChunkArea = ChunkWidth*ChunkWidth;

        /// <summary>
        /// The YXZ volume of a chunk - ie the total number of block positions in a chunk
        /// </summary>
        internal static int ChunkVolume = ChunkArea * ChunkHeight;
    }
}

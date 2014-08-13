

namespace net.azirale.geosharer.core
{
    public static class Endian
    {
        /// <summary>
        /// Deprecated -- use int.EndianReverse
        /// </summary>
        public static int Reverse(int value)
        {
            return System.Net.IPAddress.HostToNetworkOrder(value);
        }

        /// <summary>
        /// Deprecated -- use long.EndianReverse
        /// </summary>
        public static long Reverse(long value)
        {
            return System.Net.IPAddress.HostToNetworkOrder(value);
        }
    }

    public static class CustomExtensions
    {
        /// <summary>
        /// Reverse the endianness of an int
        /// </summary>
        /// <param name="this">The int to reverse</param>
        /// <returns>An int with the reverse endianness of the original value</returns>
        public static int EndianReverse(this int @this) { return System.Net.IPAddress.HostToNetworkOrder(@this); }

        /// <summary>
        /// Reverse the endianness of a long
        /// </summary>
        /// <param name="this">The long to reverse</param>
        /// <returns>A long with the reverse endianness of the original value</returns>
        public static long EndianReverse(this long @this) { return System.Net.IPAddress.HostToNetworkOrder(@this); }

        /// <summary>
        /// Advance the GZipStream forward a number of decomprossed bytes, discarding the data read along the way
        /// </summary>
        /// <param name="this">The GZipStream to advance</param>
        /// <param name="numberOfDecompressedBytes">Number of decompressed bytes by which to advance the stream</param>
        public static void Advance(this System.IO.Compression.GZipStream @this, int numberOfDecompressedBytes)
        {
            const int SmallObjectHeapMaxSize = 85000;
            byte[] dump = new byte[SmallObjectHeapMaxSize];
            while (numberOfDecompressedBytes > SmallObjectHeapMaxSize)
            {
                @this.Read(dump, 0, SmallObjectHeapMaxSize);
                numberOfDecompressedBytes -= SmallObjectHeapMaxSize;
            }
            @this.Read(dump, 0, numberOfDecompressedBytes);
        }
    }
}

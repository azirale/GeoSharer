

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
        #region Bit and Byte conversions for Int and Long
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
        /// Convert an int to a byte array in big endian order
        /// </summary>
        /// <param name="this">The int value to convert into an array</param>
        /// <returns>A new byte array containing the big endian ordered bytes for the given int</returns>
        public static byte[] ToBigEndianByteArray(this int @this)
        {
            byte[] returnValue = System.BitConverter.GetBytes(@this);
            if (System.BitConverter.IsLittleEndian) System.Array.Reverse(returnValue);
            return returnValue;
        }

        /// <summary>
        /// Convert a long to a byte array in big endian order
        /// </summary>
        /// <param name="this">The long value to convert into an array</param>
        /// <returns>A new byte array containing the big endian ordered bytes for the given long</returns>
        public static byte[] ToBigEndianByteArray(this long @this)
        {
            byte[] returnValue = System.BitConverter.GetBytes(@this);
            if (System.BitConverter.IsLittleEndian) System.Array.Reverse(returnValue);
            return returnValue;
        }
        #endregion

        #region GZipStream helpers
        /// <summary>
        /// Advance the GZipStream forward a number of decomprossed bytes, discarding the data read along the way
        /// </summary>
        /// <param name="this">The GZipStream to advance</param>
        /// <param name="numberOfDecompressedBytes">Number of decompressed bytes by which to advance the stream</param>
        public static void Advance(this System.IO.Compression.GZipStream @this, int numberOfDecompressedBytes)
        {
            const int smallObjectHeapMaxSize = 85000;
            byte[] dump = new byte[smallObjectHeapMaxSize];
            while (numberOfDecompressedBytes > smallObjectHeapMaxSize)
            {
                @this.Read(dump, 0, smallObjectHeapMaxSize);
                numberOfDecompressedBytes -= smallObjectHeapMaxSize;
            }
            @this.Read(dump, 0, numberOfDecompressedBytes);
        }

        /// <summary>
        /// Write a byte array to the GZipStream using default values for the offset and count
        /// </summary>
        /// <param name="this">GZipStream to write the data to</param>
        /// <param name="bytes">Byte array to be written to the stream</param>
        public static void Write(this System.IO.Compression.GZipStream @this, byte[] bytes)
        {
            @this.Write(bytes, 0, bytes.Length);
        }

        #endregion
    }
}

using System.Net;

namespace net.azirale.geosharer.core
{
    public static class Endian
    {
        /// <summary>
        /// Deprecated -- use int.EndianReverse
        /// </summary>
        public static int Reverse(int value)
        {
            return IPAddress.HostToNetworkOrder(value);
        }

        /// <summary>
        /// Deprecated -- use long.EndianReverse
        /// </summary>
        public static long Reverse(long value)
        {
            return IPAddress.HostToNetworkOrder(value);
        }
    }

    public static class CustomExtensions
    {
        public static int EndianReverse(this int @this) { return IPAddress.HostToNetworkOrder(@this); }
        public static long EndianReverse(this long @this) { return IPAddress.HostToNetworkOrder(@this); }
    }
}

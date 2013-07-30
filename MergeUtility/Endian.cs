using System.Net;

namespace net.azirale.civcraft.GeoSharer
{
    static class Endian
    {
        public static long Reverse(long value)
        {
            return IPAddress.HostToNetworkOrder(value);
        }

        public static int Reverse(int value)
        {
            return IPAddress.HostToNetworkOrder(value);
        }
    }
}

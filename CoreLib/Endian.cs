using System.Net;

namespace net.azirale.geosharer.core
{
    public static class Endian
    {
        public static int Reverse(int value)
        {
            return IPAddress.HostToNetworkOrder(value);
        }

        public static long Reverse(long value)
        {
            return IPAddress.HostToNetworkOrder(value);
        }
    }
}

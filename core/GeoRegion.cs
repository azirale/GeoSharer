using System;
using System.Collections.Generic;

namespace net.azirale.geosharer.core
{
    public class GeoRegion
    {
        private int x;
        private int z;
        private List<GeoChunkRaw> chunks;

        public GeoRegion(int x, int z)
        {
            this.x = x;
            this.z = z;
            this.chunks = new List<GeoChunkRaw>();
        }

        public int X { get { return this.x; } }
        public int Z { get { return this.z; } }
        public List<GeoChunkRaw> Chunks { get { return this.chunks; } }
    }
}

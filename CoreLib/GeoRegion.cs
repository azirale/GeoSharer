using System;
using System.Collections.Generic;

namespace net.azirale.geosharer.core
{
    public class GeoRegion : IEquatable<GeoRegion>
    {
        private XZDIndex index;
        private List<GeoChunkRaw> chunks;

        public GeoRegion(int x, int z, int d)
        {
            this.index = new XZDIndex(x, z, d);
            this.chunks = new List<GeoChunkRaw>();
        }

        public int X { get { return this.index.X; } }
        public int Z { get { return this.index.Z; } }
        public int Dimension { get { return this.index.Dimension; } }
        public List<GeoChunkRaw> Chunks { get { return this.chunks; } }

        #region IEquality Implementation
        public bool Equals(GeoRegion other)
        {
            return this.index == other.index;
        }

        public override bool Equals(object obj)
        {
            GeoRegion other = obj as GeoRegion;
            if (other == null) return false;
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return index.GetHashCode();
        }

        public static bool operator ==(GeoRegion a, GeoRegion b) { return a.Equals(b); }
        public static bool operator !=(GeoRegion a, GeoRegion b) { return !a.Equals(b); }
        #endregion
    }
}

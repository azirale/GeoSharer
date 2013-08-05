namespace net.azirale.civcraft.GeoSharer
{
    struct GeoBlockVector
    {
        public readonly byte X;
        public readonly byte Y;
        public readonly byte Z;

        public GeoBlockVector(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
